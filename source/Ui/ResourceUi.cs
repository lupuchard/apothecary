using Godot;
namespace Apothecary;

public partial class ResourceUi : HBoxContainer {
	public Resource? Resource {
		get;
		set {
			field = value;
			UpdateResource();
		}
	}

	private Label? count_label;
	private TextureRect? sprite;

	public override void _Ready() {
		count_label = GetNode<Label>("CountLabel");
		sprite = GetNode<TextureRect>("Sprite");
		
		Game.Instance.ResourceUpdated += resource => {
			if (resource == Resource) {
				UpdateResource();
			}
		};
	}

	public void Update() {
		var count = Resource == null ? 0 : Game.Instance.GetResource(Resource.Value);
		if (count == 0) {
			Hide();
			return;
		}
		Show();

		count_label?.Text = count.ToString();
	}

	private void UpdateResource() {
		if (Resource != null) {
			var filename = $"res://assets/resources/{Resource.Value.TrString().ToLowerInvariant()}.png";
			sprite?.Texture = ResourceLoader.Load<Texture2D>(filename);
			sprite?.Modulate = Resource.Value.GetColor();
		}

		Update();
	}
}
