using System.Collections.Immutable;
using Godot;
using MessagePack;
using MessagePack.Formatters;

namespace Apothecary;

[MessagePackFormatter(typeof(ItemModelFormatter))]
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
	
	public class ItemModelFormatter : IMessagePackFormatter<ItemModel?> {
		public void Serialize(ref MessagePackWriter writer, ItemModel? value, MessagePackSerializerOptions options) {
			if (value == null) {
				writer.WriteNil();
			} else {
				options.Resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Id, options);
			}
		}

		public ItemModel? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			if (reader.IsNil) {
				return null;
			} else {
				var id = options.Resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				return Game.Instance.World.GetItemModel(id) ?? UnknownItemModel;
			}
		}
	}
}
