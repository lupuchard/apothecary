using System.Collections.Generic;
using Godot;
namespace Apothecary;

public partial class JournalEntryUi : Panel {
	public ItemModel? Item { get; set; }
	
	private const int UNSELECTED_ID = -2;
	
	private Label? name_label;
	private TextureRect? sprite;
	private AspectListUi? aspect_list;
	
	private Control? journal_info;
	private OptionButton? where_option;
	private OptionButton? when_option;
	private OptionButton? rarity_option;

	private static readonly Dictionary<RegionModel, int> region_to_option_id = [];
	private static readonly Dictionary<int, RegionModel> option_id_to_region = [];
	
	private readonly ItemFindCondition[] item_find_conditions = [
		ItemFindCondition.None,
		ItemFindCondition.Morning, 
		ItemFindCondition.Afternoon, 
		ItemFindCondition.Night, 
		ItemFindCondition.AfterRaining, 
		ItemFindCondition.InMoonlight
	];

	private static void InitializeRegionOptionIds() {
		foreach (var region in Game.Instance.World.Regions) {
			var option_id = region_to_option_id.Count;
			region_to_option_id.Add(region, option_id);
			option_id_to_region.Add(option_id, region);
		}
	}

	public override void _Ready() {
		name_label = GetNode<Label>("%NameLabel");
		sprite = GetNode<TextureRect>("%TextureRect");
		aspect_list = GetNode<AspectListUi>("%AspectList");
		
		journal_info = GetNode<Control>("JournalInfo");

		if (region_to_option_id.Count == 0) {
			InitializeRegionOptionIds();
		}
		
		where_option = GetNode<OptionButton>("JournalInfo/WhereOption");
		where_option.Clear();
		where_option.AddItem("???", UNSELECTED_ID);
		foreach (var location in Game.Instance.World.Regions) {
			where_option.AddItem(Tr(location.Id.ToUpper()), region_to_option_id[location]);
		}
		
		when_option = GetNode<OptionButton>("JournalInfo/WhenOption");
		when_option.Clear();
		when_option.AddItem("???", UNSELECTED_ID);
		foreach (var condition in item_find_conditions) {
			when_option.AddItem(Tr(condition.TrString()), (int)condition);
		}
		
		rarity_option = GetNode<OptionButton>("JournalInfo/RarityOption");
		rarity_option.Clear();
		rarity_option.AddItem("???", UNSELECTED_ID);
		for (var rarity = 0; rarity < (int)Rarity.COUNT; rarity++) {
			rarity_option.AddItem(Tr(((Rarity)rarity).TrString()), rarity);
		}

		Update();
		where_option.ItemSelected += OnItemSelected;
		when_option.ItemSelected += OnItemSelected;
		rarity_option.ItemSelected += OnItemSelected;
		Game.Instance.Journal.Confirmation += (_) => Update();
	}

	public void Update() {
		var journal = Game.Instance.Journal;
		if (Item == null || journal.Get(Item) is not JournalEntry entry) {
			journal_info?.Hide();
			sprite?.Hide();
			aspect_list?.Hide();
			name_label?.Text = Tr("UNDISCOVERED_ITEM");
			return;
		}

		name_label?.Text = Tr(Item.Id.ToUpper());
		sprite?.Show();
		sprite?.Texture = Item.Sprite;
		aspect_list?.Show();
		aspect_list?.Update(journal.GetShownAspects([Item], Item.Aspects));

		journal_info?.Show();
		if (where_option == null || when_option == null || rarity_option == null) {
			return;
		}
		where_option.Selected = where_option.GetItemIndex(entry.LocationGuess == null ? UNSELECTED_ID : region_to_option_id[entry.LocationGuess]);
		when_option.Selected = when_option.GetItemIndex(entry.ConditionGuess == null ? UNSELECTED_ID : (int)entry.ConditionGuess);
		rarity_option.Selected = rarity_option.GetItemIndex(entry.RarityGuess == null ? UNSELECTED_ID : (int)entry.RarityGuess);
		where_option.Disabled = entry.Confirmed;
		when_option.Disabled = entry.Confirmed;
		rarity_option.Disabled = entry.Confirmed;
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
