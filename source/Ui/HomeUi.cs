using Godot;

namespace Apothecary;

public partial class HomeUi : BaseUi {
	private TabContainer? tab_container;
	private VisitorsTabUi? visitors_tab;
	private KitchenTabUi? kitchen_tab;
	private JournalTabUi? journal_tab;

	private Container? resource_list;
	private PackedScene? resource_info_scene;

	public override void _Ready() {
		base._Ready();
		tab_container = GetNode<TabContainer>("TabContainer");
		visitors_tab = GetNode<VisitorsTabUi>("%VisitorsTab");
		kitchen_tab = GetNode<KitchenTabUi>("%KitchenTab");
		journal_tab = GetNode<JournalTabUi>("%JournalTab");
		tab_container.TabChanged += (_) => Update();
		tab_container.SetCurrentTab(0);
		
		resource_info_scene = ResourceLoader.Load<PackedScene>("res://controls/resource_info.tscn");
		resource_list = GetNode<Container>("%ResourceList");
		while (resource_list.GetChildCount() > 0) {
			resource_list.RemoveChild(resource_list.GetChild(0));
		}
		
		for (var i = 1; i < (int)Resource.COUNT; i++) {
			var resource_ui = resource_info_scene.Instantiate<ResourceUi>();
			resource_ui.Resource = (Resource)i;
			resource_list.AddChild(resource_ui);
			resource_ui.Update();
		}
	}

	public void Update() {
		switch (tab_container?.CurrentTab) {
			case 0: visitors_tab?.Update(); break;
			case 1: kitchen_tab?.Update(); break;
			case 2: journal_tab?.Update(); break;
		}

		foreach (var child in resource_list!.GetChildren()) {
			if (child is ResourceUi resource_ui) {
				resource_ui.Update();
			}
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
