using System;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace Apothecary;

[JsonConverter(typeof(ItemModelConverter))]
public class ItemModel {
	public string Id { get; }
	public int Index { get; set; } = 0;
	public ImmutableArray<(Aspect, int)> Aspects { get; }

	public RegionModel WhereFound { get; }
	public ItemFindCondition WhenFound { get; }
	public Rarity Rarity { get; }
	
	public Texture2D Sprite { get; }

	private static readonly ItemModel UnknownItemModel = new("unknown", [], RegionModel.UnknownRegionModel);

	public ItemModel(
		string id, 
		ImmutableArray<(Aspect, int)> aspects, 
		RegionModel where_found, 
		ItemFindCondition when_found = ItemFindCondition.None, 
		Rarity rarity = Rarity.Common
	) {
		Id = id;
		Aspects = aspects;
		WhereFound = where_found;
		WhenFound = when_found;
		Rarity = rarity;
		
		Sprite = ResourceLoader.Load<Texture2D>($"res://assets/item/{id}.png");
	}
}

public class ItemModelConverter : JsonConverter<ItemModel?> {
	public override ItemModel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		var id = reader.GetString();
		return id == null ? null : Game.Instance.World.GetItemModel(id);
	}
	public override void Write(Utf8JsonWriter writer, ItemModel? item, JsonSerializerOptions options) {
		writer.WriteStringValue(item?.Id);
	}
}
