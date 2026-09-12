using System.Collections.Immutable;
using System.Linq;
using Godot;

namespace Apothecary;

[GlobalClass]
public partial class ResourceSelectSlot : Button {
	public ImmutableArray<(Resource, int)?> Options {
		get;
		set {
			field = value;
			Update();
		}
	} = [];

	public int Selected {
		get; 
		set {
			field = value;
			Update();
		}
	} = 0;
	public Resource? SelectedResource => Selected >= Options.Length ? null : Options[Selected]?.Item1;
	public int? SelectedAmount => Selected >= Options.Length ? null : Options[Selected]?.Item2;

	private TextureRect? texture_rect;
	private Label? amount_label;
	private Control? dropdown_panel;
	private TextureRect? expand_icon;
	private ResourceSelectUi? select_ui;
	private bool options_open = false;
	
	private static readonly Texture2D? close_icon = ResourceLoader.Load<Texture2D>("res://assets/close.png");
	private static readonly Texture2D? expand_closed = ResourceLoader.Load<Texture2D>("res://assets/expand_closed.png");
	private static readonly Texture2D? expand_closed2 = ResourceLoader.Load<Texture2D>("res://assets/expand_closed2.png");
	private static readonly Texture2D? expand_opened2 = ResourceLoader.Load<Texture2D>("res://assets/expand_opened2.png");

	public override void _Ready() {
		base._Ready();
		texture_rect = GetNode<TextureRect>("%TextureRect");
		amount_label = GetNode<Label>("%AmountLabel");
		dropdown_panel = GetNode<Control>("%DropdownPanel");
		expand_icon = GetNode<TextureRect>("%ExpandIcon");
		Update();

		select_ui = (ResourceSelectUi)GetTree().GetFirstNodeInGroup("ResourceSelect");
		Pressed += OnPress;
		MouseEntered += OnHover;
		MouseExited += OnEndHover;
		select_ui.ResourceSelected += OnResourceSelected;
		select_ui.Close += OnCloseSelect;
	}

	public void OnHover() {
		if (options_open) return;
		expand_icon?.Texture = expand_closed;
	}

	public void OnEndHover() {
		if (options_open) return;
		expand_icon?.Texture = expand_closed2;
	}

	public void OnPress() {
		expand_icon?.Texture = expand_opened2;
		options_open = true;
		select_ui?.Show([..Options.Select(x => x?.Item1)], this);
	}

	public void Update() {
		texture_rect?.Texture = SelectedResource == null 
			? close_icon 
			: ResourceLoader.Load<Texture2D>(SelectedResource.Value.SpritePath());
		texture_rect?.Modulate = SelectedResource?.GetColor() ?? Colors.White;
		amount_label?.Text = SelectedAmount > 1 ? ("x" + SelectedAmount) : "";
		//dropdown_panel?.Visible = Options.Length > 1;
		
		if (IsHovered()) {
			OnHover();
		} else {
			OnEndHover();
		}
	}

	private void OnResourceSelected(Control where, int index) {
		if (where != this) return;
		options_open = false;
		Selected = index;
	}

	private void OnCloseSelect(Control where) {
		if (where != this) return;
		options_open = false;
		Update();
	}
}
