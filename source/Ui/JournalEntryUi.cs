using System;
using System.Collections.Generic;
using Godot;
using static Apothecary.ItemFindCondition;
namespace Apothecary;

public partial class JournalEntryUi : Panel {
	[Signal] public delegate void NotesOpenedEventHandler();
	
	public ItemModel? Item { get; set; }
	
	private const int UNSELECTED_ID = -2;
	
	private Label? name_label;
	private TextureRect? sprite;
	private AspectListUi? aspect_list;
	
	private Control? journal_info;
	private OptionButton? where_option;
	private OptionButton? when_option;
	private OptionButton? rarity_option;

	private TextureButton? notes_button;
	private Texture2D? notes_texture;
	private Texture2D? notes_texture2;

	private static readonly Dictionary<RegionModel, int> region_to_option_id = [];
	private static readonly Dictionary<int, RegionModel> option_id_to_region = [];
	
	/*private readonly ItemFindCondition[] item_find_conditions = [
		None,
		Morning, 
		Afternoon, 
		ItemFindCondition.Night, 
		ItemFindCondition.AfterRaining, 
		ItemFindCondition.InMoonlight
	];*/

	private static void InitializeRegionOptionIds() {
		foreach (var region in Game.Instance.World.Regions) {
			var option_id = region_to_option_id.Count;
			region_to_option_id.Add(region, option_id);
			option_id_to_region.Add(option_id, region);
		}
	}

	public override void _Ready() {
		notes_texture = GD.Load<Texture2D>("res://assets/note.png");
		notes_texture2 = GD.Load<Texture2D>("res://assets/note2.png");
		
		name_label = GetNode<Label>("%NameLabel");
		sprite = GetNode<TextureRect>("%TextureRect");
		aspect_list = GetNode<AspectListUi>("%AspectList");
		
		journal_info = GetNode<Control>("JournalInfo");

		if (region_to_option_id.Count == 0) {
			InitializeRegionOptionIds();
		}
		
		rarity_option = GetNode<OptionButton>("JournalInfo/RarityOption");
		rarity_option.Clear();
		rarity_option.AddItem("???", UNSELECTED_ID);
		for (var rarity = 0; rarity < (int)Rarity.COUNT; rarity++) {
			rarity_option.AddItem(Tr(((Rarity)rarity).TrString()), rarity);
		}
		
		where_option = GetNode<OptionButton>("JournalInfo/WhereOption");
		when_option = GetNode<OptionButton>("JournalInfo/WhenOption");

		notes_button = GetNode<TextureButton>("%NotesButton");
		notes_button.Pressed += EmitSignalNotesOpened;

		Update();
		where_option.ItemSelected += OnItemSelected;
		when_option.ItemSelected += OnItemSelected;
		rarity_option.ItemSelected += OnItemSelected;
		Game.Instance.JournalConfirmation += (_) => Update();
	}

	public void Update() {
		var journal = Game.Instance.Journal;
		if (Item == null || journal.Get(Item) is not JournalEntry entry) {
			journal_info?.Hide();
			sprite?.Hide();
			aspect_list?.Hide();
			notes_button?.Hide();
			name_label?.Text = Tr("UNDISCOVERED_ITEM");
			return;
		}
		
		notes_button?.TextureNormal = string.IsNullOrWhiteSpace(entry.Notes) ? notes_texture : notes_texture2;

		name_label?.Text = Tr(Item.Id.ToUpper());
		sprite?.Show();
		sprite?.Texture = Item.Sprite;
		aspect_list?.Show();
		aspect_list?.Update(journal.GetShownAspects([Item], Item.Aspects));
		notes_button?.Show();

		journal_info?.Show();
		if (where_option == null || when_option == null || rarity_option == null) {
			return;
		}
		
		where_option.Clear();
		where_option.AddItem("???", UNSELECTED_ID);
		foreach (var location in Game.Instance.World.Regions) {
			var region = Game.Instance.GetRegion(location.Id);
			if (region?.Known != true) continue;
			where_option.AddItem(Tr(region.TrString()), region_to_option_id[location]);
		}
		
		when_option.Clear();
		when_option.AddItem("???", UNSELECTED_ID);
		foreach (var condition in GetItemFindConditions()) {
			when_option.AddItem(Tr(condition.TrString()), (int)condition);
		}
		
		where_option.Selected = where_option.GetItemIndex(entry.LocationGuess == null ? UNSELECTED_ID : region_to_option_id[entry.LocationGuess]);
		when_option.Selected = when_option.GetItemIndex(entry.ConditionGuess == null ? UNSELECTED_ID : (int)entry.ConditionGuess);
		rarity_option.Selected = rarity_option.GetItemIndex(entry.RarityGuess == null ? UNSELECTED_ID : (int)entry.RarityGuess);
		where_option.Disabled = entry.Confirmed;
		when_option.Disabled = entry.Confirmed;
		rarity_option.Disabled = entry.Confirmed;
	}
	
	private static readonly ItemFindCondition[] FirstEstival = [None, Morning, Afternoon, AfterRaining];
	private static readonly ItemFindCondition[] Estival   = [None, Morning, Afternoon, AfterRaining, HeatWave, Wind];
	private static readonly ItemFindCondition[] Serotinal = [None, Morning, Afternoon, AfterRaining, Night, Wind];
	private static readonly ItemFindCondition[] Autumnal  = [None, Daytime, AfterRaining, Night, InMoonlight, Wind];
	private static readonly ItemFindCondition[] Hibernal  = [None, Daytime, Night, InMoonlight, Wind, Snowing];
	private static readonly ItemFindCondition[] Prevernal = [None, Daytime, AfterRaining, Night, InMoonlight, Wind];
	private static readonly ItemFindCondition[] Vernal    = [None, Morning, Afternoon, AfterRaining, Night, Wind];

	private static ItemFindCondition[] GetItemFindConditions() {
		return Game.Instance.Season switch {
			Season.Prevernal => Prevernal,
			Season.Vernal => Vernal,
			Season.Estival => Game.Instance.Year > 0 ? Estival : FirstEstival,
			Season.Serotinal => Serotinal,
			Season.Autumnal => Autumnal,
			Season.Hibernal => Hibernal,
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private void OnItemSelected(long _) {
		var journal = Game.Instance.Journal;
		if (Item == null || journal.Get(Item) is not JournalEntry entry) {
			return;
		}

		var where = where_option!.GetSelectedId();
		var when = when_option!.GetSelectedId();
		var rarity = rarity_option!.GetSelectedId();
		var new_entry = entry with {
			LocationGuess = where == UNSELECTED_ID ? null :  option_id_to_region[where],
			ConditionGuess = when == UNSELECTED_ID ? null : (ItemFindCondition?)when,
			RarityGuess = rarity == UNSELECTED_ID ? null : (Rarity?)rarity,
		};
		journal.UpdateGuess(new_entry);
	}
}
