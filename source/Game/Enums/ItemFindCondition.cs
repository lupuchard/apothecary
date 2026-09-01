using System;
using System.Collections.Generic;
using System.Linq;

namespace Apothecary;

[Flags]
public enum ItemFindCondition {
	None = 0,
		
	Morning = 0x001,
	Afternoon = 0x002,
	Daytime = 0x004,
	Night = 0x008,

	AfterRaining = 0x010,
	HeatWave = 0x020,
	Wind = 0x040,
	Snowing = 0x080,

	InMoonlight = 0x100,
}

public static class ItemFindConditions {
	
	private static readonly Dictionary<string, ItemFindCondition> condition_name_map = new List<ItemFindCondition> {
		ItemFindCondition.Morning, 
		ItemFindCondition.Afternoon, 
		ItemFindCondition.Daytime, 
		ItemFindCondition.Night,
		ItemFindCondition.AfterRaining,
		ItemFindCondition.HeatWave,
		ItemFindCondition.Wind,
		ItemFindCondition.Snowing,
		ItemFindCondition.InMoonlight
	}.ToDictionary(x => x.TrString(), StringComparer.OrdinalIgnoreCase);
	public static ItemFindCondition? FromString(string name) {
		if (condition_name_map.TryGetValue(name, out var condition)) {
			return condition;
		} else {
			return null;
		}
	}
	
	public static string TrString(this ItemFindCondition condition) {
		return condition switch {
			ItemFindCondition.None => "ANY_TIME",
			ItemFindCondition.Morning => "MORNING",
			ItemFindCondition.Afternoon => "AFTERNOON",
			ItemFindCondition.Daytime => "DAYTIME",
			ItemFindCondition.Night => "NIGHT",
			ItemFindCondition.AfterRaining => "AFTER_RAINING",
			ItemFindCondition.HeatWave => "HEAT_WAVE",
			ItemFindCondition.Wind => "WIND",
			ItemFindCondition.Snowing => "SNOWING",
			ItemFindCondition.InMoonlight => "IN_MOONLIGHT",
			_ => throw new ArgumentOutOfRangeException(nameof(condition), condition, null)
		};
	}
}
