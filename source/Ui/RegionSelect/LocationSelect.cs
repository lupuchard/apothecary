using Godot;

namespace Apothecary;

public abstract partial class LocationSelect : Area2D {
	[Signal] public delegate void SelectedEventHandler();
	
	[Export] public Polygon2D? fog_poly { get; set; }
	private LocationLabel? location_label;
	private Node? fog_viewport;
	
	public bool Alert { get; set; }

	private Tween? hover_tween;
	private bool hovering = false;

	private Tween? hide_fog_tween;
	private bool fog_shown = false;
	
	public override void _Ready() {
		base._Ready();

		var location_labels = GetNode<Control>("%RegionLabels");
		location_label = (LocationLabel?)location_labels.FindChild(Name, recursive: false);
		if (location_label == null) {
			location_label = CreateLocationLabel();
			location_labels.AddChild(location_label);
		}

		location_label.Follows = this;
		location_label.Modulate = Colors.Transparent;

		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
		
		fog_viewport = GetNode("%FogViewport");
		if (fog_poly == null) {
			var collision_poly = GetNode<CollisionPolygon2D>("Polygon");
			fog_poly = new Polygon2D();
			fog_poly.GlobalPosition = collision_poly.GlobalPosition;
			fog_poly.Polygon = collision_poly.Polygon;
			fog_poly.Name = Name;
			fog_poly.Color = Colors.Black;
			fog_viewport.AddChild(fog_poly);
		}

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
		
		Update();
		Game.Instance.RegionUnlocked += _ => {
			Update();
		};
	}

	public abstract LocationLabel CreateLocationLabel();
	public abstract bool Unlocked();

	public void Update(bool instant = false) {
		if (fog_poly == null) return;
		
		var poly_line = fog_poly.GetChild<Line2D>(0);
		var unlocked = Unlocked();
		if (fog_shown && unlocked) {
			hide_fog_tween?.Kill();
			if (instant) {
				fog_poly.Hide();
			} else {
				hide_fog_tween = CreateTween();
				hide_fog_tween.TweenProperty(fog_poly, "modulate", new Color(1, 1, 1, 0.5f), 0.5);
				hide_fog_tween.TweenProperty(poly_line, "modulate", Colors.Transparent, 1.0);
				hide_fog_tween.Parallel().TweenProperty(fog_poly, "modulate", Colors.Transparent, 0.5);
				hide_fog_tween.Finished += () => fog_poly.Hide();
			}
			fog_shown = false;
			Alert = true;
		} else if (!fog_shown && !unlocked) {
			hide_fog_tween?.Kill();
			fog_poly.Show();
			fog_poly.Modulate = Colors.White;
			poly_line.Modulate = Colors.White;
			fog_shown = true;
		}

		location_label?.Alert = Alert;
		location_label?.Update();
	}

	public override void _Input(InputEvent inputEvent) {
		if (hovering && Unlocked() && inputEvent is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) {
			Alert = false;
			Update();
			EmitSignalSelected();
		}
	}

	private void OnMouseEntered() {
		hover_tween?.Kill();
		hover_tween = CreateTween();
		hover_tween.TweenProperty(location_label, "modulate", Colors.White, 0.2);
		hovering = true;
	}

	private void OnMouseExited() {
		hover_tween?.Kill();
		hover_tween = CreateTween();
		hover_tween.TweenProperty(location_label, "modulate", Colors.Transparent, 0.2);
		hovering = false;
	}
}
