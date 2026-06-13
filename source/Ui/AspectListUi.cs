using Godot;
using System.Collections.Generic;

namespace Apothecary;

public partial class AspectListUi : HBoxContainer {
	private static readonly StyleBox empty_style_box = new StyleBoxEmpty();
	private readonly List<Label> amount_labels = [];
	private readonly List<TextureRect> sprites = [];
	private readonly List<VSeparator> separators = [];
	
	public override void _Ready() {
		foreach (var child in GetChildren()) {
			RemoveChild(child);
			child.QueueFree();
		}

		AddThemeConstantOverride("separation", 1);
		AddLabelAndSprite();
	}

	public void Update(IList<(Aspect, int)> aspects) {
		while (sprites.Count < aspects.Count) {
			AddSeparator();
			AddLabelAndSprite();
		}

		var i = 0;
		foreach (var (aspect, count) in aspects) {
			amount_labels[i].Text = $"{count}";
			amount_labels[i].Show();
			sprites[i].Texture = aspect.Sprite;
			sprites[i].Modulate = aspect.Color;
			sprites[i].Show();
			if (i > 0) separators[i - 1].Show();
			i++;
		}

		for (; i < sprites.Count; i++) {
			amount_labels[i].Hide();
			sprites[i].Hide();
			if (i > 0) separators[i - 1].Hide();
		}
	}

	private void AddSeparator() {
		var new_separator = new VSeparator();
		new_separator.AddThemeConstantOverride("separation", 5);
		new_separator.AddThemeStyleboxOverride("separator", empty_style_box);
		separators.Add(new_separator);
		AddChild(new_separator);
	}

	private void AddLabelAndSprite() {
		var new_label = new Label();
		new_label.ThemeTypeVariation = "LabelSmall";
		amount_labels.Add(new_label);
		AddChild(new_label);

		var new_sprite = new TextureRect();
		new_sprite.StretchMode = TextureRect.StretchModeEnum.Keep;
		sprites.Add(new_sprite);
		AddChild(new_sprite);
	}
}
