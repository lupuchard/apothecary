using Godot;

namespace Apothecary;

public partial class VisitorsTabUi : TabBaseUi {
	private InventoryUi? inventory;
	private CurrentVisitorUi? current_visitor;
	private RequestListUi? request_list;

	public override void _Ready() {
		inventory = GetNode<InventoryUi>("HBoxContainer/Inventory");
		current_visitor = GetNode<CurrentVisitorUi>("%CurrentVisitor");
		request_list = GetNode<RequestListUi>("%CurrentRequests");
		current_visitor.AcceptButton?.Pressed += OnAccept;
		current_visitor.RejectButton?.Pressed += OnReject;
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
		current_visitor?.Update();
		request_list?.Update();
		inventory?.Update();
	}
	
	public override void CloseUi() {
		inventory?.CloseUi();
	}
}
