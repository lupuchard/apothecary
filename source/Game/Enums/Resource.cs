using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;

namespace Apothecary;

public enum Resource {
	None,
	Time,
	Reputation,
	Coins,
	Firewood,
	Stamina,
	StaminaMax,
	Focus,
	FocusMax,
	COUNT
}

public static class Resources {
	public static readonly ImmutableArray<Resource> Materials = [
		Resource.Reputation, 
		Resource.Coins, 
		Resource.Firewood
	];

	private static readonly Dictionary<string, Resource> resource_name_map = Enumerable.Range(1, (int)Resource.COUNT - 1)
		.Select(x => (Resource)x).ToDictionary(x => x.TrString(), StringComparer.OrdinalIgnoreCase);
	public static Resource? FromString(string name) {
		if (resource_name_map.TryGetValue(name, out var resource)) {
			return resource;
		} else {
			return null;
		}
	}
	
	extension(Resource resource) {
		public string TrString(bool plural = false) {
			return resource switch {
				Resource.Time => "TIME",
				Resource.Reputation => "REPUTATION",
				Resource.Coins => plural ? "COIN" : "COINS",
				Resource.Firewood => "FIREWOOD",
				Resource.Stamina => "STAMINA",
				Resource.StaminaMax => "STAMINA_MAX",
				Resource.Focus => "FOCUS",
				Resource.FocusMax => "FOCUS_MAX",
				_ => "NONE",
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
				Resource.Coins => "SPENT",
				Resource.Firewood => "USED",
				_ => "LOST",
			};
		}

		public Color GetColor() {
			return resource switch {
				Resource.Reputation => Colors.CornflowerBlue,
				Resource.Coins => Colors.Yellow,
				Resource.Firewood => Colors.DarkOrange,
				Resource.Stamina => Colors.LightGreen,
				Resource.Focus => Colors.Cyan,
				_ => Colors.White,
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
