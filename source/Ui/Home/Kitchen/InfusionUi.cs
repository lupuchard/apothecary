using System.Collections.Generic;
using System.Linq;
namespace Apothecary;

public partial class InfusionUi : KitchenProcessBaseUi {
	private SlowButton? button;
	private List<InputSlot> input_slots = [];
	private OutputSlot? output_slot;
	private AspectListUi? input_aspects;
	private AspectListUi? output_aspects;

	public override void _Ready() {
		button = GetNode<SlowButton>("HoldButton");
		input_slots = [
			GetNode<InputSlot>("HBoxContainer/InputSlot1"),
			GetNode<InputSlot>("HBoxContainer/InputSlot2"),
			GetNode<InputSlot>("HBoxContainer/InputSlot3")
		];
		output_slot = GetNode<OutputSlot>("HBoxContainer/OutputSlot");

		foreach (var slot in input_slots) {
			slot.ItemUpdated += Update;
		}

		input_aspects = GetNode<AspectListUi>("InputAspects");
		output_aspects = GetNode<AspectListUi>("OutputAspects");

		button.Pressed += DoInfusion;
	}

	public override void Update() {
		foreach (var slot in input_slots) {
			slot.Update();
		}
		output_slot?.Update();

		var inputs = input_slots.Where(slot => slot.Item != null).Select(slot => slot.Item!.Value).ToList();
		if (inputs.Count == 0) {
			input_aspects?.Update([]);
			output_aspects?.Update([]);
			output_slot?.Output = null;
			button?.Disabled = true;
		} else {
			var output = Item.Infusion(inputs);

			var combined_aspects = Item.CombineAspects(inputs.Select(item => item.Aspects));
			input_aspects?.Update(Game.Instance.Journal.GetShownAspects(output.Raw, combined_aspects));
			output_slot?.Output = output;
			output_aspects?.Update(Game.Instance.Journal.GetShownAspects(output.Raw, output.Aspects));
			button?.Disabled = output.Aspects.Count == 0;
		}
	}

	private void DoInfusion() {
		var items = input_slots.Where(slot => slot.Item != null).Select(slot => (slot.Item!.Value, slot.Amount)).ToList();
		foreach (var (item, amount) in items) {
			Game.Instance.RemoveItem(item, amount);
		}

		if (output_slot?.Output != null) {
			Game.Instance.AcquireItem(output_slot.Output.Value, output_slot.OutputAmount);
		}

		foreach (var slot in input_slots) {
			slot.Referencing?.ReferencedBy = null;
			slot.Referencing = null;
		}
		
		EmitSignalOutputCreated();
	}
}
