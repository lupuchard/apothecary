namespace Apothecary;

public enum SpecialRequest {
	None,
	Bills
}

public static class SpecialRequests {
	extension(SpecialRequest resource) {
		public string TrString() {
			return resource switch {
				SpecialRequest.Bills => "BILLS",
				_ => "NONE",
			};
		}
		
		public string SpritePath() {
			return $"res://assets/visitor/{resource.TrString().ToLowerInvariant()}.png";
		}

		public string SmallSpritePath() {
			return $"res://assets/visitor/{resource.TrString().ToLowerInvariant()}_small.png";
		}
	}
}
