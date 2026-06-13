using Godot;

namespace Apothecary;

public partial class HomeUi : BaseUi {
	private TabContainer? tab_container;
	private VisitorsTabUi? visitors_tab;

	public override void _Ready() {
		base._Ready();
		tab_container = GetNode<TabContainer>("TabContainer");
		visitors_tab = GetNode<VisitorsTabUi>("%VisitorsTab");
	}

	public void Update() {
		if (tab_container?.CurrentTab == 0) {
			visitors_tab?.Update();
		}
	}
}
