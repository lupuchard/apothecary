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
	private MaterialUi? resource_info;

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
		resource_info = GetNode<MaterialUi>("%VisitorResourceInfo");
		
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
			resource_info?.Hide();
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
			time_label?.Show();
			time_label?.Text = FormatDays(visitor.RemainingDays);
			sprite?.Show();
			
			if (visitor.Request != null) {
				resource_info?.Hide();
				requirements_container?.Show();
				requirements_container?.Update([..visitor.Request.Aspects.Cast<(Aspect?, int)>()]);
				request_label?.Show();
				request_label?.Text = Tr("INFUSION");
				sprite?.Texture = visitor.Request.Type.Sprite;
			} else if (visitor.Special == SpecialRequest.Bills) {
				resource_info?.Show();
				resource_info?.Resource = Resource.Coins;
				resource_info?.Amount = visitor.Amount;
				requirements_container?.Hide();
				request_label?.Hide();
				sprite?.Texture = ResourceLoader.Load<Texture2D>(visitor.Special.SpritePath());
			}
			
			AcceptButton?.Show();
			RejectButton?.Show();
		}

		if (Game.Instance.Day == 0) {
			RejectButton?.Hide();
		}
	}

	private string FormatDays(int days) {
		return days == 1 ? Tr("ONE_DAY") : string.Format(Tr("X_DAYS"), days);
	}
}
