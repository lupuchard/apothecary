using System;
using Godot;

namespace Apothecary;

public enum Resource {
	None,
	Reputation,
	Coins,
	Firewood,
	COUNT
}

public static class ResourceExtensions {
	extension(Resource resource) {
		public string TrString( ) {
			return resource switch {
				Resource.Reputation => "REPUTATION",
				Resource.Coins => "COINS",
				Resource.Firewood => "FIREWOOD",
				_ => throw new ArgumentOutOfRangeException(nameof(resource), resource, null)
			};
		}

		public Color GetColor() {
			return resource switch {
				Resource.Reputation => Colors.CornflowerBlue,
				Resource.Coins => Colors.Yellow,
				Resource.Firewood => Colors.Brown,
				_ => throw new ArgumentOutOfRangeException(nameof(resource), resource, null)
			};
		}
	}
}
