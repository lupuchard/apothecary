using Godot;

namespace Apothecary;

public partial class RegionLocationSelect : LocationSelect {
	[Export] public required string region_id { get; set; }

	public override void _Ready() {
		if (string.IsNullOrEmpty(region_id)) {
			GD.PushError("LocationSelect has no region_id: " + GetPath());
			return;
		}

		base._Ready();
	}

	public override LocationLabel CreateLocationLabel() {
		var label = new RegionLocationLabel();
		label.Region = Game.Instance.World.GetRegionModel(region_id ?? "");
		return label;
	}

	public override bool Unlocked() {
		return Game.Instance.GetRegion(region_id ?? "")?.Unlocked == true;
	}
}
