namespace Apothecary;

public readonly struct UnlockRequirement(UnlockRequirementType Type, int Amount) {
	public UnlockRequirementType Type { get; } = Type;
	public int Amount { get; } = Amount;
	
	public static readonly UnlockRequirement None = new(UnlockRequirementType.None, 0);

	public static UnlockRequirement FulfilledRequests(int amount) {
		return new UnlockRequirement(UnlockRequirementType.FulfilledRequests, amount);
	}

	public static UnlockRequirement ConfirmedJournalEntries(int amount) {
		return new UnlockRequirement(UnlockRequirementType.ConfirmedJournalEntries, amount);
	}
}
