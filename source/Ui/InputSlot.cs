using Godot;
namespace Apothecary;

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

	public ItemSlot? Referencing {
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
		AddToGroup("input_item_slot");
	}

	public override void _Ready() {
		base._Ready();
		CollisionArea = GetNode<Area2D>("Area2D");
		Disabled = true;
	}
}
