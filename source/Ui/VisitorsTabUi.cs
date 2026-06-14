using Godot;

namespace Apothecary;

public partial class VisitorsTabUi : Control {
	private InventoryUi? inventory;
	private CurrentVisitorUi? current_visitor;
	private RequestListUi? request_list;

	public override void _Ready() {
		inventory = GetNode<InventoryUi>("HBoxContainer/Inventory");
		current_visitor = GetNode<CurrentVisitorUi>("%CurrentVisitor");
		request_list = GetNode<RequestListUi>("%CurrentRequests");
		current_visitor.AcceptButton?.PressFinished += OnAccept;
		current_visitor.RejectButton?.PressFinished += OnReject;
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
		current_visitor?.Update();
		request_list?.Update();
	}
	
	public void CloseUi() {
		inventory?.CloseUi();
	}
}
