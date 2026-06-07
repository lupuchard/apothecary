using Godot;
namespace Apothecary;

public partial class ForagingResult : Button {
	[Export] public int Index { get; set; }
	private Control? Child;
	private TextureRect? TextureRect;
	private Label? Label;

	public override void _Ready() {
		Child = (Control)GetChild(0);
		TextureRect = GetNode<TextureRect>("%TextureRect");
		Label = GetNode<Label>("%Label");
		Pressed += OnPressed;
		Disable();
	}

	public void OnPressed() {
		Game.Instance.AcquireForagingResult(Index);
		Disable();
	}

	public void Disable() {
		Disabled = true;
		Child?.Hide();
	}

	public void Enable(ItemModel? with_item) {
		Disabled = false;
		if (with_item == null) {
			Label?.Text = Tr("NO_FORAGE_FOUND");
			TextureRect?.Texture = ResourceLoader.Load<Texture2D>("res://assets/item/nothing.png");
		} else {
			Label?.Text = Tr(with_item.Id.ToUpper());
			TextureRect?.Texture = with_item.Sprite;
		}
		Child?.Show();
	}
}
