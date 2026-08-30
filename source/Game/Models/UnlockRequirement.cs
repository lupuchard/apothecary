namespace Apothecary;

public readonly struct UnlockRequirement(UnlockRequirementType Type, int Amount, Resource Resource = Resource.None) {
	public UnlockRequirementType Type { get; } = Type;
	public Resource Resource { get; } = Resource;
	public int Amount { get; } = Amount;
	
	public static readonly UnlockRequirement None = new(UnlockRequirementType.None, 0);

	public static UnlockRequirement ResourceAcquired(Resource resource, int amount) {
		return new UnlockRequirement(UnlockRequirementType.ResourceAcquired, amount, resource);
	}

	public static UnlockRequirement ConfirmedJournalEntries(int amount) {
		return new UnlockRequirement(UnlockRequirementType.ConfirmedJournalEntries, amount);
	}

	public static UnlockRequirement Day(int day) {
		return new UnlockRequirement(UnlockRequirementType.Day, day);
	}
}
