using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Apothecary;

[JsonConverter(typeof(RequestModelConverter))]
public class RequestModel {
	private readonly struct TextGen(string[] base_texts, Dictionary<int, string[]> fillers) {
		public readonly string[] base_texts = base_texts;
		public readonly Dictionary<int, string[]> fillers = fillers;
	}

	public string Id { get; }
	public VisitorType Type { get; }
	private readonly List<TextGen> text_gens = [];
	public ImmutableArray<(Aspect, int)> Aspects { get; }
	public int Reward { get; }
	
	private static readonly RequestModel UnknownRequestModel = new("unknown", VisitorType.UnknownVisitorType, "", [], 0);

	public RequestModel(string id, VisitorType type, string text_gen, ImmutableArray<(Aspect, int)> aspects, int reward) {
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
		var text = rando.Pick(text_gen.base_texts);
		foreach (var (key, values) in text_gen.fillers) {
			text = text.Replace("{" + key + "}", rando.Pick(values));
		}
		return text;
	}
}

public class RequestModelConverter : JsonConverter<RequestModel?> {
	public override RequestModel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		var id = reader.GetString();
		return id == null ? null : Game.Instance.World.GetRequest(id);
	}
	public override void Write(Utf8JsonWriter writer, RequestModel? region, JsonSerializerOptions options) {
		writer.WriteStringValue(region?.Id);
	}
}
