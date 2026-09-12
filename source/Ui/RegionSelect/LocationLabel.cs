using Godot;

namespace Apothecary;

public abstract partial class LocationLabel : RichTextLabel {
	[Export] public Node2D? Follows;
	public bool Alert { get; set; }
	private Vector2 SCREEN_MARGIN = new(10, 10);
	private Label subtitle_label = new();

	public override void _Ready() {
		base._Ready();
		BbcodeEnabled = true;
		FitContent = true;
		AutowrapMode = TextServer.AutowrapMode.Off;
		ClipContents = false;
		
		if (Follows == null) {
			GD.PushError("No follows for: " + GetPath());
		}
		AddThemeColorOverride("default_color", Colors.Black);

		subtitle_label.AddThemeColorOverride("font_color", Colors.Black);
		subtitle_label.AddThemeFontOverride("font", ResourceLoader.Load<Font>("res://assets/theme/m5x7.ttf"));
		AddChild(subtitle_label);
	}

	public override void _Process(double _delta) {
		if (Follows == null) return;
		var position = Follows.GetGlobalTransformWithCanvas().Origin;

		GlobalPosition = (position - Size / 2).Clamp(Vector2.Zero + SCREEN_MARGIN, GetViewportRect().Size - Size - SCREEN_MARGIN);
		subtitle_label.GlobalPosition = position + new Vector2(-subtitle_label.Size.X / 2, Size.Y / 2);
	}

	public void Update() {
		Text = GetTitleText();
		if (Alert) Text = BbCodeUtil.Img("res://assets/alert_icon.png", Colors.Black) + Text;
		subtitle_label.Text = GetSubtitleText();
	}

	public abstract string GetTitleText();
	public virtual string GetSubtitleText() => "";
}
