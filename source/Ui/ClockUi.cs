using Godot;

namespace Apothecary;

public partial class ClockUi : Control {
	private const float ATLAS_OFFSET = 64.0f;
	private const float ATLAS_CELL_SIZE = 64.0f;
		
	private TextureRect? dial_texture;
	private TextureRect? step_texture;
	private VBoxContainer? attributes_container;

	public override void _Ready() {
		dial_texture = GetNode<TextureRect>("DialTexture");
		step_texture = GetNode<TextureRect>("StepTexture");
		attributes_container = GetNode<VBoxContainer>("Attributes");
		Game.Instance.TimeChanged += Update;
		Game.Instance.ResourceUpdated += (_, _) => Update();
		Update();
	}

	public void Update() {
		if (step_texture == null || attributes_container == null) {
			return;
		}

		var texture = (AtlasTexture)step_texture.Texture;
		var rect = texture.Region;
		rect.Position = new Vector2(0, Game.Instance.TimeOfDay * ATLAS_CELL_SIZE + ATLAS_OFFSET);
		texture.Region = rect;
		
		foreach (var child in attributes_container.GetChildren()) {
			if (child is AttributeUi attribute_ui) {
				attribute_ui.Update();
			}
		}
	}
}
