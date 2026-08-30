using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace Apothecary;

[JsonConverter(typeof(RegionModelConverter))]
public partial class RegionModel(string id, int max_forage, double forage_recovery, UnlockRequirement unlock_requirement) : RefCounted {
	public string Id { get; } = id;
	public int MaxForage { get; } = max_forage;
	public double ForageRecovery { get; } = forage_recovery;
	public UnlockRequirement UnlockRequirement { get; } = unlock_requirement;

	public static readonly RegionModel UnknownRegionModel = new("unknown", 0, 0, UnlockRequirement.None);
}

public class RegionModelConverter : JsonConverter<RegionModel?> {
	public override RegionModel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		var id = reader.GetString();
		return id == null ? null : Game.Instance.World.GetRegionModel(id);
	}
	public override void Write(Utf8JsonWriter writer, RegionModel? region, JsonSerializerOptions options) {
		writer.WriteStringValue(region?.Id);
	}
}
