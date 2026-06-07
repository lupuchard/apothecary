using System.Collections.Immutable;
using Godot;

namespace Apothecary;

public class ItemModel {
	public string Id { get; }
	public ImmutableArray<(Aspect, int)> Aspects { get; }

	public RegionModel? WhereFound { get; }
	public ItemFindCondition? WhenFound { get; }
	public Rarity Rarity { get; }
	
	public Texture2D Sprite { get; }

	public ItemModel(string id, ImmutableArray<(Aspect, int)> aspects, RegionModel? where_found = null, ItemFindCondition? when_found = null, Rarity rarity = Rarity.Common) {
		Id = id;
		Aspects = aspects;
		WhereFound = where_found;
		WhenFound = when_found;
		Rarity = rarity;
		
		Sprite = ResourceLoader.Load<Texture2D>($"res://assets/item/{id}.png");
	}
}
