using System.Collections.Immutable;

namespace Apothecary;

public record JournalEntry(ItemModel Item) {
	public ItemModel Item { get; } = Item;
	public bool Confirmed { get; init; }
	public RegionModel? LocationGuess { get; init; }
	public Rarity? RarityGuess { get; init; }
	public ItemFindCondition? ConditionGuess { get; init; }
	public string? Notes { get; init; }
	public ImmutableList<ItemObservation> Observations { get; init; } = [];
}

public record ItemObservation(RegionModel Region, int Amount, Season Season, int TimeOfDay, ItemFindCondition Weather);
