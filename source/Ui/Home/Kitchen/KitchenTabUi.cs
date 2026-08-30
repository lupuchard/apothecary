using System.Collections.Generic;
namespace Apothecary;

public partial class KitchenTabUi : TabBaseUi {
	private InventoryUi? inventory;
	private List<KitchenProcessBaseUi> processes = [];

	public override void _Ready() {
		inventory = GetNode<InventoryUi>("HBoxContainer/Inventory");
		processes = [
			GetNode<InfusionUi>("%InfusionControl"),
			GetNode<GrinderUi>("%GrinderControl")
		];

		foreach (var ui in processes) {
			ui.OutputCreated += Update;
		}
	}

	private void OnAccept() {
		Game.Instance.AcceptRequest();
		Update();
	}

	private void OnReject() {
		Game.Instance.RejectRequest();
		Update();
	}

	public override void Update() {
		inventory?.Update();
		foreach (var ui in processes) {
			ui.Update();
		}
	}

	public override void CloseUi() {
		inventory?.CloseUi();
	}

	public override bool IsUnlocked() {
		return Game.Instance.IsUnlocked(Feature.Kitchen);
	}
}
