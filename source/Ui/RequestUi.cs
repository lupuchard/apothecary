using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
namespace Apothecary;

public partial class RequestUi : PanelContainer {
	[Signal] public delegate void GivenEventHandler();
	
	private Visitor? visitor;
	private TextureRect? sprite;
	private Label? name_label;
	private Label? request_label;
	private AspectListUi? requirements_ui;
	private Label? time_label;
	private InputSlot? destination_slot;
	private Container? complete_request_row;
	private Label? treatment_quality_label;
	private SlowButton? give_button;
	private ResourceSelectSlot? resource_slot;

	public override void _Ready() {
		sprite = GetNode<TextureRect>("%Sprite");
		name_label = GetNode<Label>("%NameLabel");
		request_label = GetNode<Label>("%RequestLabel");
		requirements_ui = GetNode<AspectListUi>("%RequirementsUi");
		time_label = GetNode<Label>("%TimeLeftLabel");
		destination_slot = GetNode<InputSlot>("%DestinationSlot");
		destination_slot.ItemType = ItemType.Infusion;
		complete_request_row = GetNode<Container>("%CompleteRequestRow");
		treatment_quality_label = GetNode<Label>("%TreatmentQualityLabel");
		give_button = GetNode<SlowButton>("%GiveButton");
		resource_slot = GetNode<ResourceSelectSlot>("%ResourceSlot");

		destination_slot.ItemUpdated += OnItemUpdated;
		give_button.Pressed += OnGive;
		give_button.Show();
	}

	public void Update(Visitor new_visitor) {
		visitor = new_visitor;
		name_label?.Text = visitor.Name;
		time_label?.Text = FormatDays(visitor.RemainingDays);

		if (visitor.Request != null) {
			sprite?.Texture = visitor.Request.Type.SpriteSmall;
		} else {
			sprite?.Texture = ResourceLoader.Load<Texture2D>(visitor.Special.SmallSpritePath());
		}

		if (visitor.Request == null) {
			requirements_ui?.Hide();
			destination_slot?.Hide();
			request_label?.Text = Tr(visitor.Special.TrString());
			resource_slot?.Show();
			resource_slot?.Options = visitor.Special switch {
				SpecialRequest.None => [null],
				SpecialRequest.Bills => [(Resource.Coins, visitor.Amount ?? 1)],
				_ => throw new ArgumentOutOfRangeException()
			};
		} else {
			resource_slot?.Hide();
			request_label?.Text = Tr("INFUSION");
			requirements_ui?.Show();
			requirements_ui?.Update([..visitor.Request.Aspects.Cast<(Aspect?, int)>()]);
			destination_slot?.Show();
		}
		
		OnItemUpdated();
	}
	
	private string FormatDays(int days) {
		return days == 1 ? Tr("ONE_DAY_LEFT") : string.Format(Tr("X_DAYS_LEFT"), days);
	}

	private void OnItemUpdated() {
		if (visitor?.Request == null) {
			complete_request_row?.Show();
			treatment_quality_label?.Text = "";
		} else if (destination_slot?.Item is Item item && visitor != null) {
			complete_request_row?.Show();
			var visible_aspects = Game.Instance.Journal.GetShownAspects(item.Raw, item.Aspects);
			var known = visible_aspects.Where(x => x.Item1 != null).Cast<(Aspect, int)>().ToList();
			var treatment_quality = Game.CalculateTreatmentQuality(visitor, known);
			UpdateTreatmentQualityLabel(treatment_quality, known.Count < visible_aspects.Count);
		} else {
			complete_request_row?.Hide();
		}
	}

	private void UpdateTreatmentQualityLabel(int treatment_quality, bool at_least) {
		var (text, color) = treatment_quality switch {
			< 0 => ("INADEQUATE_TREATMENT", at_least ? Colors.Orange : Colors.OrangeRed),
			0 => ("ADEQUATE_TREATMENT", Colors.LawnGreen),
			1 => ("GOOD_TREATMENT", Colors.LawnGreen.Lerp(Colors.LightSeaGreen, 0.3f)),
			2 => ("VERY_GOOD_TREATMENT", Colors.LawnGreen.Lerp(Colors.LightSeaGreen, 0.6f)),
			> 2 => ("EXCELLENT_TREATMENT", Colors.LightSeaGreen)
		};

		treatment_quality_label?.Text = Tr((at_least ? "AT_LEAST_" : "") + text);
		treatment_quality_label?.Modulate = color;
	}

	private void OnGive() {
		if (destination_slot?.Item is Item item && visitor != null) {
			var (payment, tip) = Game.Instance.GiveVisitor(visitor, item);
			if (payment != null || tip != null) {
				List<string> text = [];
				if (payment != null) {
					text.Add(string.Format(Tr("PAYMENT"), payment.Value.ToBbCodeString()));
				}
				if (tip != null) {
					text.Add(string.Format(Tr("TIP"), tip.Value.ToBbCodeString()));
				}

				if (text.Count > 0) {
					FloatingNotifications.Instance?.Create(string.Join("\n", text), give_button!.GlobalPosition);
				}
				
				destination_slot.Referencing = null;
				EmitSignalGiven();
			}
		} else {
			switch (visitor?.Special) {
				case SpecialRequest.None: break;
				case SpecialRequest.Bills:
					Game.Instance.PayBill(visitor);
					EmitSignalGiven();
					break;
				default: throw new ArgumentOutOfRangeException();
			}
		}
	}
}
