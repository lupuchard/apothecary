using Godot;

namespace Apothecary;

public partial class TabBaseUi : Container {
	public bool Alert { get; set; }
	
	public virtual void Update() { }

	public virtual void OpenUi() {
		Alert = false;
		Update();
	}

	public virtual void CloseUi() {
		Update();
	}

	public virtual bool IsUnlocked() {
		return true;
	}
}
