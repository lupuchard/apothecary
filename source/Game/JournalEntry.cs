using Serde;

namespace Apothecary;

[GenerateSerde]
public partial record JournalEntry(ItemModel Item) {
	public ItemModel Item { get; } = Item;
	public bool Confirmed { get; init; }
	public RegionModel? LocationGuess { get; init; }
	public Rarity? RarityGuess { get; init; }
	public ItemFindCondition? ConditionGuess { get; init; }
	public string? Notes { get; init; }
}
