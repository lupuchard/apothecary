using Godot;
using System.Collections.Generic;
namespace Apothecary;

public partial class RequestListUi : VBoxContainer {
	private readonly List<RequestUi> request_controls = [];
	
	public override void _Ready() {
		foreach (var child in GetChildren()) {
			RemoveChild(child);
			child.QueueFree();
		}
	}

	public void Update() {
		var requests = Game.Instance.CurrentRequests;

		while (request_controls.Count < requests.Count) {
			var new_control = new RequestUi();
			request_controls.Add(new_control);
			AddChild(new_control);
		}
		
		var i = 0;
		foreach (var request in requests) {
			request_controls[i].Update(request);
			request_controls[i].Show();
			i++;
		}

		for (; i < request_controls.Count; i++) {
			request_controls[i].Hide();
		}
	}
}
