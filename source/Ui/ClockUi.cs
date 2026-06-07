using Godot;
using System;
using Apothecary;

namespace Apothecary;

public partial class ClockUi : Control {
	private const float ATLAS_OFFSET = 64.0f;
	private const float ATLAS_CELL_SIZE = 64.0f;
		
	private TextureRect? dial_texture;
	private TextureRect? step_texture;

	public override void _Ready() {
		dial_texture = GetNode<TextureRect>("DialTexture");
		step_texture = GetNode<TextureRect>("StepTexture");

		Game.Instance.TimeChanged += Update;
	}

	public void Update() {
		if (step_texture == null) {
			return;
		}

		var texture = (AtlasTexture)step_texture.Texture;
		var rect = texture.Region;
		rect.Position = new Vector2(0, Game.Instance.TimeOfDay * ATLAS_CELL_SIZE + ATLAS_OFFSET);
		texture.Region = rect;
	}
}
