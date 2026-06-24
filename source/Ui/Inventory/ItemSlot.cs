using Godot;
namespace Apothecary;

public abstract partial class ItemSlot : Button {
	public abstract Item? Item { get; }
	public abstract int Amount { get; }

	protected TextureRect? texture_rect;
	protected Label? amount_label;

	public override void _Ready() {
		base._Ready();
		texture_rect = GetNode<TextureRect>("TextureRect");
		amount_label = GetNode<Label>("AmountLabel");
		Update();
	}

	public virtual void Update() {
		amount_label?.Text = Amount > 1 ? ("x" + Amount) : "";
		texture_rect?.Texture = Item?.GetSprite();
	}
}
