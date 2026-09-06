using System.Linq;
using System.Text;
using Godot;

namespace Apothecary;

public partial class BedroomTabUi : TabBaseUi {
	private Label? sleep_label;
	private SlowButton? sleep_button;
	private ColorRect? end_of_day_fade;
	
	private Control? end_of_day_popup;
	private RichTextLabel? end_of_day_summary;
	private Label? next_day_label;
	private Button? wake_up_button;

	private readonly Color EIGENGRAU = Color.Color8(22, 22, 29, 250);
	private readonly Color TRANSPARENT_EIGENGRAU = Color.Color8(22, 22, 29, 0);

	public override void _Ready() {
		base._Ready();
		sleep_label = GetNode<Label>("%SleepLabel");
		sleep_button = GetNode<SlowButton>("%SleepButton");
		sleep_button.Pressed += OnEndDay;
		
		end_of_day_fade = GetNode<ColorRect>("%EndOfDayFade");
		end_of_day_popup = GetNode<Control>("%EndOfDayPopup");
		end_of_day_summary = GetNode<RichTextLabel>("%EndOfDaySummary");
		next_day_label = GetNode<Label>("%NextDayLabel");
		wake_up_button = GetNode<Button>("%WakeUpButton");
		wake_up_button.Pressed += OnWakeUp;

		end_of_day_fade?.Hide();
		end_of_day_popup?.Hide();
		
		Update();
	}

	public override void Update() {
		sleep_button?.Disabled = false;
		if (Game.Instance.IsItDaytime()) {
			sleep_label?.Text = Tr("TOO_EARLY_TO_SLEEP");
			sleep_button?.Hide();
		} else if (Game.Instance.TimeOfDay < 5) {
			sleep_label?.Text = string.Format(Tr("EARLY_TO_SLEEP"), 5 - Game.Instance.TimeOfDay);
			sleep_button?.Show();
		} else {
			sleep_label?.Text = Tr("TIME_TO_SLEEP");
			sleep_button?.Show();
		}
	}

	public override bool IsUnlocked() {
		return Game.Instance.IsUnlocked(Feature.Bedroom);
	}

	private void OnEndDay() {
		if (end_of_day_fade == null) return;
		sleep_button?.Disabled = true;
		end_of_day_fade.Show();
		var tween = CreateTween();
		tween.TweenProperty(end_of_day_fade, "color", EIGENGRAU, 2.0);
		tween.Finished += () => {
			var game = Game.Instance;
			
			var report = game.NextDay();
			var summary_text = new StringBuilder();
			if (report.FailedRequests.Count > 0) {
				summary_text.Append(Tr("FAILED_REQUESTS")).Append("\n  ");
				summary_text.AppendJoin("\n  ", report.FailedRequests.Select(request => string.Format(
					Tr("FAILED_REQUEST"),
					request.Name
				)) + new Reward([(Resource.Reputation, -1)]).ToBbCodeString());
			}

			if (report.ResourceSummary.Length > 0) {
				summary_text.Append(Tr("END_OF_DAY_SUMMARY")).Append("\n  ");
				summary_text.AppendJoin("\n  ", report.ResourceSummary.Select(x => string.Format(
					Tr(x.amount > 0 ? x.resource.GainTrString() : x.resource.LostTrString()),
					x.amount,
					BbCodeUtil.Img(x.resource.SmallSpritePath(), x.resource.GetColor())
				)));
			}
			
			next_day_label?.Text = string.Format(Tr("IT_IS_NOW_DAY"), game.Day, Tr(game.Season.TrString()));
			end_of_day_popup?.Show();
		};
	}

	private void OnWakeUp() {
		Update();
		end_of_day_popup?.Hide();
		end_of_day_fade?.Color = TRANSPARENT_EIGENGRAU;
		end_of_day_fade?.Hide();
	}
	
	public override bool ClosePopup() {
		if (end_of_day_popup?.Visible == true) {
			OnWakeUp();
			return true;
		}

		return false;
	}
}
