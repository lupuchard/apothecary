
using MessagePack;

namespace Apothecary;

[MessagePackObject]
public record JournalEntry(ItemModel Item) {
	[Key("item")] public ItemModel Item { get; } = Item;
	[Key("confirmed")] public bool Confirmed { get; init; }
	[Key("location_guess")] public RegionModel? LocationGuess { get; init; }
	[Key("rarity_guess")] public Rarity? RarityGuess { get; init; }
	[Key("condition_guess")] public ItemFindCondition? ConditionGuess { get; init; }
	[Key("notes")] public string? Notes { get; init; }
}
