using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Apothecary;

public readonly record struct Item {
	public readonly ImmutableList<ItemModel> Raw;
	public readonly ImmutableList<(Aspect, int)> Aspects;
	public readonly ImmutableHashSet<ItemType> Type;

	private Item(ImmutableList<ItemModel> raw, ImmutableList<(Aspect, int)> aspects, ImmutableHashSet<ItemType> type) {
		Raw = raw;
		Aspects = aspects;
		Type = type;
	}

	public Item(ItemModel item) {
		Raw = [item];
		Aspects = item.Aspects.ToImmutableList();
		Type = [ItemType.Raw];
	}
	
	public static Item Ground(Item item) {
		return new Item(
			item.Raw,
			ModifyAspect(ModifyAspect(item.Aspects, 1, at: 0), -1, at: 1),
			item.Type.Remove(ItemType.Raw).Add(ItemType.Ground)
		);
	}

	public static Item Roasted(Item item) {
		return new Item(
			item.Raw,
			MutateAspect(item.Aspects, at: -1),
			item.Type.Remove(ItemType.Raw).Add(ItemType.Roasted)
		);
	}

	public static Item Infusion(IList<Item> items) {
		var aspects = items.Aggregate<Item, ImmutableList<(Aspect, int)>>([], (aspects, item) => CombineAspects(item.Aspects, aspects));
		for (var i = aspects.Count - 1; i >= 0; i--) {
			aspects = ModifyAspect(aspects, -1, at: i);
		}
		return new Item(
			[.. items.SelectMany(item => item.Raw)],
			aspects,
			[ItemType.Infusion]
		);
	}
	
	private static ImmutableList<(Aspect, int)> ModifyAspect(ImmutableList<(Aspect, int)> aspects, int amount, int at) {
		if (GetIndex(at, aspects.Count) is int index) {
			var (aspect, old_amount) = aspects[index];
			if (old_amount + amount <= 0) {
				aspects = aspects.RemoveAt(index);
			} else {
				aspects = aspects.SetItem(index, (aspect, old_amount + amount));
			}
		}
		return aspects;
	}

	private static ImmutableList<(Aspect, int)> MutateAspect(ImmutableList<(Aspect, int)> aspects, int at) {
		if (GetIndex(at, aspects.Count) is int index) {
			var (aspect, amount) = aspects[index];
			if (aspect.MutatesInto is Aspect new_aspect) {
				aspects = aspects.SetItem(index, (new_aspect, amount));
			}
		}
		return aspects;
	}

	private static ImmutableList<(Aspect, int)> CombineAspects(ImmutableList<(Aspect, int)> aspects1, ImmutableList<(Aspect, int)> aspects2) {
		var aspects = aspects1;
		foreach (var (aspect, amount) in aspects2) {
			var index = aspects.FindIndex(x => x.Item1 == aspect);
			if (index != -1) {
				aspects = aspects.SetItem(index, (aspect, aspects[index].Item2 + amount));
			} else {
				aspects = aspects.Add((aspect, amount));
			}
		}
		return aspects;
	}
	
	private static int? GetIndex(int pos, int list_length) {
		pos = pos < 0 ? list_length + pos : pos;
		return (pos < 0 || pos >= list_length) ? null : pos;
	}
}
