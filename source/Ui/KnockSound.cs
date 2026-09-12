using Godot;
namespace Apothecary;

public partial class KnockSound : AudioStreamPlayer2D {
	private Timer timer = new();
	public override void _Ready() {
		Game.Instance.TimeChanged += OnTimeChanged;
		AddChild(timer);
		timer.OneShot = true;
		timer.Timeout += () => Play();
	}

	private void OnTimeChanged() {
		if (Game.Instance.VisitorAtDoor != null) {
			timer.Start(0.5);
		}
	}
}
