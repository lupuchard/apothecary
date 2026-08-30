using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;

namespace Apothecary;

public readonly struct Item : IEquatable<Item> {
	public ImmutableList<ItemModel> Raw { get; init; }
	public ImmutableList<(Aspect, int)> Aspects { get; init; }
	public ItemType Type { get; init; }

	private Item(ImmutableList<ItemModel> raw, ImmutableList<(Aspect, int)> aspects, ItemType type) {
		Raw = raw;
		Aspects = aspects;
		Type = type;
	}

	public Item(ItemModel item) {
		Raw = [item];
		Aspects = [.. item.Aspects];
		Type = ItemType.Raw;
	}

	public bool Equals(Item other) {
		return Type == other.Type && Raw.SequenceEqual(other.Raw);
	}

	public override bool Equals(object? other) {
		if (other is not Item item) {
			return false;
		}

		return Type == item.Type && Raw.SequenceEqual(item.Raw);
	}
	
	public static bool operator ==(Item lhs, Item rhs) {
		return lhs.Equals(rhs);
	}

	public static bool operator !=(Item lhs, Item rhs) {
		return !lhs.Equals(rhs);
	}

	public static int CompareIndexes(Item item1, Item item2) {
		if (item1.Raw.Count != item2.Raw.Count) {
			return item1.Raw.Count - item2.Raw.Count;
		} else {
			return CompareIndexes(item1.Raw, item2.Raw);
		}
	}

	private static int CompareIndexes(ImmutableList<ItemModel> lhs, ImmutableList<ItemModel> rhs) {
		if (lhs.IsEmpty) {
			return 0;
		} else if (lhs[0].Index == rhs[0].Index) {
			return CompareIndexes(lhs.RemoveAt(0), rhs.RemoveAt(0));
		} else {
			return lhs[0].Index - rhs[0].Index;
		}
	}

	public override int GetHashCode() {
		return Raw.Aggregate(0x2D2816FE, (current, item) => current * 397);
	}

	public string GetName() {
		return Is(ItemType.Infusion) ? "INFUSION" : Raw[0].Id.ToUpperInvariant();
	}

	public Texture2D GetSprite() {
		if (Is(ItemType.Infusion)) {
			return ResourceLoader.Load<Texture2D>("res://assets/item/infusion.png");
		} else {
			return Raw[0].Sprite;
		}
	}

	public bool Is(ItemType type) {
		return (Type & type) != 0;
	}
	
	public static Item Ground(Item item) {
		return new Item(
			item.Raw,
			ModifyAspect(ModifyAspect(item.Aspects, 1, at: 0), -1, at: 1),
			(item.Type & ~ItemType.Raw) | ItemType.Ground
		);
	}

	public static Item Roasted(Item item) {
		return new Item(
			item.Raw,
			MutateAspect(item.Aspects, at: -1),
			(item.Type & ~ItemType.Raw) | ItemType.Roasted
		);
	}

	public static Item Infusion(IList<Item> items) {
		var aspects = CombineAspects(items.Select(item => item.Aspects));
		for (var i = aspects.Count - 1; i >= 0; i--) {
			aspects = ModifyAspect(aspects, -1, at: i);
		}
		return new Item(
			[.. items.SelectMany(item => item.Raw)],
			aspects,
			ItemType.Infusion
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

	public static ImmutableList<(Aspect, int)> CombineAspects(IEnumerable<ImmutableList<(Aspect, int)>> aspects) {
		return aspects.Aggregate<ImmutableList<(Aspect, int)>, ImmutableList<(Aspect, int)>>([], CombineAspects);
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
