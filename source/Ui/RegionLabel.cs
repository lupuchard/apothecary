using System;
using Godot;

namespace Apothecary;

public partial class RegionLabel : Label {
	[Export] public Node2D? Follows;
	public Region? Region { get; set; }
	private Vector2 SCREEN_MARGIN = new(10, 10);
	private Label? unlocking_label;

	public override void _Ready() {
		base._Ready();
		if (Follows == null) {
			GD.PushError("No follows for: " + GetPath());
		}
		AddThemeColorOverride("font_color", Colors.Black);

		unlocking_label = new Label();
		unlocking_label.AddThemeColorOverride("font_color", Colors.Black);
		unlocking_label.AddThemeFontOverride("font", ResourceLoader.Load<Font>("res://assets/theme/m5x7.ttf"));
		AddChild(unlocking_label);
	}

	public override void _Process(double _delta) {
		if (Follows == null || unlocking_label == null) return;
		var position = Follows.GetGlobalTransformWithCanvas().Origin;

		//var rect = ((PlayerCamera)GetViewport().GetCamera2D()).Rect();
		GlobalPosition = (position - Size / 2).Clamp(Vector2.Zero + SCREEN_MARGIN, GetViewportRect().Size - Size - SCREEN_MARGIN);
		unlocking_label?.GlobalPosition = position + new Vector2(-unlocking_label!.Size.X / 2, Size.Y / 2);
	}

	public void Update() {
		if (Region == null) return;
		Text = Tr(Region.TrString());
		if (Region.Unlocked) {
			unlocking_label?.Hide();
		} else {
			unlocking_label?.Show();
			unlocking_label?.Text = GetUnlockingDescription();
		}
	}

	private string GetUnlockingDescription() {
		var req = Region?.Model.UnlockRequirement ?? UnlockRequirement.None;
		var game = Game.Instance;
		return req.Type switch {
			UnlockRequirementType.None 
				=> "",
			UnlockRequirementType.Day 
				=> string.Format(Tr("UNLOCKED_ON_DAY"), req.Amount + 1),
			UnlockRequirementType.ResourceAcquired 
				=> string.Format(Tr("UNLOCKED_ON_RESOURCE"), req.Amount, Tr(req.Resource.TrString(req.Amount != 1)).ToLower(), game.GetResource(req.Resource)),
			UnlockRequirementType.ConfirmedJournalEntries 
				=> string.Format(Tr("UNLOCKED_ON_JOURNAL"), req.Amount, game.Journal.TotalConfirmed),
			_ => throw new ArgumentOutOfRangeException()
		};
	}
}
