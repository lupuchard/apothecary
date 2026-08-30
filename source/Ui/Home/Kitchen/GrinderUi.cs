namespace Apothecary;

public partial class GrinderUi : KitchenProcessBaseUi {
	private SlowButton? button;
	private InputSlot? input_slot;
	private OutputSlot? output_slot;
	private AspectListUi? input_aspects;
	private AspectListUi? output_aspects;

	public override void _Ready() {
		button = GetNode<SlowButton>("HoldButton");
		input_slot = GetNode<InputSlot>("HBoxContainer/InputSlot");
		output_slot = GetNode<OutputSlot>("HBoxContainer/OutputSlot");
		input_slot.ItemUpdated += Update;

		input_aspects = GetNode<AspectListUi>("InputAspects");
		output_aspects = GetNode<AspectListUi>("OutputAspects");

		button.Pressed += DoGrind;
	}

	public override void Update() {
		input_slot?.Update();
		output_slot?.Update();

		if (input_slot?.Item is Item item) {
			var output = Item.Ground(item);
			input_aspects?.Update(Game.Instance.Journal.GetShownAspects(output.Raw, item.Aspects));
			output_slot?.Output = output;
			output_aspects?.Update(Game.Instance.Journal.GetShownAspects(output.Raw, output.Aspects));
			button?.Disabled = output.Aspects.Count == 0;
		} else {
			input_aspects?.Update([]);
			output_aspects?.Update([]);
			output_slot?.Output = null;
			button?.Disabled = true;
		}
	}

	private void DoGrind() {
		var item = input_slot?.Item;
		if (input_slot == null || item == null) return;

		Game.Instance.RemoveItem(item.Value, input_slot.Amount);

		if (output_slot?.Output != null) {
			Game.Instance.AcquireItem(output_slot.Output.Value, output_slot.OutputAmount);
		}

		input_slot.Referencing?.ReferencedBy = null;
		input_slot.Referencing = null;
		
		EmitSignalOutputCreated();
		Game.Instance.PassTime();
	}
	
	public override bool IsUnlocked() {
		return Game.Instance.IsUnlocked(Feature.Grinder);
	}
}
