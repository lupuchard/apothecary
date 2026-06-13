using Godot;
namespace Apothecary;

public partial class RequestUi : PanelContainer {
	private TextureRect? sprite;
	private Label? name_label;
	private Label? request_label;
	private AspectListUi? requirements_ui;
	private Label? time_label;

	public override void _Ready() {
		sprite = GetNode<TextureRect>("%Sprite");
		name_label = GetNode<Label>("%NameLabel");
		request_label = GetNode<Label>("%RequestLabel");
		requirements_ui = GetNode<AspectListUi>("%RequirementsUi");
		time_label = GetNode<Label>("%TimeLeftLabel");
	}

	public void Update(Visitor visitor) {
		name_label?.Text = visitor.Name;
		request_label?.Text = Tr("INFUSION");
		time_label?.Text = FormatDays(visitor.RemainingDays);
		sprite?.Texture = visitor.Request.Type.SpriteSmall;
		requirements_ui?.Update(visitor.Request.Aspects);
	}
	
	private string FormatDays(int days) {
		return days == 1 ? Tr("ONE_DAY_LEFT") : string.Format(Tr("X_DAYS_LEFT"), days);
	}
}
