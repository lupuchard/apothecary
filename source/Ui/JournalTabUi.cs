using System;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Apothecary;

public partial class JournalTabUi : MarginContainer {
	private GridContainer? grid;
	private BaseButton? prev_page_button;
	private BaseButton? next_page_button;
	private Label? page_label;

	private Control? verified_popup;
	private Label? verified_popup_label;
	private Button? verified_popup_button;

	private readonly List<JournalEntryUi> entries = [];
	private int page = 0;
	private int num_pages = 0;

	public override void _Ready() {
		grid = GetNode<GridContainer>("VBoxContainer/GridContainer");
		prev_page_button = GetNode<BaseButton>("%JournalPrevPageButton");
		next_page_button = GetNode<BaseButton>("%JournalNextPageButton");
		page_label = GetNode<Label>("%JournalPageLabel");

		foreach (var child in grid.GetChildren()) {
			if (child is JournalEntryUi entry) {
				entries.Add(entry);
			}
		}
		
		verified_popup = GetNode<Control>("VerifiedPopup");
		verified_popup_label = GetNode<Label>("%VerifiedPopupLabel");
		verified_popup_button = GetNode<Button>("%VerifiedPopupButton");
		verified_popup.Hide();

		Update();
		num_pages = (int)Math.Ceiling(Game.Instance.World.Items.Length / (double)(entries.Count));
		prev_page_button.Pressed += ToPrevPage;
		next_page_button.Pressed += ToNextPage;

		Game.Instance.JournalConfirmation += ShowPopup;
		verified_popup_button.Pressed += () => verified_popup.Hide();
	}

	private void ToPrevPage() {
		page = Math.Max(page - 1, 0);
		Update();
	}

	private void ToNextPage() {
		page = Math.Min(page + 1, num_pages - 1);
		Update();
	}

	private void ShowPopup(Godot.Collections.Array<string> confirmed_items) {
		verified_popup!.Show();
		verified_popup_label!.Text = string.Format(
			Tr("VERIFIED_POPUP_TEXT"), 
			confirmed_items.Select(x => "- " + Tr(x.ToUpper()) + "\n"),
			""
		);

		Update();
	}

	public void Update() {
		var all_items = Game.Instance.World.Items;
		var offset = page * entries.Count;

		for (var i = offset; i < offset + entries.Count; i++) {
			var entry = entries[i - offset];
			entry.Item = i < all_items.Length ? all_items[i] : null;
			entry.Update();
		}

		page_label?.Text = string.Format(Tr("X_OF_Y"), page + 1, num_pages);
		prev_page_button?.Disabled = page <= 0;
		next_page_button?.Disabled = page >= num_pages - 1;
	}
}
