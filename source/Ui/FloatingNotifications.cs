using System.Collections.Generic;
using Godot;

namespace Apothecary;

public partial class FloatingNotifications : Control {
	public static FloatingNotifications? Instance { get; private set; }
	
	private const float ASCEND_SPEED = 10.0f;
	private const float START_FADE = 4.0f;
	private const float END_FADE = 5.0f;
	
	private double current_time = 0.0;
	private readonly List<(RichTextLabel, double?)> notifications = [];
	private readonly List<int> available = [];

	public override void _Ready() {
		Instance = this;
	}

	public override void _Process(double delta) {
		current_time += delta;

		for (var i = 0; i < notifications.Count; i++) {
			var (label, starting_time) = notifications[i];
			if (starting_time == null) continue;

			var age = (float)(current_time - starting_time.Value);
			if (age > END_FADE) {
				notifications[i] = (label, null);
				available.Add(i);
			} else {
				label.Position += new Vector2(0, -(float)delta * ASCEND_SPEED);
				if (age > START_FADE) {
					label.Modulate = new Color(1, 1, 1, float.Lerp(1, 0, (age - START_FADE) / (END_FADE - START_FADE)));
				}
			}
		}
	}

	public void Create(string text, Vector2 position) {
		int idx;
		if (available.Count == 0) {
			var new_label = new RichTextLabel();
			new_label.BbcodeEnabled = true;
			new_label.AddThemeColorOverride("default_color", Colors.Black);
			new_label.FitContent = true;
			new_label.AutowrapMode = TextServer.AutowrapMode.Off;
			AddChild(new_label);
			notifications.Add((new_label, null));
			idx = notifications.Count - 1;
		} else {
			idx = available[^1];
			available.RemoveAt(available.Count - 1);
		}

		var label = notifications[idx].Item1;
		notifications[idx] = (label, current_time);
		label.Text = text;
		label.GlobalPosition = position;
		label.Modulate = Colors.White;
	}
}
