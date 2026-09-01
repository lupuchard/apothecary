using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;

namespace Apothecary;

public partial class SlowButton : PanelContainer {
	[Export] public double Duration { get; set; } = 0.5;
	[Export] public AudioStreamPlayer? StartSound { get; set; }
	[Export] public AudioStreamPlayer? FinishSound { get; set; }
	[Export] public bool Small { get; set; } = false;

	private StyleBox? normal_style;
	private StyleBox? hover_style;
	private bool hovering = false;
	private bool pressing = false;

	[Export]
	public string Text { get; set { field = value; Update(); } } = "";

	[Export]
	public bool Disabled {
		get => disabled || !CostsMet();
		set {
			disabled = value;
			if (value) {
				hovering = false;
			}
			Update();
		}
	}
	private bool disabled = false;
	
	[Export]
	public bool Label { get; set { field = value; Update(); } }

	[Export]
	public Godot.Collections.Dictionary<string, int>? Costs {
		get;
		set {
			field = value;
			if (value != null) {
				costs = value.Select(
					entry => (Resources.FromString(entry.Key) ?? Resource.None, entry.Value)
				).OrderBy(x => x.Item1).ToList();
			} else {
				costs = [];
			}
			Update();
		}
	}
	private List<(Resource resource, int amount)> costs = [];

	[Signal] public delegate void PressedEventHandler();
	private ProgressBar? progress_bar;
	private RichTextLabel? label;
	private Tween? tween;

	private bool CostsMet() {
		return Costs == null || costs.All(cost => Game.Instance.GetResource(cost.resource) >= cost.amount);
	}
	
	public override void _Ready() {
		base._Ready();
		
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;

		normal_style = ResourceLoader.Load<StyleBox>("res://assets/theme/ButtonStyleBox.tres");
		hover_style = ResourceLoader.Load<StyleBox>("res://assets/theme/ButtonHoverStyleBox.tres");
		
		label = GetNode<RichTextLabel>("%RichTextLabel");
		progress_bar = GetNode<ProgressBar>("%ProgressBar");
		progress_bar.Value = 0;
		Update();

		if (Small) {
			label.AddThemeFontOverride("font", ResourceLoader.Load<Font>("res://assets/theme/m5x7.ttf"));
		}
	}

	private void OnPressed() {
		pressing = true;
		StartSound?.Play();
		tween?.Kill();
		tween = CreateTween();
		tween.TweenProperty(progress_bar, "value", 1.0, Duration);
		tween.Finished += () => {
			FinishSound?.Play();
			progress_bar?.Value = 0;
			EmitSignalPressed();
			tween = null;
			Update();
			pressing = false;
		};
	}

	private void OnReleased() {
		if (!pressing) return;
		tween?.Kill();
		tween = null;
		progress_bar?.Value = 0;
		pressing = false;
	}

	public void Update() {
		var text = new StringBuilder();
		text.Append(Tr(Text));
		if (Costs != null && !disabled) {
			text.Append(" (");
			text.AppendJoin(", ", costs.Select(
				cost => cost.amount + BbCodeUtil.Img(cost.resource.SmallSpritePath(), cost.resource.GetColor()))
			);
			text.Append(')');
		}
		label?.Text = text.ToString();
		
		Modulate = Disabled ? Colors.Gray : Colors.White;
	}
	
	public override void _Input(InputEvent inputEvent) {
		if (!Disabled && hovering && inputEvent is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) {
			OnPressed();
		}

		if (inputEvent is InputEventMouseButton { Pressed: false, ButtonIndex: MouseButton.Left }) {
			OnReleased();
		}
	}
	
	private void OnMouseEntered() {
		if (Disabled) return;
		hovering = true;
		AddThemeStyleboxOverride("panel", hover_style);
	}

	private void OnMouseExited() {
		hovering = false;
		AddThemeStyleboxOverride("panel", normal_style);
		OnReleased();
	}
}
