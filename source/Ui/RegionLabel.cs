using Godot;

namespace Apothecary;

public partial class RegionLabel : Label {
	[Export] public Node2D? Follows;

	public override void _Ready() {
		base._Ready();
		if (Follows == null) {
			GD.PushError("No follows for: " + GetPath());
		}
		AddThemeColorOverride("font_color", Colors.Black);
	}

	public override void _Process(double _delta) {
		if (Follows == null) return;
		GlobalPosition = Follows.GetGlobalTransformWithCanvas().Origin - Size / 2;
	}
}
