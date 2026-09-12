using Godot;
using System.Collections.Generic;

namespace Apothecary;

public partial class ResourceSelectUi : PanelContainer {
	[Signal] public delegate void ResourceSelectedEventHandler(Control where, int index);
	[Signal] public delegate void CloseEventHandler(Control where);
	private Container? button_list;
	private Control? current_location;
	
	private Texture2D? close_icon;
	private bool is_hovered = false;
	
	public override void _EnterTree() {
		AddToGroup("ResourceSelect");
	}

	public override void _Ready() {
		close_icon = ResourceLoader.Load<Texture2D>("res://assets/close.png");
		
		button_list = GetNode<Container>("%ButtonList");
		var button = (Button)button_list.GetChild(0);
		button.Pressed += () => OnSelect(0);
		
		MouseEntered += () => is_hovered = true;
		MouseExited += () => is_hovered = false;
	}
	
	public void Show(IList<Resource?> resources, Control where) {
		if (button_list == null) return;
		
		GlobalPosition = where.GlobalPosition + new Vector2(0, where.Size.Y);
		current_location = where;
		Show();

		while (button_list.GetChildCount() < resources.Count) {
			var button = (Button)button_list.GetChild(0).Duplicate(0);
			button.Pressed += () => OnSelect(button_list.GetChildCount());
			button_list.AddChild(button);
		}

		for (var i = 0; i < resources.Count; i++) {
			var button = (Button)button_list.GetChild(i);
			button.Show();
			var rect = (TextureRect)button.GetChild(0);
			rect.Texture = resources[i] == null 
				? close_icon 
				: ResourceLoader.Load<Texture2D>(resources[i]!.Value.SpritePath());
			rect.Modulate = resources[i]?.GetColor() ?? Colors.White;
		}

		for (var i = resources.Count; i < button_list.GetChildCount(); i++) {
			var button = (Button)button_list.GetChild(i);
			button.Hide();
		}
	}

	private void OnSelect(int index) {
		if (current_location != null) {
			EmitSignalResourceSelected(current_location, index);
			Hide();
		}
	}
	
	public override void _Input(InputEvent input_event) {
		if (input_event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true } && !is_hovered && Visible) {
			EmitSignalClose(current_location);
			Hide();
		}
	}
}
