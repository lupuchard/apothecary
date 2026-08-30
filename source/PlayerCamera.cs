using Godot;

namespace Apothecary;

public partial class PlayerCamera : Camera2D {
	private readonly Vector2 ZOOM_MAX = new(1.0f, 1.0f);
	private readonly Vector2 ZOOM_MIN = new(0.5f, 0.5f);
	private const float PAN_SPEED = 6.0f;

	public bool CanPan { get; set; } = true;
	private bool panning = false;

	[Export] public Sprite2D? Map { get; set; }

	public override void _Ready() {
		base._Ready();

		if (Map != null) {
			var rect = Map.GetRect();
			LimitEnabled = true;
			LimitSmoothed = true;
			LimitLeft = (int)rect.Position.X;
			LimitRight = (int)rect.End.X;
			LimitTop = (int)rect.Position.Y;
			LimitBottom = (int)rect.End.Y;
		}
	}

	public override void _Input(InputEvent inputEvent) {
		base._Input(inputEvent);
		
		if (inputEvent.IsActionPressed("pan")) {
			panning = true;
		} else if (inputEvent.IsActionReleased("pan")) {
			panning = false;
		}

		if (inputEvent is InputEventMouseMotion motionEvent && panning) {
			Position = GetTargetPosition() - motionEvent.Relative;
		}

		if (!CanPan) return;
		if (inputEvent.IsActionPressed("zoom_in")) {
			Zoom = (Zoom + new Vector2(0.1f, 0.1f)).Min(ZOOM_MAX);
		} else if (inputEvent.IsActionPressed("zoom_out")) {
			Zoom = (Zoom - new Vector2(0.1f, 0.1f)).Max(ZOOM_MIN);
		}
	}

	public override void _Process(double delta) {
		if (!CanPan) return;
		var Motion = Vector2.Zero;
		if (Input.IsActionPressed("pan_left")) {
			Motion += new Vector2(-PAN_SPEED, 0.0f);
		} else if (Input.IsActionPressed("pan_right")) {
			Motion += new Vector2(PAN_SPEED, 0.0f);
		}
		if (Input.IsActionPressed("pan_up")) {
			Motion += new Vector2(0.0f, -PAN_SPEED);
		} else if (Input.IsActionPressed("pan_down")) {
			Motion += new Vector2(0.0f, PAN_SPEED);
		}

		if (Motion != Vector2.Zero) {
			Position = GetTargetPosition() + Motion;
		}
	}
}
