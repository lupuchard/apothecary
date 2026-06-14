using Godot;

namespace Apothecary;

public partial class SlowButton : Button {
	[Export] public double Duration { get; set; } = 0.5;
	
	[Signal] public delegate void PressFinishedEventHandler();
	private ProgressBar? progress_bar;
	private Tween? tween;
	
	public override void _Ready() {
		base._Ready();
		progress_bar = GetNode<ProgressBar>("%ProgressBar");
		ButtonDown += OnPressed;
		ButtonUp += OnReleased;
	}

	private void OnPressed() {
		tween?.Kill();
		tween = CreateTween();
		tween.TweenProperty(progress_bar, "value", 1.0, Duration);
		tween.Finished += () => {
			progress_bar?.Value = 0;
			EmitSignalPressFinished();
			tween = null;
		};
	}

	private void OnReleased() {
		tween?.Kill();
		tween = null;
		progress_bar?.Value = 0;
	}
}
