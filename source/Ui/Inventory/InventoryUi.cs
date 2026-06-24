using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Apothecary;

public partial class InventoryUi : PanelContainer {
	[Export] public Node? WhereInputNodes;

	private const int WIDTH = 6;
	private VBoxContainer? container;
	private readonly List<HBoxContainer> rows = [];
	private readonly List<InventorySlot> slots = [];
	private List<InputSlot> input_slots = [];

	private PackedScene? inventory_slot_scene;
	private Area2D? drag_indicator;
	private Sprite2D? drag_indicator_sprite;
	private InputSlot? drag_snap_target;
	private InventorySlot? drag_snap_target_prev_item;
	private InventorySlot? dragging;

	private Node2D? hover_info;
	private Label? hover_info_name;
	private AspectListUi? hover_info_aspects;
	private InventorySlot? hovering = null;

	public override void _Ready() {
		base._Ready();
		inventory_slot_scene = ResourceLoader.Load<PackedScene>("res://controls/inventory_slot.tscn");
		container = GetNode<VBoxContainer>("ScrollContainer/InventoryContainer");
		drag_indicator = GetNode<Area2D>("DragIndicator");
		drag_indicator_sprite = GetNode<Sprite2D>("DragIndicator/Sprite");
		drag_indicator.Hide();

		hover_info = GetNode<Node2D>("HoverInfo");
		hover_info_name = GetNode<Label>("%NameLabel");
		hover_info_aspects = GetNode<AspectListUi>("%AspectList");
		hover_info.Hide();

		while (container.GetChildCount() > 0) {
			container.RemoveChild(container.GetChild(0));
		}

		var first_row = new HBoxContainer();
		rows.Add(first_row);
		container.AddChild(first_row);
	}

	private void CheckForNewInputSlots() {
		foreach (var child in GetTree().GetNodesInGroup("new_input_slot")) {
			if (WhereInputNodes?.IsAncestorOf(child) == true && child is InputSlot input_slot) {
				if (input_slot.IsNodeReady()) {
					SetUpSlot(input_slot);
				} else {
					input_slot.Ready += () => SetUpSlot(input_slot);
				}
				input_slot.RemoveFromGroup("new_input_slot");
			}
		}
	}

	private void SetUpSlot(InputSlot slot) {
		slot.ButtonDown += () => {
			if (slot.Referencing != null) {
				drag_snap_target = slot;
				drag_indicator?.GlobalPosition = slot.GlobalPosition;
				StartDrag(slot.Referencing);
				
			}
		};

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

	public override void _Input(InputEvent input_event) {
		if (input_event is InputEventMouseMotion mouseEvent) {
			if (dragging != null) {
				drag_indicator?.Position += mouseEvent.Relative;
			}

			if (hovering != null) {
				hover_info?.GlobalPosition = mouseEvent.GetGlobalPosition();
			}
		} else if (input_event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false } && dragging != null) {
			EndDrag();
		}
	}

	public void StartDrag(InventorySlot slot) {
		var inventory = Game.Instance.GetInventory();
		if (slot.Index >= inventory.Count) return;

		dragging?.Dragging = false;

		drag_indicator?.Show();
		drag_indicator_sprite?.Texture = inventory[slot.Index].Item1.GetSprite();
		dragging = slot;
		dragging.Dragging = true;
		hovering = null;
		hover_info?.Hide();
	}

	private void SnapDrag(InputSlot slot) {
		if (slot.Referencing == dragging) {
			return;
		}
		if (slot.ItemType != null && (dragging?.Item?.Type & slot.ItemType) == 0) {
			return;
		}
		
		if (drag_snap_target != null && drag_snap_target != slot) {
			EndSnapDrag(drag_snap_target);
		}
		drag_snap_target_prev_item = slot.Referencing;
		slot.Referencing = dragging;
		if (dragging?.ReferencedBy is InputSlot input_slot && input_slot != slot) {
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

	private void EndDrag() {
		drag_snap_target_prev_item?.ReferencedBy = null;
		drag_snap_target_prev_item = null;
		drag_snap_target = null;
		drag_indicator?.Hide();
		dragging?.Dragging = false;
		dragging = null;
		drag_indicator_sprite?.Show();
	}

	private void StartHover(InventorySlot slot) {
		if (dragging != null || slot.Item is not Item item) return;
		hover_info?.Show();
		hover_info_name?.Text = item.GetName();
		hover_info_aspects?.Update(Game.Instance.Journal.GetShownAspects(item.Raw, item.Aspects));
		hovering = slot;
	}

	private void EndHover(InventorySlot slot) {
		if (hovering == slot) {
			hover_info?.Hide();
			hovering = null;
		}
	}

	public void Update() {
		if (container == null) return;

		CheckForNewInputSlots();
		
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
			new_slot.ButtonDown += () => {
				StartDrag(new_slot);
				drag_indicator?.GlobalPosition = new_slot.GlobalPosition;
			};
			new_slot.MouseEntered += () => StartHover(new_slot);
			new_slot.MouseExited += () => EndHover(new_slot);
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
			EndDrag();
		}

		foreach (var slot in input_slots) {
			slot.Referencing?.ReferencedBy = null;
			slot.Referencing = null;
		}
	}
}
