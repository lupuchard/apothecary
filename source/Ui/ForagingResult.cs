using Godot;
namespace Apothecary;

public partial class ForagingResult : Button {
	[Signal] public delegate void AcquireEventHandler(TextureRect sprite);
	[Export] public int Index { get; set; }
	private Control? Child;
	private TextureRect? TextureRect;
	private Label? Label;
	public bool Empty { get; private set; }= false;

	public override void _Ready() {
		Child = (Control)GetChild(0);
		TextureRect = GetNode<TextureRect>("%TextureRect");
		Label = GetNode<Label>("%Label");
		Pressed += OnPressed;
		Disable();
	}

	public void OnPressed() {
		if (!Empty) {
			Game.Instance.AcquireForagingResult(Index);
			EmitSignalAcquire(TextureRect);
		}

		Empty = false;
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
			Empty = true;
		} else {
			Label?.Text = Tr(with_item.Id.ToUpper());
			TextureRect?.Texture = with_item.Sprite;
			Empty = false;
		}
		Child?.Show();
	}
}
