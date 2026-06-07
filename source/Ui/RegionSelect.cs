using Godot;

namespace Apothecary;

public partial class RegionSelect : Area2D {
	[Signal] public delegate void SelectedEventHandler(RegionModel region);
	
	[Export] public string? region_id { get; set; }
	private RegionModel? region;
	private RegionLabel? region_label;

	private Tween? hover_tween;
	private bool hovering = false;
	
	public override void _Ready() {
		base._Ready();
		
		if (region_id == null) {
			GD.PushError("RegionSelect has no region_id: " + GetPath());
			return;
		}

		region = Game.Instance.World.GetRegionModel(region_id);
		if (region == null) {
			GD.PushError("Region '" + region_id + "' not found: " + GetPath());
			return;
		}

		var region_labels = GetNode<Control>("%RegionLabels");
		region_label = (RegionLabel?)region_labels.FindChild(Name, recursive: false);
		if (region_label == null) {
			region_label = new RegionLabel();
			region_labels.AddChild(region_label);
		}

		region_label.Follows = this;
		region_label.Text = Tr(region_id.ToUpperInvariant());
		region_label.Modulate = Colors.Transparent;

		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
	}

	public override void _Input(InputEvent inputEvent) {
		if (hovering && inputEvent is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) {
			EmitSignalSelected(region);
		}
	}

	private void OnMouseEntered() {
		hover_tween?.Kill();
		hover_tween = CreateTween();
		hover_tween.TweenProperty(region_label, "modulate", Colors.White, 0.2);
		hovering = true;
	}

	private void OnMouseExited() {
		hover_tween?.Kill();
		hover_tween = CreateTween();
		hover_tween.TweenProperty(region_label, "modulate", Colors.Transparent, 0.2);
		hovering = false;
	}
}
