using System;

namespace Apothecary;

[Flags]
public enum ItemFindCondition {
	None = 0,

	Morning = 0x001,
	Afternoon = 0x002,
	Night = 0x004,

	AfterRaining = 0x010,

	InMoonlight = 0x100,
}

public static class ItemFindConditionExtensions {
	public static string TrString(this ItemFindCondition condition) {
		return condition switch {
			ItemFindCondition.None => "ANY_TIME",
			ItemFindCondition.Morning => "MORNING",
			ItemFindCondition.Afternoon => "AFTERNOON",
			ItemFindCondition.Night => "NIGHT",
			ItemFindCondition.AfterRaining => "AFTER_RAINING",
			ItemFindCondition.InMoonlight => "IN_MOONLIGHT",
			_ => throw new ArgumentOutOfRangeException(nameof(condition), condition, null)
		};
	}
}
