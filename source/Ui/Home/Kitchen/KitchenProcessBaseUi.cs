using Godot;

namespace Apothecary;

public partial class KitchenProcessBaseUi : Panel {
	[Signal] public delegate void OutputCreatedEventHandler();
	
	public virtual void Update() { }
	
	public virtual bool IsUnlocked() {
		return true ;
	}
}
