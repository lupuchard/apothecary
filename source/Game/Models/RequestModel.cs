using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MessagePack;
using MessagePack.Formatters;

namespace Apothecary;

[MessagePackFormatter(typeof(RequestModelFormatter))]
public class RequestModel {
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
	
	public class RequestModelFormatter : IMessagePackFormatter<RequestModel?> {
		public void Serialize(ref MessagePackWriter writer, RequestModel? value, MessagePackSerializerOptions options) {
			if (value == null) {
				writer.WriteNil();
			} else {
				options.Resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Id, options);
			}
		}

		public RequestModel? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			if (reader.IsNil) {
				return null;
			} else {
				var id = options.Resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				return Game.Instance.World.GetRequest(id) ?? UnknownRequestModel;
			}
		}
	}
}
