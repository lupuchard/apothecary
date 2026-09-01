using System;
using Godot;
namespace Apothecary;

public partial class ForagingResultUi : Button {
	[Signal] public delegate void AcquireEventHandler(TextureRect sprite);
	[Export] public int Index { get; set; }
	public Control? Child;
	private TextureRect? TextureRect;
	private Label? Label;
	public bool Empty { get; private set; }= false;

	public override void _Ready() {
		Child = (Control)GetChild(0);
		TextureRect = GetNode<TextureRect>("%TextureRect");
		Label = GetNode<Label>("%Label");
		Pressed += OnPressed;
		Disable();
	}

	public void OnPressed() {
		if (!Empty) {
			Game.Instance.AcquirePickupResult(Index);
			EmitSignalAcquire(TextureRect);
		}

		Empty = false;
		Disable();
	}

	public void Disable() {
		Disabled = true;
		Child?.Hide();
	}

	public void Enable(Pickup pickup) {
		Disabled = false;
		Child?.Show();
		
		switch (pickup.Type) {
			case PickupType.Empty: 
				Label?.Text = Tr("NO_FORAGE_FOUND");
				TextureRect?.Texture = ResourceLoader.Load<Texture2D>("res://assets/item/nothing.png");
				Empty = true;
				break;
			case PickupType.ItemModel:
				var item = pickup.Item!;
				Label?.Text = Tr(item.Id.ToUpper());
				TextureRect?.Texture = item.Sprite;
				Empty = false;
				break;
			case PickupType.Material:
				var material = pickup.Resource!.Value;
				Label?.Text = Tr(material.TrString());
				TextureRect?.Texture = ResourceLoader.Load<Texture2D>(material.SpritePath());
				Empty = false;
				break;
			default: 
				throw new ArgumentOutOfRangeException();
		}
	}
}
