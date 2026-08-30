using Godot;

namespace Apothecary;

public partial class AttributeUi : HBoxContainer {
	[Export] public string resource_name = "";
	[Export] public string resource_max_name = "";
	
	private Label? amount_label;
	private TextureRect? sprite;
	private Label? name_label;
	private Resource resource = Resource.None;
	private Resource resource_max = Resource.None;

	public override void _Ready() {
		amount_label = GetNode<Label>("AmountLabel");
		sprite = GetNode<TextureRect>("Sprite");
		name_label = GetNode<Label>("NameLabel");
		resource = Resources.FromString(resource_name) ?? Resource.None;
		resource_max = Resources.FromString(resource_max_name) ?? Resource.None;
		Update();
	}

	public void Update() {
		var game = Game.Instance;
		var max = game.GetResource(resource_max);
		if (max == 0) {
			Hide();
			return;
		}

		Show();
		amount_label?.Text = $"{game.GetResource(resource)}/{game.GetResource(resource_max)}";
		sprite?.Texture = ResourceLoader.Load<Texture2D>(resource.SmallSpritePath());
		sprite?.Modulate = resource.GetColor();
		name_label?.Text = Tr(resource.TrString());
		name_label?.Modulate = resource.GetColor();
	}
}
