using Godot;
namespace Apothecary;

[GlobalClass]
public partial class InputSlot : ItemSlot {
	[Signal] public delegate void ItemUpdatedEventHandler();

	[Export]
	public int InputAmount {
		get;
		set {
			field = value;
			Update();
		}
	} = 1;
	public override int Amount => InputAmount;

	public ItemType? ItemType { get; set; }

	public InventorySlot? Referencing {
		get;
		set {
			field = value;
			Update();
			EmitSignalItemUpdated();
		}
	}
	public override Item? Item => Referencing?.Item;

	public Area2D? CollisionArea { get; private set; }

	public override void _EnterTree() {
		AddToGroup("new_input_slot");
	}

	public override void _Ready() {
		base._Ready();
		CollisionArea = GetNode<Area2D>("Area2D");
		Disabled = true;
	}

	public override void Update() {
		base.Update();
		Disabled = Referencing == null;
	}

	private void OnRightClick() {
		Referencing?.ReferencedBy = null;
		Referencing = null;
	}

	public override void _GuiInput(InputEvent input_event) {
		base._GuiInput(input_event);
		if (input_event is InputEventMouseButton mouse_event && mouse_event.ButtonIndex == MouseButton.Right) {
			OnRightClick();
		}
	}
}
