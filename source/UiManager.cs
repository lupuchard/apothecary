using System.Collections.Generic;
using Godot;

namespace Apothecary;

public partial class UiManager : Node2D {
	private MapColor? map;
	private Button? background_exit_button;
	private ForagingUi? foraging_ui;
	private HomeUi? home_ui;
	private BaseButton? home_icon;
	
	private MainMenuUi? main_menu;

	private readonly List<RegionSelect> region_selects = [];
	private BaseUi? current_ui;

	private PlayerCamera? player_camera;

	private AudioStreamPlayer? click_sound1;
	private AudioStreamPlayer? click_sound2;
	
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
		
		map = GetNode<MapColor>("%Map");

		foreach (var child in map.GetChildren()) {
			if (child is RegionSelect region_select) {
				region_select.Selected += OnRegionSelected;
				region_selects.Add(region_select);
			}
		}
		
		main_menu = GetNode<MainMenuUi>("%MainMenu");
		main_menu.GameStarted += CloseUi;

		player_camera = GetNode<PlayerCamera>("PlayerCamera");
		
		click_sound1 = GetNode<AudioStreamPlayer>("%ClickSound1");
		click_sound2 = GetNode<AudioStreamPlayer>("%ClickSound2");
		AttachButtonClickSound(this);

		Game.Instance.TimeChanged += OnTimeChanged;
		OnTimeChanged();
	}
	
	public override void _Input(InputEvent input_event) {
		if (input_event.IsActionPressed("ui_close_dialog")) {
			if (current_ui != null) {
				CloseUi();
			} else {
				main_menu?.Open();
			}
		}
	}

	private void OnTimeChanged() {
		var game = Game.Instance;
		map?.SetColor(game.TimeOfDay, game.Season, 1.0);
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

		player_camera?.CanPan = false;
	}

	private void CloseUi() {
		background_exit_button?.Hide();
		current_ui?.CloseUi();
		current_ui?.Hide();
		current_ui = null;
		foreach (var region_select in region_selects) {
			region_select.InputPickable = true;
		}
		
		player_camera?.CanPan = true;
	}

	private void AttachButtonClickSound(Node parent) {
		foreach (var child in parent.FindChildren("*", "Button")) {
			if (child is Button button) {
				button.Pressed += () => click_sound1?.Play();
			}
		}
		
		foreach (var child in parent.FindChildren("*", "TextureButton")) {
			if (child is TextureButton button) {
				button.Pressed += () => click_sound1?.Play();
			}
		}
		
		foreach (var child in parent.FindChildren("*", "TabContainer")) {
			if (child is TabContainer button) {
				button.TabChanged += _ => click_sound2?.Play();
			}
		}
	}
}
