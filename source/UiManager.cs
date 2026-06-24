using System.Collections.Generic;
using Godot;

namespace Apothecary;

public partial class UiManager : Node2D {
	private Node2D? map;
	private Button? background_exit_button;
	private ForagingUi? foraging_ui;
	private HomeUi? home_ui;
	private BaseButton? home_icon;

	private readonly List<RegionSelect> region_selects = [];
	private BaseUi? current_ui;
	
	public override void _Ready() {
		background_exit_button = GetNode<Button>("%BackgroundExitButton");
		background_exit_button.Pressed += CloseUi;
		background_exit_button.Hide();
		
		foraging_ui = GetNode<ForagingUi>("%ForagingUi");
		foraging_ui.CloseButton?.Pressed += CloseUi;
		foraging_ui.Hide();
		
		home_ui = GetNode<HomeUi>("%HomeUi");
		home_ui.CloseButton?.Pressed += CloseUi;
		home_ui.Hide();
		home_icon = GetNode<BaseButton>("%HomeIcon");
		home_icon.Pressed += () => OpenUi(home_ui);
		
		map = GetNode<Node2D>("%Map");

		foreach (var child in map.GetChildren()) {
			if (child is RegionSelect region_select) {
				region_select.Selected += OnRegionSelected;
				region_selects.Add(region_select);
			}
		}
	}

	private void OnRegionSelected(RegionSelect.Type type, RegionModel? region) {
		if (current_ui != null) return;
		
		if (type == RegionSelect.Type.Region && region != null && foraging_ui != null) {
			foraging_ui.Region = Game.Instance.GetRegion(region.Id);
			OpenUi(foraging_ui);
		} else if (type == RegionSelect.Type.Home && home_ui != null) {
			home_ui.Update();
			OpenUi(home_ui);
		}
	}

	private void OpenUi(BaseUi control) {
		background_exit_button?.Show();
		control.Show();
		control.OpenUi();
		current_ui = control;
		foreach (var region_select in region_selects) {
			region_select.InputPickable = false;
		}
	}

	private void CloseUi() {
		background_exit_button?.Hide();
		current_ui?.CloseUi();
		current_ui?.Hide();
		current_ui = null;
		foreach (var region_select in region_selects) {
			region_select.InputPickable = true;
		}
	}
}
