namespace Apothecary;

public partial class HomeLocationSelect : LocationSelect {
	public override void _Ready() {
		base._Ready();
		Game.Instance.TimeChanged += OnTimeChanged;
	}

	private void OnTimeChanged() {
		var has_visitor = Game.Instance.VisitorAtDoor != null;
		if (Alert != has_visitor) {
			Alert = has_visitor;
			Update();
		}
	}
	
	public override LocationLabel CreateLocationLabel() {
		return new HomeLocationLabel();
	}
	
	public override bool Unlocked() {
		return true;
	}
}
