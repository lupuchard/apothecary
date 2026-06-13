using Godot;
using System.Collections.Generic;

namespace Apothecary;

public partial class InventoryUi : PanelContainer {
	private const int WIDTH = 6;
	private VBoxContainer? container;
	private readonly List<HBoxContainer> rows = [];
	private readonly List<ItemSlot> slots = [];

	private PackedScene? item_slot_scene;

	public override void _Ready() {
		base._Ready();
		item_slot_scene = ResourceLoader.Load<PackedScene>("res://controls/item_slot.tscn");
		container = GetNode<VBoxContainer>("ScrollContainer/InventoryContainer");

		while (container.GetChildCount() > 0) {
			container.RemoveChild(container.GetChild(0));
		}

		var first_row = new HBoxContainer();
		rows.Add(first_row);
		container.AddChild(first_row);
	}

	public void Update() {
		if (container == null) return;
		
		var inventory = Game.Instance.GetInventory();
		while (slots.Count < inventory.Count) {
			var last_row = rows[^1];
			if (last_row.GetChildCount() >= WIDTH) {
				var new_row = new HBoxContainer();
				rows.Add(new_row);
				last_row = new_row;
				container.AddChild(last_row);
			}

			var new_slot = (ItemSlot)item_slot_scene!.Instantiate();
			new_slot.Index = slots.Count;
			last_row.AddChild(new_slot);
			slots.Add(new_slot);
		}

		foreach (var slot in slots) {
			slot.Update();
		}
	}
}
