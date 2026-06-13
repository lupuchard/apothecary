using Godot;

namespace Apothecary;

public partial class VisitorsTabUi : Control {
	private InventoryUi? inventory;

	public override void _Ready() {
		inventory = GetNode<InventoryUi>("HBoxContainer/Inventory");
	}

	public void Update() {
		inventory?.Update();
	}
}
