using System;

namespace Apothecary;

public enum Season {
	Prevernal, Vernal, Estival, Serotinal, Autumnal, Hibernal
}

public static class SeasonExtensions {
	extension(Season season) {
		public string TrString() {
			return season switch {
				Season.Prevernal => "PREVERNAL",
				Season.Vernal => "VERNAL",
				Season.Estival => "ESTIVAL",
				Season.Serotinal => "SEROTINAL",
				Season.Autumnal => "AUTUMNAL",
				Season.Hibernal => "HIBERNAL",
				_ => throw new ArgumentOutOfRangeException(nameof(season), season, null)
			};
		}

		public string TrTimeOfDay(int time_of_day) {
			return season switch {
				Season.Estival => time_of_day switch {
					0 => "EARLY_MORNING",
					1 => "MID_MORNING",
					2 => "LATE_MORNING",
					3 => "EARLY_AFTERNOON",
					4 => "MID_AFTERNOON",
					5 => "LATE_AFTERNOON",
					_ => "NIGHT"
				},
				Season.Serotinal or Season.Vernal => time_of_day switch {
					0 => "EARLY_MORNING",
					1 => "LATE_MORNING",
					2 => "NOON",
					3 => "EARLY_AFTERNOON",
					4 => "MID_AFTERNOON",
					5 => "EVENING",
					_ => "NIGHT"
				},
				Season.Autumnal or Season.Prevernal => time_of_day switch {
					0 => "MORNING",
					1 => "LATE_MORNING",
					2 => "EARLY_AFTERNOON",
					3 => "MID_AFTERNOON",
					4 => "EVENING",
					_ => "NIGHT"
				},
				Season.Hibernal => time_of_day switch {
					0 => "MORNING",
					1 => "MIDDAY",
					2 => "AFTERNOON",
					3 => "NIGHTFALL",
					4 => "EVENING",
					_ => "NIGHT"
				},
				_ => throw new ArgumentOutOfRangeException(nameof(season), season, null)
			};
		}
	}
}
