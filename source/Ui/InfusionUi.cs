using Godot;
using System.Collections.Generic;
using System.Linq;
namespace Apothecary;

public partial class InfusionUi : Panel {
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
	}

	public void Update() {
		foreach (var slot in input_slots) {
			slot.Update();
		}
		output_slot?.Update();

		var inputs = input_slots.Where(slot => slot.Item != null).Select(slot => slot.Item!.Value).ToList();
		if (inputs.Count == 0) {
			input_aspects?.Update([]);
			output_aspects?.Update([]);
			output_slot?.Output = null;
		} else {
			input_aspects?.Update(Item.CombineAspects(inputs.Select(item => item.Aspects)));
			var output = Item.Infusion(inputs);
			output_slot?.Output = output;
			output_aspects?.Update(output.Aspects);
		}
	}
}
