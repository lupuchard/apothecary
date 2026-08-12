using Godot;

namespace Apothecary;

public partial class MainMenuUi : PanelContainer {
	private readonly SaveManager save_manager = new();
	
	private Control? main_main_menu;
	private Button? new_game_button;
	private Button? continue_button;
	private Button? load_game_button;
	private Button? exit_button;

	private Control? new_game_menu;
	private BaseButton? new_game_back_button;
	private LineEdit? new_game_name_edit;
	private Button? new_game_start_button;

	private Control? load_game_menu;
	private BaseButton? load_game_back_button;
	private Control? load_game_list;
	
	public override void _Ready() {
		main_main_menu = GetNode<Control>("%MainMainMenu");
		new_game_button = GetNode<Button>("%NewGameButton");
		new_game_button.Pressed += OnNewGame;
		continue_button = GetNode<Button>("%ContinueButton");
		continue_button.Pressed += OnContinue;
		load_game_button = GetNode<Button>("%LoadGameButton");
		exit_button = GetNode<Button>("%ExitButton");
		exit_button.Pressed += OnExitGame;
		
		new_game_menu = GetNode<Control>("%NewGameMenu");
		new_game_back_button = GetNode<BaseButton>("%NewGameBackButton");
		new_game_back_button.Pressed += Open;
		new_game_name_edit = GetNode<LineEdit>("%NewGameNameEdit");
		new_game_start_button = GetNode<Button>("%NewGameStartButton");
		new_game_start_button.Pressed += StartNewGame;

		load_game_menu = GetNode<Control>("%LoadGameMenu");
		load_game_button = GetNode<Button>("%LoadGameButton");
		load_game_button.Pressed += OnLoadGame;
		load_game_back_button = GetNode<BaseButton>("%LoadGameBackButton");
		load_game_back_button.Pressed += Open;
		load_game_list = GetNode<Control>("%LoadGameList");

		Open();
	}

	private void Open() {
		main_main_menu?.Show();
		new_game_menu?.Hide();
		load_game_menu?.Hide();
		var has_profiles = save_manager.GetProfiles().Count == 0;
		continue_button?.Disabled = has_profiles;
		load_game_button?.Disabled = has_profiles;
		Show();
	}

	private void OnNewGame() {
		main_main_menu?.Hide();
		new_game_menu?.Show();
		var num_profiles = save_manager.GetProfiles().Count;
		new_game_name_edit?.Text = string.Format(Tr("DEFAULT_PROFILE_NAME"), num_profiles + 1);
	}

	private void OnContinue() {
		var profiles = save_manager.GetProfiles();
		if (profiles.Count > 0) {
			LoadGame(profiles[0]);
		}
	}

	private void StartNewGame() {
		save_manager.CreateProfile(new_game_name_edit!.Text);
		Hide();
	}

	private void OnLoadGame() {
		if (load_game_list == null) return;
		main_main_menu?.Hide();
		load_game_menu?.Show();

		while (load_game_list.GetChildCount() > 1) {
			var child = load_game_list.GetChild(load_game_list.GetChildCount() - 1);
			child.QueueFree();
			load_game_list.RemoveChild(child);
		}

		var profiles = save_manager.GetProfiles();
		foreach (var profile in profiles) {
			var button = new Button();
			button.Text = profile.Name;
			button.Pressed += () => LoadGame(profile);
			load_game_list.AddChild(button);
		}
	}

	private void LoadGame(Profile profile) {
		var game = save_manager.LoadGame(profile);
		Game.LoadGame(game);
		Hide();
	}

	private void OnExitGame() {
		save_manager.SaveGame(Game.Instance);
		Game.NewGame();
		GetTree().Quit();
	}
}
