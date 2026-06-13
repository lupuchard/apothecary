using Godot;
namespace Apothecary;

public partial class ItemSlot : Button {
	public int Index { get; set; }
	private TextureRect? texture_rect;
	private Label? amount_label;

	public override void _Ready() {
		base._Ready();
		texture_rect = GetNode<TextureRect>("TextureRect");
		amount_label = GetNode<Label>("AmountLabel");
		Update();
	}
	
	public void Update() {
		var inventory = Game.Instance.GetInventory();
		if (Index >= inventory.Count) {
			Hide();
		} else {
			Show();
			
			var (item, amount) = inventory[Index];
			amount_label?.Text = amount > 1 ? ("x" + amount) : "";
			texture_rect?.Texture = item.Raw[0].Sprite; // TODO
		}
	}
}
