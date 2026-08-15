using System;

namespace Apothecary;

public enum Season {
	Prevernal, Vernal, Estival, Serotinal, Autumnal, Hibernal
}

public static class SeasonExtensions {
	extension(Season season) {
		public string TrString( ) {
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
	}
}
