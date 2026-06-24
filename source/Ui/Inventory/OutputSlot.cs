using Godot;
namespace Apothecary;

public partial class OutputSlot : ItemSlot {
	[Export]
	public int OutputAmount {
		get;
		set {
			field = value;
			Update();
		}
	} = 1;
	public override int Amount => OutputAmount;

	public Item? Output {
		get;
		set {
			field = value;
			Update();
		}
	}
	public override Item? Item => Output;

	public override void _Ready() {
		base._Ready();
		Disabled = true;
	}

	public override void Update() {
		base.Update();
		texture_rect?.Modulate = new Color(1, 1, 1, 0.5f);
	}
}
