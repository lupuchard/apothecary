using System.Collections.Immutable;
using Godot;
using Serde;

namespace Apothecary;

[GenerateSerde(With = typeof(ItemModelSerdeObj))]
public partial class ItemModel {
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

public class ItemModelSerdeObj : ISerde<ItemModel?> {
	public ISerdeInfo SerdeInfo { get; } = StringProxy.SerdeInfo.WithName("ItemModel");

	public void Serialize(ItemModel? item, ISerializer serializer) {
		if (item == null) {
			serializer.WriteNull();
		} else {
			serializer.WriteString(item.Id);
		}
	}

	public ItemModel? Deserialize(IDeserializer deserializer) {
		return deserializer.TryReadNull() ? null : Game.Instance.World.GetItemModel(deserializer.ReadString());
	}
}
