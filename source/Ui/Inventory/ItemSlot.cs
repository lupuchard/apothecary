using Godot;
namespace Apothecary;

public abstract partial class ItemSlot : Button {
	public abstract Item? Item { get; }
	public abstract int Amount { get; }

	protected TextureRect? texture_rect;
	protected Label? amount_label;
	protected TextureRect? corner_sprite;

	public override void _Ready() {
		base._Ready();
		texture_rect = GetNode<TextureRect>("%TextureRect");
		amount_label = GetNode<Label>("%AmountLabel");
		corner_sprite = GetNode<TextureRect>("%Corner");
		Update();
	}

	public virtual void Update() {
		amount_label?.Text = Amount > 1 ? ("x" + Amount) : "";
		if (Item != null) {
			var (texture, corner_texture) = Item.Value.GetSprite();
			texture_rect?.Texture = texture;
			corner_sprite?.Texture = corner_texture;
			corner_sprite?.StretchMode = corner_texture?.GetSize().X > corner_sprite.Size.X 
				? TextureRect.StretchModeEnum.KeepCentered 
				: TextureRect.StretchModeEnum.Scale;
		} else {
			texture_rect?.Texture = null;
			corner_sprite?.Texture = null;
		}
	}
}
