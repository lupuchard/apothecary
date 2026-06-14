using Godot;
namespace Apothecary;

public partial class InventorySlot : ItemSlot {
	[Export] public int Index { get; set; }

	public override Item? Item {
		get {
			var inventory = Game.Instance.GetInventory();
			return Index < inventory.Count ? inventory[Index].Item1 : null;
		}
	}

	public override int Amount {
		get {
			var inventory = Game.Instance.GetInventory();
			if (Index >= inventory.Count) return 0;
			var (_, amount) = inventory[Index];
			if (ReferencedBy != null) {
				return amount - ReferencedBy.Amount;
			} else if (Dragging) {
				return amount - 1;
			} else {
				return amount;
			}
		}
	}

	public bool Dragging {
		get;
		set {
			field = value;
			Update();
		}
	}
	public InputSlot? ReferencedBy {
		get;
		set {
			field = value;
			Update();
		}
	}

	public override void Update() {
		base.Update();

		if (Item == null) {
			Hide();
		} else {
			Show();
			texture_rect?.Modulate = Amount == 0 ? new Color(1, 1, 1, 0.5f) : Colors.White;
		}
	}
}
