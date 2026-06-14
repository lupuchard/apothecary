using Godot;
using System.Collections.Generic;

namespace Apothecary;

public partial class InventoryUi : PanelContainer {
	private const int WIDTH = 6;
	private VBoxContainer? container;
	private readonly List<HBoxContainer> rows = [];
	private readonly List<InventorySlot> slots = [];

	private PackedScene? inventory_slot_scene;
	private Area2D? drag_indicator;
	private Sprite2D? drag_indicator_sprite;
	private InputSlot? drag_snap_target;
	private ItemSlot? drag_snap_target_prev_item;
	private InventorySlot? dragging;

	public override void _Ready() {
		base._Ready();
		inventory_slot_scene = ResourceLoader.Load<PackedScene>("res://controls/inventory_slot.tscn");
		container = GetNode<VBoxContainer>("ScrollContainer/InventoryContainer");
		drag_indicator = GetNode<Area2D>("DragIndicator");
		drag_indicator_sprite = GetNode<Sprite2D>("DragIndicator/Sprite");
		drag_indicator.Hide();

		while (container.GetChildCount() > 0) {
			container.RemoveChild(container.GetChild(0));
		}

		var first_row = new HBoxContainer();
		rows.Add(first_row);
		container.AddChild(first_row);

		var input_item_slots = GetTree().GetNodesInGroup("input_item_slot");
		foreach (var node in input_item_slots) {
			if (node is InputSlot slot) {
				if (slot.IsNodeReady()) {
					SetUpSlot(slot);
				} else {
					node.Ready += () => SetUpSlot(slot);
				}
			}
		}
	}

	private void SetUpSlot(InputSlot slot) {
		slot.CollisionArea!.AreaEntered += (area) => {
			if (area == drag_indicator) {
				SnapDrag(slot);
			}
		};
		slot.CollisionArea.AreaExited += (area) => {
			if (area == drag_indicator) {
				EndSnapDrag(slot);
			}
		};
	}

	public override void _Input(InputEvent inputEvent) {
		if (inputEvent is InputEventMouseMotion mouseEvent && dragging != null) {
			drag_indicator?.Position += mouseEvent.Relative;
		}
	}

	public void StartDrag(InventorySlot slot) {
		var inventory = Game.Instance.GetInventory();
		if (slot.Index >= inventory.Count) return;

		dragging?.Dragging = false;

		drag_indicator?.Show();
		drag_indicator_sprite?.Texture = inventory[slot.Index].Item1.GetSprite();
		drag_indicator?.GlobalPosition = slot.GlobalPosition;
		dragging = slot;
		dragging.Dragging = true;
	}

	private void SnapDrag(InputSlot slot) {
		if (drag_snap_target != null) {
			EndSnapDrag(drag_snap_target);
		}
		drag_snap_target_prev_item = slot.Referencing;
		slot.Referencing = dragging;
		if (dragging?.ReferencedBy is InputSlot input_slot) {
			input_slot.Referencing = null;
		}
		dragging?.ReferencedBy = slot;
		drag_snap_target = slot;
		drag_indicator_sprite?.Hide();
	}

	private void EndSnapDrag(InputSlot slot) {
		if (slot == drag_snap_target) {
			drag_snap_target = null;
			slot.Referencing = drag_snap_target_prev_item;
			dragging?.ReferencedBy = null;
			drag_indicator_sprite?.Show();
		}
	}

	private void EndDrag(ItemSlot slot) {
		if (dragging == slot) {
			if (drag_snap_target_prev_item is InventorySlot inventory_slot) {
				inventory_slot.ReferencedBy = null;
			}
			drag_snap_target_prev_item = null;
			drag_snap_target = null;
			drag_indicator?.Hide();
			dragging.Dragging = false;
			dragging = null;
			drag_indicator_sprite?.Show();
		}
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

			var new_slot = (InventorySlot)inventory_slot_scene!.Instantiate();
			new_slot.Index = slots.Count;
			new_slot.ButtonDown += () => StartDrag(new_slot);
			new_slot.ButtonUp += () => EndDrag(new_slot);
			last_row.AddChild(new_slot);
			slots.Add(new_slot);
		}

		foreach (var slot in slots) {
			slot.Update();
		}
	}

	public void CloseUi() {
		if (drag_snap_target != null) {
			EndSnapDrag(drag_snap_target);
		}

		if (dragging != null) {
			EndDrag(dragging);
		}

		var input_item_slots = GetTree().GetNodesInGroup("input_item_slot");
		foreach (var node in input_item_slots) {
			if (node is InputSlot slot) {
				if (slot.Referencing is InventorySlot inventory_slot) {
					inventory_slot.ReferencedBy = null;
					slot.Referencing = null;
				}
			}
		}
	}
}
