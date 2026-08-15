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
		public string TrString() {
			return resource switch {
				Resource.Reputation => "REPUTATION",
				Resource.Coins => "COINS",
				Resource.Firewood => "FIREWOOD",
				_ => throw new ArgumentOutOfRangeException(nameof(resource), resource, null)
			};
		}

		public string GainTrString() {
			return resource switch {
				Resource.Coins => "EARNED",
				_ => "GAINED",
			};
		}
		
		public string LostTrString() {
			return resource switch {
				_ => "LOST",
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

		public string SpritePath() {
			return $"res://assets/resources/{resource.TrString().ToLowerInvariant()}.png";
		}

		public string SmallSpritePath() {
			return $"res://assets/resources/{resource.TrString().ToLowerInvariant()}_small.png";
		}
	}
}
