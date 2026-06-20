
namespace Apothecary;

public record JournalEntry(ItemModel Item) {
	public ItemModel Item { get; } = Item;
	public bool Confirmed { get; init; } = false;
	public RegionModel? LocationGuess { get; init; } = null;
	public Rarity? RarityGuess { get; init; } = null;
	public ItemFindCondition? ConditionGuess { get; init; } = null;
}
