using Godot;

namespace Apothecary;

public partial class ProfileEntryUi : HBoxContainer {
	private Button? select_button;
	private BaseButton? delete_button;

	[Signal] public delegate void SelectedEventHandler();
	[Signal] public delegate void DeletePressedEventHandler();

	public string ProfileName {
		get;
		set {
			field = value;
			Update();
		}
	} = "???";

	public override void _Ready() {
		select_button = GetNode<Button>("SelectButton");
		delete_button = GetNode<BaseButton>("DeleteButton");
		select_button.Pressed += EmitSignalSelected;
		delete_button.Pressed += EmitSignalDeletePressed;
		Update();
	}

	private void Update() {
		select_button?.Text = ProfileName;
	}
}
