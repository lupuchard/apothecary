using Godot;
namespace Apothecary;

public partial class KitchenTabUi : MarginContainer {
	private InventoryUi? inventory;
	private InfusionUi? infusion;

	public override void _Ready() {
		inventory = GetNode<InventoryUi>("HBoxContainer/Inventory");
		infusion = GetNode<InfusionUi>("%InfusionControl");
		infusion.InfusionCreated += Update;
	}

	private void OnAccept() {
		Game.Instance.AcceptRequest();
		Update();
	}

	private void OnReject() {
		Game.Instance.RejectRequest();
		Update();
	}

	public void Update() {
		inventory?.Update();
		infusion?.Update();
	}

	public void CloseUi() {
		inventory?.CloseUi();
	}
}
