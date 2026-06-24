using Godot;

namespace Apothecary;

public partial class RegionSelect : Area2D {
	public enum Type {
		Region,
		Home
	}
	
	[Signal] public delegate void SelectedEventHandler(Type type, RegionModel? region);
	
	[Export] public string? region_id { get; set; }
	[Export] public Polygon2D? fog_poly { get; set; }
	private Type type;
	private Region? region;
	private RegionLabel? region_label;
	private Node? fog_viewport;

	private Tween? hover_tween;
	private bool hovering = false;

	private Tween? hide_fog_tween;
	private bool fog_shown = false;
	
	public override void _Ready() {
		base._Ready();
		
		if (region_id == null) {
			GD.PushError("RegionSelect has no region_id: " + GetPath());
			return;
		}

		if (region_id == "home") {
			type = Type.Home;
		} else {
			region = Game.Instance.GetRegion(region_id);
			if (region == null) {
				GD.PushError("Region '" + region_id + "' not found: " + GetPath());
				return;
			}
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
		
		fog_viewport = GetNode("%FogViewport");
		if (type == Type.Region && fog_poly == null) {
			var collision_poly = GetNode<CollisionPolygon2D>("Polygon");
			fog_poly = new Polygon2D();
			fog_poly.GlobalPosition = collision_poly.GlobalPosition;
			fog_poly.Polygon = collision_poly.Polygon;
			fog_poly.Name = Name;
			fog_poly.Color = Colors.Black;
			fog_viewport.AddChild(fog_poly);
		}

		if (fog_poly != null) {
			var line = new Line2D();
			line.Points = fog_poly.Polygon;
			line.Closed = true;
			line.Width = 128;
			var gradient = new Gradient() {
				Offsets = [0.5f, 1.0f],
				Colors = [Colors.Black, new Color(0, 0, 0, 0)]
			};
			line.Texture = new GradientTexture2D() {
				Gradient = gradient,
				FillFrom = new Vector2(0.5f, 1.0f),
				FillTo = new Vector2(0.5f, 0.0f)
			};
			line.TextureMode = Line2D.LineTextureMode.Tile;
			fog_poly.AddChild(line);
			fog_poly.Hide();
		}
		
		Update();
		Game.Instance.RegionUnlocked += (id) => {
			if (id == region?.Model.Id) {
				Update();
			}
		};
	}

	public void Update() {
		if (region_id != null && region != null && fog_poly != null) {
			var poly_line = fog_poly.GetChild<Line2D>(0);
			if (fog_shown && region.Unlocked) {
				hide_fog_tween?.Kill();
				hide_fog_tween = new Tween();
				hide_fog_tween.TweenProperty(fog_poly, "modulate", new Color(1, 1, 1, 0.5f), 0.5);
				hide_fog_tween.TweenProperty(poly_line, "modulate", Colors.Transparent, 1.0);
				hide_fog_tween.Parallel().TweenProperty(fog_poly, "modulate", Colors.Transparent, 0.5);
				hide_fog_tween.Finished += () => fog_poly.Hide();
				fog_shown = false;
				region_label?.Text = Tr(region_id.ToUpperInvariant());
			} else if (!fog_shown && !region.Unlocked) {
				hide_fog_tween?.Kill();
				fog_poly.Show();
				fog_poly.Modulate = Colors.White;
				poly_line.Modulate = Colors.White;
				fog_shown = true;
				region_label?.Text = Tr(region_id.ToUpperInvariant() + "_UNKNOWN");
			}
		}
	}

	public override void _Input(InputEvent inputEvent) {
		if (hovering && region?.Unlocked != false && inputEvent is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) {
			EmitSignalSelected(type, region?.Model);
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
