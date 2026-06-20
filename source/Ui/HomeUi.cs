using Godot;

namespace Apothecary;

public partial class HomeUi : BaseUi {
	private TabContainer? tab_container;
	private VisitorsTabUi? visitors_tab;
	private KitchenTabUi? kitchen_tab;
	private JournalTabUi? journal_tab;

	public override void _Ready() {
		base._Ready();
		tab_container = GetNode<TabContainer>("TabContainer");
		visitors_tab = GetNode<VisitorsTabUi>("%VisitorsTab");
		kitchen_tab = GetNode<KitchenTabUi>("%KitchenTab");
		journal_tab = GetNode<JournalTabUi>("%JournalTab");
		tab_container.TabChanged += (_) => Update();
	}

	public void Update() {
		switch (tab_container?.CurrentTab) {
			case 0: visitors_tab?.Update(); break;
			case 1: kitchen_tab?.Update(); break;
			case 2: journal_tab?.Update(); break;
		}
	}

	public override void CloseUi() {
		switch (tab_container?.CurrentTab) {
			case 0: visitors_tab?.CloseUi(); break;
			case 1: kitchen_tab?.CloseUi(); break;
			case 2: journal_tab?.Update(); break;
		}
	}
}
