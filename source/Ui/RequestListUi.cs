using Godot;
using System.Collections.Generic;
namespace Apothecary;

public partial class RequestListUi : VBoxContainer {
	private readonly List<RequestUi> request_controls = [];
	
	private PackedScene? visitor_request_scene;
	
	public override void _Ready() {
		foreach (var child in GetChildren()) {
			RemoveChild(child);
			child.QueueFree();
		}
		
		visitor_request_scene = ResourceLoader.Load<PackedScene>("res://controls/visitor_request.tscn");
	}

	public void Update() {
		var requests = Game.Instance.CurrentRequests;

		while (request_controls.Count < requests.Count) {
			var new_control = (RequestUi)visitor_request_scene!.Instantiate();
			request_controls.Add(new_control);
			AddChild(new_control);
			new_control.Given += Update;
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
