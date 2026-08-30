using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Apothecary;

public class DictionaryToListConverter<TKey, TValue> : JsonConverter<Dictionary<TKey, TValue>> where TKey : notnull {
	private struct Pair {
		public TKey Key { get; set; }
		public TValue Value { get; set; }
	}

	public override Dictionary<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		// Deserialize the list of pairs back into a dictionary
		var list = JsonSerializer.Deserialize<List<Pair>>(ref reader, options);
		return list?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
	}

	public override void Write(Utf8JsonWriter writer, Dictionary<TKey, TValue> value, JsonSerializerOptions options) {
		// Serialize the dictionary as a list of key-value pairs
		var list = value.Select(kvp => new Pair { Key = kvp.Key, Value = kvp.Value }).ToList();
		JsonSerializer.Serialize(writer, list, options);
	}
}
