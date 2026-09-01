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

		Game.Instance.FeatureUnlocked += OnFeatureUnlocked;
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
			ui.Visible = ui.IsUnlocked();
		}
	}

	private void OnFeatureUnlocked(Feature feature) {
		foreach (var ui in processes) {
			if (ui.Visible != ui.IsUnlocked()) {
				ui.Visible = ui.IsUnlocked();
				Alert = true;
			}
		}
	}

	public override void CloseUi() {
		inventory?.CloseUi();
	}

	public override bool IsUnlocked() {
		return Game.Instance.IsUnlocked(Feature.Kitchen);
	}
}
