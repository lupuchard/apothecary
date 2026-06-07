using Godot;

namespace Apothecary;

public partial class BaseUi : Control {
	public BaseButton? CloseButton { get; private set; }
	
	public override void _Ready() {
		base._Ready();
		CloseButton = GetNodeOrNull<BaseButton>("Panel/CloseButton");
	}
	
	public virtual void OpenUi() { }
	public virtual void CloseUi() { }
}
