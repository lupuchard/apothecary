using System;
using Serde;

namespace Apothecary;

[GenerateSerde]
public enum Rarity {
	Common,
	Rare,
	Scarce,
	COUNT
}

public static class RarityExtensions {
	public static string TrString(this Rarity rarity) {
		return rarity switch {
			Rarity.Common => "COMMON",
			Rarity.Rare => "RARE",
			Rarity.Scarce => "SCARCE",
			_ => throw new ArgumentOutOfRangeException(nameof(rarity), rarity, null)
		};
	}
}
