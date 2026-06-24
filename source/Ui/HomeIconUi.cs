using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Apothecary;

public partial class HomeIconUi : TextureButton {
	private Path2D? acquire_item_path;
	private Sprite2D? bag_sprite;
	private Vector2 bag_start;
	private Vector2 bag_end;

	private class ItemInMotion(Sprite2D sprite, Vector2 starting_position, double distance) {
		public readonly Sprite2D sprite = sprite;
		public readonly Vector2 starting_position = starting_position;
		public double distance = distance;
	}
	
	private List<ItemInMotion> items_in_motion = [];
	
	public override void _Ready() {
		foreach (var foraging_result in GetNode<ForagingUi>("%ForagingUi").ForagingResultsControls) {
			foraging_result.Acquire += OnAcquire;
		}
		
		acquire_item_path = GetNode<Path2D>("%AcquireItemPath");
		bag_sprite = GetNode<Sprite2D>("BagMask/BagSprite");
		bag_start = GetNode<Node2D>("BagStart").GlobalPosition;
		bag_end = bag_sprite.GlobalPosition;
		bag_sprite.GlobalPosition = bag_start;
	}

	public override void _Process(double delta) {
		if (acquire_item_path == null) return;
		
		foreach (var item in items_in_motion) {
			item.distance += delta * 2.0;
			if (item.distance >= 1.0) {
				item.sprite.QueueFree();
			} else {
				var curve = acquire_item_path.Curve;
				var item_path_pos = curve.SampleBaked((float)item.distance * curve.GetBakedLength(), true);
				item.sprite.Position = item_path_pos / curve.GetPointPosition(0) * item.starting_position;

				if (item.distance > 0.8) {
					item.sprite.Scale = Vector2.One * (float)(1.0 - (item.distance - 0.8) / 0.2);
				}
			}
		}

		items_in_motion = [..items_in_motion.Where(item => item.distance < 1.0)];

		if (bag_sprite != null) {
			if (items_in_motion.Count > 0) {
				bag_sprite.GlobalPosition = bag_sprite.GlobalPosition.Lerp(bag_end, (float)delta * 6.0f);
			} else {
				bag_sprite.GlobalPosition = bag_sprite.GlobalPosition.Lerp(bag_start, (float)delta * 6.0f);
			}
		}
	}

	private void OnAcquire(TextureRect sprite) {
		var new_item = new Sprite2D { Texture = sprite.Texture };
		acquire_item_path!.AddChild(new_item);
		new_item.GlobalPosition = sprite.GlobalPosition + sprite.CustomMinimumSize / 2.0f;
		items_in_motion.Add(new ItemInMotion(new_item, new_item.Position, 0.0));
	}
}
