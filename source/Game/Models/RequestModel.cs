using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Serde;

namespace Apothecary;

[GenerateSerde(With = typeof(RequestModelSerdeObj))]
public partial class RequestModel {
	private readonly struct TextGen(string[] base_texts, Dictionary<int, string[]> fillers) {
		public readonly string[] base_texts = base_texts;
		public readonly Dictionary<int, string[]> fillers = fillers;
	}

	public string Id { get; }
	public VisitorType Type { get; }
	private readonly List<TextGen> text_gens = [];
	public ImmutableList<(Aspect, int)> Aspects { get; }
	public int Reward { get; }
	
	private static readonly RequestModel UnknownRequestModel = new("unknown", VisitorType.UnknownVisitorType, "", [], 0);

	public RequestModel(string id, VisitorType type, string text_gen, ImmutableList<(Aspect, int)> aspects, int reward) {
		Id = id;
		Type = type;
		ParseTextGen(text_gen);
		Aspects = aspects;
		Reward = reward;
	}

	private void ParseTextGen(string text_gen) {
		foreach (var gen in text_gen.Trim().Split('[').Skip(1)) {
			var parts = gen.Trim().TrimEnd(']').Split(" % ").Select(part => part.Trim().Split('|').Select(x => x.Trim()));
			string[] base_texts = [..parts.First()];
			var fillers = parts.Skip(1).Select((part, i) => (part.ToArray(), i)).ToDictionary(x => x.i, x => x.Item1);
			text_gens.Add(new TextGen(base_texts, fillers));
		}
	}

	public string GenText(ref Rando rando) {
		var text_gen = rando.Pick(text_gens);
		string text = rando.Pick(text_gen.base_texts);
		foreach (var (key, values) in text_gen.fillers) {
			text = text.Replace("{" + key + "}", rando.Pick(values));
		}
		return text;
	}
}

public class RequestModelSerdeObj : ISerde<RequestModel?> {
	public ISerdeInfo SerdeInfo { get; } = StringProxy.SerdeInfo.WithName("RequestModel");

	public void Serialize(RequestModel? request, ISerializer serializer) {
		if (request == null) {
			serializer.WriteNull();
		} else {
			serializer.WriteString(request.Id);
		}
	}

	public RequestModel? Deserialize(IDeserializer deserializer) {
		return deserializer.TryReadNull() ? null : Game.Instance.World.GetRequest(deserializer.ReadString());
	}
}
