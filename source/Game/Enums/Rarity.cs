using System;
using System.Collections.Generic;
using System.Linq;

namespace Apothecary;

public enum Rarity {
	Common,
	Rare,
	Scarce,
	COUNT
}

public static class Rarities {
	private static readonly Dictionary<string, Rarity> rarity_name_map = Enumerable.Range(1, (int)Rarity.COUNT - 1)
		.Select(x => (Rarity)x).ToDictionary(x => x.TrString(), StringComparer.OrdinalIgnoreCase);
	public static Rarity? FromString(string name) {
		if (rarity_name_map.TryGetValue(name, out var rarity)) {
			return rarity;
		} else {
			return null;
		}
	}
	
	public static string TrString(this Rarity rarity) {
		return rarity switch {
			Rarity.Common => "COMMON",
			Rarity.Rare => "RARE",
			Rarity.Scarce => "SCARCE",
			_ => throw new ArgumentOutOfRangeException(nameof(rarity), rarity, null)
		};
	}
}
