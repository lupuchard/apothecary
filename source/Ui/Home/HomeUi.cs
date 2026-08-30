using System.Collections.Generic;
using Godot;

namespace Apothecary;

public partial class HomeUi : BaseUi {
	private TabContainer? tab_container;
	private List<TabBaseUi> tabs = [];

	private Container? material_list;
	private PackedScene? material_info_scene;
	private Texture2D? alert_icon;

	public override void _Ready() {
		base._Ready();
		alert_icon = ResourceLoader.Load<Texture2D>("res://assets/alert_icon.png");
		
		tab_container = GetNode<TabContainer>("TabContainer");
		for (var i = 0; i < tab_container.GetTabCount(); i++) {
			tabs.Add((TabBaseUi)tab_container.GetTabControl(i));
		}
		
		material_info_scene = ResourceLoader.Load<PackedScene>("res://controls/resource_info.tscn");
		material_list = GetNode<Container>("%ResourceList");
		foreach (var child in material_list.GetChildren()) {
			child.QueueFree();
		}
		
		foreach (var material in Resources.Materials) {
			var material_ui = material_info_scene.Instantiate<MaterialUi>();
			material_ui.Resource = material;
			material_list.AddChild(material_ui);
			material_ui.Update();
		}
		
		tab_container.TabChanged += (_) => Update(false);
		tab_container.SetCurrentTab(0);

		Game.Instance.FeatureUnlocked += (_) => Update(false);
	}

	public void Update(bool instant) {
		if (tab_container == null) return;
		
		tabs[tab_container.GetCurrentTab()].OpenUi();

		for (var i = 0; i < tabs.Count; i++) {
			var tab = tabs[i];
			if (instant) {
				tab.Alert = false;
			} else if (tab_container.IsTabHidden(i) && tabs[i].IsUnlocked() && i != tab_container.GetCurrentTab()) {
				tab.Alert = true;
			}
			
			if (tab.Alert) {
				tab_container.SetTabIcon(i, alert_icon);
			} else {
				tab_container.SetTabIcon(i, null);
			}
			
			tab_container.SetTabHidden(i, !tabs[i].IsUnlocked());
		}

		foreach (var child in material_list!.GetChildren()) {
			if (child is MaterialUi material_ui) {
				material_ui.Update();
			}
		}
	}

	public override void CloseUi() {
		if (tab_container == null) return;
		tabs[tab_container.GetCurrentTab()].CloseUi();
	}
}
