using System.Linq;
using Godot;
namespace Apothecary;

public partial class CurrentVisitorUi : PanelContainer {
	private Label? title_label;
	private Label? name_label;
	private Label? speech_label;
	private Label? request_label;
	private Label? time_label;
	private Label? none_label;
	private TextureRect? sprite;
	private AspectListUi? requirements_container;

	public SlowButton? AcceptButton { get ; private set; }
	public SlowButton? RejectButton { get ; private set; }

	public override void _Ready() {
		title_label = GetNode<Label>("%VisitorTitleLabel");
		title_label?.Text = Tr("VISITOR");
		
		name_label = GetNode<Label>("%VisitorNameLabel");
		speech_label = GetNode<Label>("%VisitorSpeechLabel");
		request_label = GetNode<Label>("%VisitorRequestLabel");
		time_label = GetNode<Label>("%VisitorTimeLimitLabel");
		none_label = GetNode<Label>("%VisitorNoneLabel");
		sprite = GetNode<TextureRect>("%VisitorSprite");
		requirements_container =  GetNode<AspectListUi>("%VisitorRequirementsContainer");
		
		AcceptButton = GetNode<SlowButton>("%VisitorAcceptButton");
		RejectButton = GetNode<SlowButton>("%VisitorRejectButton");
	}

	public void Update() {
		var visitor = Game.Instance.VisitorAtDoor;
		
		if (visitor == null) {
			name_label?.Hide();
			speech_label?.Hide();
			request_label?.Hide();
			time_label?.Hide();
			requirements_container?.Hide();
			sprite?.Hide();
			AcceptButton?.Hide();
			RejectButton?.Hide();
			
			none_label?.Show();
			none_label?.Text = Tr("NO_VISITOR_TEXT");
		} else {
			none_label?.Hide();
			
			name_label?.Show();
			name_label?.Text = visitor.Name;
			speech_label?.Show();
			speech_label?.Text = visitor.RequestText;
			request_label?.Show();
			request_label?.Text = Tr("INFUSION");
			time_label?.Show();
			time_label?.Text = FormatDays(visitor.RemainingDays);
			sprite?.Show();
			sprite?.Texture = visitor.Request.Type.Sprite;
			
			requirements_container?.Show();
			requirements_container?.Update([..visitor.Request.Aspects.Cast<(Aspect?, int)>()]);
			
			AcceptButton?.Show();
			RejectButton?.Show();
		}
	}

	private string FormatDays(int days) {
		return days == 1 ? Tr("ONE_DAY") : string.Format(Tr("X_DAYS"), days);
	}
}
