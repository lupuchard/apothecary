using System.Linq;
using Godot;

namespace Apothecary;

public partial class ForagingUi : BaseUi {
	public Region? Region {
		get;
		set {
			if (field == value) return;
			field = value;
			Update();
		}
	}

	private Label? title_label;
	private ForagingResult[] foraging_results_controls = [];
	private SlowButton? forage_button;
	private Label? forages_remaining_label;

	public override void _Ready() {
		base._Ready();
		
		title_label = GetNode<Label>("%TitleLabel");
		foraging_results_controls = [..GetNode<Control>("%ForagingResults").GetChildren().OfType<ForagingResult>()];
		foreach (var control in foraging_results_controls) {
			control.Pressed += Update;
		}
		forages_remaining_label = GetNode<Label>("%ForagesRemainingLabel");
		
		forage_button = GetNode<SlowButton>("%ForageButton");
		forage_button.Show();
		forage_button.PressFinished += OnPressForage;
	}

	public void Update() {
		if (Region == null) {
			title_label?.Text = "Unknown";
			forages_remaining_label?.Text = "Error";
			return;
		}

		var results_remaining = false;
		var current_foraging_results = Game.Instance.CurrentForagingResults();
		foreach (var control in foraging_results_controls) {
			if (control.Index < current_foraging_results.Count && current_foraging_results[control.Index] != null) {
				control.Enable(current_foraging_results[control.Index]);
				results_remaining = true;
			} else {
				control.Disable();
			}
		}

		if (!results_remaining && forage_button != null) {
			forage_button.Show();
			var end_of_day = Game.Instance.TimeOfDay >= Game.END_OF_DAY;
			forage_button.Disabled = Region.Remaining <= 0 || end_of_day;
			if (forage_button.Disabled) {
				forage_button.Text = Tr(end_of_day ? "FORAGE_BUTTON_END_OF_DAY" : "FORAGE_BUTTON_UNAVAILABLE");
			} else {
				forage_button.Text = Tr("FORAGE_BUTTON_AVAILABLE");
			}
		} else if (results_remaining) {
			forage_button?.Hide();
		}
		

		title_label?.Text = Tr(Region.Model.Id.ToUpperInvariant());
		forages_remaining_label?.Text = string.Format(Tr("FORAGES_REMAINING_LABEL"), Region.Remaining, Region.Model.MaxForage);
	}

	public override void OpenUi() {
		Update();
	}

	public override void CloseUi() {
		foreach (var result in foraging_results_controls) {
			if (!result.Disabled) {
				result.OnPressed();
			}
		}
	}

	public void OnPressForage() {
		if (Region == null) {
			return;
		}
		forage_button?.Hide();
		
		Game.Instance.DoForaging(Region.Model);
		var foraging_results = Game.Instance.CurrentForagingResults();
		Update();
		if (foraging_results.Count == 0) {
			foraging_results_controls[1].Enable(null);
		}
	}
}
