using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;
using MessagePack;

namespace Apothecary;

public partial class Game : RefCounted {
	public static Game Instance { get; private set; } = new();
	
	[Signal] public delegate void TimeChangedEventHandler();
	[Signal] public delegate void RegionUnlockedEventHandler(string region_id);
	[Signal] public delegate void ResourceUpdatedEventHandler(Resource resource, int amount);
	[Signal] public delegate void JournalConfirmationEventHandler(Godot.Collections.Array<string> items);
	
	private const int MAX_FORAGE_RESULTS = 3;
	public const int END_OF_DAY = 6;
	private const int DAYS_IN_SEASON = 36;
	private const float VISITOR_CHANCE = 0.4f;

	public readonly World World = CreateWorld();

	[MessagePackObject(keyAsPropertyName: true)]
	public class GameState(Journal journal) {
		public Rando rando = new (seed: 2);
		public int day = 0;
		public Season season = Season.Estival;
		public int time_of_day = 0;
		public int year = 0;

		public List<Region> regions = [];
		public Dictionary<Item, int> inventory = [];

		public Visitor? visitor_at_door = null;
		public List<Visitor> current_requests = [];

		public Journal journal = journal;
		
		public List<int> resources = [];
	}

	public GameState state { get; private set; }

	public int TimeOfDay => state.time_of_day;

	private readonly Dictionary<(long, RegionModel), List<ItemModel>> foraging_possibilities_cache = [];
	private List<ItemModel?> current_foraging_results = [];

	private readonly Dictionary<string, Region> region_lookup = [];

	private List<(Item, int)>? sorted_inventory = [];
	//[Key("inventory_aspect_filter")] private Aspect? inventory_aspect_filter = null;
	//[Key("inventory_type_filter")] private ItemType? inventory_type_filter = null;
	private InventorySortMethod inventory_sort_method = InventorySortMethod.Index;
	private bool inventory_sort_descending = false;

	public Visitor? VisitorAtDoor => state.visitor_at_door;
	public IReadOnlyList<Visitor> CurrentRequests => state.current_requests;

	public Journal Journal => state.journal;
	private readonly Dictionary<UnlockRequirementType, List<RegionModel>> region_unlocks = [];

	public Game() {
		state = new(new Journal());
		state.journal.OnConfirmation = OnJournalConfirmation;
			
		for (var i = 1; i < (int)UnlockRequirementType.COUNT; i++) {
			region_unlocks.Add((UnlockRequirementType)i, []);
		}
		
		foreach (var region in World.Regions) {
			var new_region = new Region(region);
			state.regions.Add(new_region);
			region_lookup.Add(region.Id, new_region);

			if (region.UnlockRequirement.Type == UnlockRequirementType.None) {
				new_region.Unlocked = true;
			} else {
				region_unlocks[region.UnlockRequirement.Type].Add(region);
			}
		}
		
		for (var i = 1; i < (int)UnlockRequirementType.COUNT; i++) {
			var type = (UnlockRequirementType)i;
			region_unlocks[type] = [..region_unlocks[type].OrderBy(x => x.UnlockRequirement.Amount)];
		}

		for (var i = 0; i < (int)Resource.COUNT; i++) {
			state.resources.Add(0);
		}
		
		MakeNewVisitor(World.GetRequest("pain"));
	}

	public static void LoadGame(GameState state) {
		Instance = new Game();
		Instance.state = state;
	}

	public static void NewGame() {
		Instance = new Game();
	}

	public int GetResource(Resource resource) {
		return state.resources[(int)resource];
	}

	public void ModifyResource(Resource resource, int amount) {
		state.resources[(int)resource] += amount;
		EmitSignalResourceUpdated(resource, Math.Max(amount, 0));
	}

	public void GetReward(Reward reward) {
		foreach (var (resource, amount) in reward.Rewards) {
			var old_amount = GetResource(resource);
			ModifyResource(resource, amount);
			
			// TODO a bit ugly
			var unlocked = GetUnlocksWith(UnlockRequirement.ResourceAcquired(resource, amount), old_amount);
			foreach (var region_model in unlocked) {
				if (region_model.UnlockRequirement.Resource == resource) {
					var region = region_lookup[region_model.Id];
					if (!region.Unlocked) {
						region.Unlocked = true;
						EmitSignalRegionUnlocked(region_model.Id);
					}
				}
			}
		}
	}
	
	public void PassTime() {
		if (TimeOfDay < END_OF_DAY) {
			state.time_of_day += 1;
			EmitSignalTimeChanged();
		}
	}

	public void NextDay() {
		state.day += 1;
		state.time_of_day = 0;
		if (state.day >= DAYS_IN_SEASON) {
			state.season += 1;
			state.day = 0;
			if (state.season > Season.Hibernal) {
				state.season = Season.Prevernal;
				state.year += 1;
			}
		}

		foreach (var location in state.regions) {
			location.DailyRecovery(ref state.rando);
		}

		UpdateVisitors();
		EmitSignalTimeChanged();
	}

	public IReadOnlyList<ItemModel?> CurrentForagingResults() {
		return current_foraging_results;
	}

	public void DoForaging(RegionModel location) {
		if (TimeOfDay >= END_OF_DAY) {
			GD.PushError("Can't forage at end of day");
			return;
		}

		var region = GetRegion(location.Id);
		if (region?.Remaining > 0) {
			region.ConsumeForage();
			current_foraging_results = [..GetForagingResults(location).Cast<ItemModel?>()];
		}

		PassTime();
	}

	public void AcquireForagingResult(int index) {
		if (index < current_foraging_results.Count && current_foraging_results[index] is { } item_model) {
			var item = new Item(item_model);
			AcquireItem(item);
			current_foraging_results[index] = null;
		}
	}

	public void AcquireItem(Item item, int amount = 1) {
		ref var cur_amount = ref CollectionsMarshal.GetValueRefOrAddDefault(state.inventory, item, out _);
		//if (!exists) value.Item2 = item;
		cur_amount += amount;
		sorted_inventory = null;
		Journal.Discover(item.Raw[0]);
	}

	public void RemoveItem(Item item, int amount = 1) {
		ref var cur_amount = ref CollectionsMarshal.GetValueRefOrNullRef(state.inventory, item);
		if (cur_amount <= amount) {
			state.inventory.Remove(item);
		} else {
			cur_amount -= amount;
		}

		sorted_inventory = null;
	}

	public List<(Item, int)> GetInventory() {
		if (sorted_inventory == null) {
			sorted_inventory = [.. state.inventory.Select(x => (x.Key, x.Value))];
			sorted_inventory.Sort(GetInventoryComparer());
		}

		return sorted_inventory;
	}

	private Comparison<(Item, int)> GetInventoryComparer() {
		return inventory_sort_method switch {
			InventorySortMethod.Index => (lhs, rhs) => Item.CompareIndexes(lhs.Item1, rhs.Item1),
			InventorySortMethod.Name => (lhs, rhs) => string.Compare(lhs.Item1.GetName(), rhs.Item1.GetName(), StringComparison.Ordinal),
			InventorySortMethod.Type => (lhs, rhs) => lhs.Item1.Type - rhs.Item1.Type,
			_ => (lhs, rhs) => Item.CompareIndexes(lhs.Item1, rhs.Item1),
		};
	}

	public Region? GetRegion(string region_id) {
		return region_lookup.GetValueOrDefault(region_id);
	}

	private List<ItemModel> GetForagingResults(RegionModel location) {
		var forage_key = (GetCurrentConditions(), location);
		var possibilities = GetForagingPossibilities(forage_key);
		possibilities = state.rando.Shuffle([.. possibilities.Concat(possibilities)]);

		var results = new List<ItemModel>();
		foreach (var item in possibilities) {
			if (state.rando.RandDouble() < GetItemForageProbability(item)) {
				results.Add(item);
				if (results.Count >= MAX_FORAGE_RESULTS) {
					break;
				}
			}
		}

		return results;
	}

	private static double GetItemForageProbability(ItemModel item) => item.Rarity switch {
		Rarity.Common => 0.8,
		Rarity.Rare => 0.09,
		Rarity.Scarce => 0.01,
		_ => throw new InvalidEnumArgumentException("item", (int)item.Rarity, typeof(Rarity))
	};

	private long GetCurrentConditions() {
		long conditions = 0;

		if (!IsItDaytime()) {
			conditions |= (long)ItemFindCondition.Night;
		}
		if (IsItMorning()) {
			conditions |= (long)ItemFindCondition.Morning;
		}
		if (IsItAfternoon()) {
			conditions |= (long)ItemFindCondition.Afternoon;
		}

		return conditions;
	}

	private List<ItemModel> GetForagingPossibilities((long conditions, RegionModel location) forage_key) {
		if (foraging_possibilities_cache.TryGetValue(forage_key, out var possibilities)) {
			return possibilities;
		}

		possibilities = [];
		foreach (var item in World.Items) {
			if (item.WhenFound != ItemFindCondition.None && (forage_key.conditions & (long)item.WhenFound) == 0) {
				continue;
			}

			if (item.WhereFound == forage_key.location || (World.Adjacencies.GetValueOrDefault(item.WhereFound)?.Contains(forage_key.location) ?? false)) {
				possibilities.Add(item);
			}
		}

		foraging_possibilities_cache.Add(forage_key, possibilities);
		return possibilities;
	}

	public bool IsItDaytime() {
		return state.season switch {
			Season.Estival => true,
			Season.Vernal or Season.Serotinal => TimeOfDay < 5,
			Season.Prevernal or Season.Autumnal => TimeOfDay < 4,
			Season.Hibernal => TimeOfDay < 3,
			_ => throw new InvalidEnumArgumentException("season", (int)state.season, typeof(Season))
		};
	}

	public bool IsItMorning() {
		return state.season switch {
			Season.Estival => TimeOfDay < 3,
			Season.Vernal or Season.Serotinal => TimeOfDay < 2,
			Season.Prevernal or Season.Autumnal => TimeOfDay < 2,
			Season.Hibernal => TimeOfDay < 1,
			_ => throw new InvalidEnumArgumentException("season", (int)state.season, typeof(Season)),
		};
	}

	public bool IsItAfternoon() {
		return state.season switch {
			Season.Estival or Season.Vernal or Season.Serotinal => TimeOfDay >= 3,
			Season.Prevernal or Season.Autumnal or Season.Hibernal => TimeOfDay >= 2,
			_ => throw new InvalidEnumArgumentException("season", (int)state.season, typeof(Season))
		};
	}

	private void UpdateVisitors() {
		foreach (var visitor in state.current_requests) {
			visitor.RemainingDays -= 1;
		}
		state.current_requests = [.. state.current_requests.Where(v => v.RemainingDays <= 0)];
		state.visitor_at_door = null;

		if (state.rando.RandDouble() < VISITOR_CHANCE) {
			MakeNewVisitor();
		}
	}

	private void MakeNewVisitor(RequestModel? request_type = null) {
		var model = request_type ?? state.rando.Pick(World.Requests);
		state.visitor_at_door = new Visitor(model, ref state.rando);
	}

	public Reward? GiveVisitor(Visitor visitor, Item treatment) {
		if (!state.inventory.ContainsKey(treatment)) return null;
		if (!treatment.Is(ItemType.Infusion)) return null;

		var quality = CalculateTreatmentQuality(visitor, treatment.Aspects);
		var rewards = visitor.Request.Type.Reward.Select(res => (res, visitor.Request.Reward));
		var tip = visitor.Request.Type.Tip.Select(res => (res, quality));
		var reward = new Reward(rewards.Concat(tip));
		GetReward(reward);
		state.current_requests.Remove(visitor);
		RemoveItem(treatment);
		return reward;
	}

	public static int CalculateTreatmentQuality(Visitor visitor, IList<(Aspect, int)> aspects) {
		var prevAspect = int.MaxValue;
		var quality = 0;
		foreach (var (aspect, amount) in visitor.Request.Aspects) {
			var aspectQuality = aspects.FirstOrDefault(x => x.Item1 == aspect).Item2 - amount;
			if (aspectQuality < 0) return -1;
			prevAspect = Math.Min(prevAspect, aspectQuality);
			quality += prevAspect;
		}

		return quality;
	}

	public void AcceptRequest() {
		if (VisitorAtDoor != null) {
			state.current_requests.Add(VisitorAtDoor);
			state.visitor_at_door = null;
		}
	}

	public void RejectRequest() {
		state.visitor_at_door = null;
	}

	public List<RegionModel> GetUnlocksWith(UnlockRequirement requirement, int from) {
		return region_unlocks[requirement.Type].Where(
			region => region.UnlockRequirement.Amount <= requirement.Amount && region.UnlockRequirement.Amount >= from
		).ToList();
	}

	private void OnJournalConfirmation(IEnumerable<ItemModel> items) {
		var unlocked = GetUnlocksWith(UnlockRequirement.ConfirmedJournalEntries(Journal.TotalConfirmed), Journal.TotalConfirmed - 2);
		foreach (var region in unlocked) {
			region_lookup[region.Id].Unlocked = true;
			EmitSignalRegionUnlocked(region.Id);
		}
		
		EmitSignalJournalConfirmation([..items.Select(i => i.Id)]);
	}

	private static World CreateWorld() {
		Aspect? bloom, caust = null, spice = null, vigor = null, umber = null, gelus = null;
		bloom = new Aspect("bloom", Aspect.SpringGreen, () => caust!);
		caust = new Aspect("caust", Aspect.Chartreuse, () => spice!);
		spice = new Aspect("spice", Aspect.Orange, () => vigor!);
		vigor = new Aspect("vigor", Aspect.Rose, () => umber!);
		umber = new Aspect("umber", Aspect.Violet, () => gelus!);
		gelus = new Aspect("gelus", Aspect.Azure, () => bloom!);

		var frontYard = new RegionModel("front_yard", 2, 0.25, UnlockRequirement.None);
		var backyard = new RegionModel("backyard", 2, 0.25, UnlockRequirement.ResourceAcquired(Resource.Reputation, 1));
		var road = new RegionModel("road", 3, 0.5, UnlockRequirement.ResourceAcquired(Resource.Reputation, 1));
		var meadow = new RegionModel("meadow", 5, 0.5, UnlockRequirement.ResourceAcquired(Resource.Reputation, 1));
		var eastWoods = new RegionModel("east_woods", 5, 0.5, UnlockRequirement.ConfirmedJournalEntries(3));
		var westWoods = new RegionModel("west_woods", 5, 0.5, UnlockRequirement.ConfirmedJournalEntries(6));
		var creek = new RegionModel("creek", 4, 0.5, UnlockRequirement.ConfirmedJournalEntries(9));

		var adjacencies = new Dictionary<RegionModel, HashSet<RegionModel>> {
			{ frontYard, [road, meadow, eastWoods, backyard] },
			{ backyard, [frontYard, eastWoods, creek, westWoods] },
			{ road, [frontYard, meadow, westWoods] },
			{ meadow, [frontYard, road, eastWoods] },
			{ eastWoods, [frontYard, meadow, creek, backyard] },
			{ westWoods, [backyard, creek, road] },
			{ creek, [backyard, eastWoods, westWoods] }
		};

		ImmutableArray<ItemModel> items = [
			new("meadowsweet", [(bloom, 1), (vigor, 1)], creek),
			new("wild_laceroot", [(caust, 1), (spice, 1), (umber, 1)], eastWoods, ItemFindCondition.Afternoon),
			new("mintflower", [(bloom, 1), (gelus, 1), (spice, 1)], eastWoods),
			new("feverfew", [(bloom, 2), (gelus, 1)], backyard, rarity: Rarity.Rare),
			new("white_coneflower", [(bloom, 1), (gelus, 1)], meadow, ItemFindCondition.Morning),
			new("chamomile", [(umber, 2), (gelus, 1), (bloom, 1)], meadow, rarity: Rarity.Rare)
		];

		// https://www.st-george-squadron.com/sgs/wiki/index.php/18th_century_names
		var villager = new VisitorType(
			"villager",
			["Abraham", "Adam", "Adrian", "Alexander", "Allen", "Ambrose", "Andrew", "Anthony", "Arthur", "Avery", "Barnaby", "Bartholomew", "Benedict", "Bernard", "Brian", "Bryan", "Caleb", "Charles", "Christopher", "Cuthbert", "Daniel", "David", "Edmund", "Edward", "Emmerson", "Frances", "Francis", "Fulke", "Geoffrey", "George", "Gerard", "Gilbert", "Giles", "Gregory", "Henry", "Hugh", "Humphrey", "Isaac", "James", "Jerome", "Johan", "John", "Jonathan", "Joseph", "Judd", "Julian", "Lancelot", "Lawrance", "Lawrence", "Leonard", "Luke", "Mark", "Martin", "Mathias", "Matthew", "Metcalfe", "Michael", "Miles", "Nathaniel", "Nicholas", "Oliver", "Oswyn", "Peter", "Philip", "Phillip", "Piers", "Raiph", "Ralph", "Reynold", "Richard", "Robert", "Roger", "Rowland", "Samuel", "Silas", "Simon", "Solomon", "Stephen", "Tamer", "Thomas", "Tobias", "Toby", "Valentine", "Walter", "William", "Addeline", "Agnes", "Alice", "Amelia", "Amy", "Ann", "Anne", "Audrey", "Augusta", "Avis", "Barbara", "Beatrice", "Blanche", "Bridget", "Carolina", "Caroline", "Catherine", "Cecily", "Charity", "Charlotte", "Christian", "Christina", "Clemence", "Constance", "Deborah", "Denise", "Dorothea", "Dorothy", "Edith", "Eleanor", "Elinor", "Eliza", "Elizabeth", "Ellen", "Ellener", "Ellin", "Elliner", "Emma", "Florence", "Fortune", "Frances", "Frideswide", "Gillian", "Grace", "Hannah", "Helen", "Isabel", "Isabell", "Jan", "Jane", "Janet", "Jennet", "Joan", "Josian", "Joyce", "Judith", "Julian", "Katherine", "Lettice", "Louisa", "Lucy", "Mabel", "Margaret", "Margery", "Maria", "Marie", "Marion", "Martha", "Mary", "Matilda", "Maud", "Mildred", "Millicent", "Parnell", "Phebe", "Philippa", "Rachel", "Rebecca", "Rose", "Ruth", "Sarah", "Sophia", "Susanna", "Sybil", "Thomasin", "Maud", "Mildred", "Millicent", "Parnell", "Phebe", "Philippa", "Rachel", "Rebecca", "Rose", "Ruth", "Sarah", "Sarah", "Sophia", "Susanna", "Susanna", "Sybil", "Thomasin", "Ursula", "Wilmot", "Winifred"],
			["Abell", "Ackworth", "Adams", "Addicock", "Alban", "Aldebourne", "Alfray", "Alicock", "Allard", "Allen", "Allington", "Amberden", "Amcotts", "Amondsham", "Andrews", "Annesley", "Ansty", "Archer", "Ardall", "Ardern", "Argentein", "Arnold", "Arthur", "Asger", "Ashby", "Ashcombe", "Ashenhurst", "Ashton", "Askew", "Asplin", "Astley", "Atherton", "Atkinson", "Atlee", "Attilburgh", "Aubrey", "Audeley", "Audlington", "Ayde", "Ayleward", "Aylmer", "Aynesworth", "Babham", "Babington", "Badby", "Bailey", "Baker", "Balam", "Baldwin", "Ballard", "Ballett", "Bammard", "Barber", "Bardolf", "Barefoot", "Barker", "Barnes", "Barre", "Barrentine", "Barrett", "Barstaple", "Bartelot", "Barton", "Basset", "Bathurst"],
			[Resource.Coins, Resource.Reputation],
			[Resource.Coins, Resource.Reputation]
		);

		var painText = """
			[I'd like something to help with my {0}.|I have {0}.|Do you have something for {0}? % arthritis|leg pain|cramping]
			[I stubbed my toe.][I think I've sprained something.]
		""";
		var painRequest = new RequestModel("pain", villager, painText, [(bloom, 1), (umber, 0)], 1);

		var migraineText = """
			[Migraine.][My headache hasn't gone away.][I've been having {0} migraines. % terrible|awful]
		""";
		var migraineRequest = new RequestModel("migraine", villager, migraineText, [(bloom, 1), (spice, 0)], 1);

		var foodPoisoningText = """
			[I shouldn't have eaten {0}. % that raw shellfish|those leftovers|that salami off the floor]
		""";
		var foodPoisoningRequest = new RequestModel("food_poisoning", villager, foodPoisoningText, [(spice, 1), (gelus, 0)], 1);

		return new World(
			[frontYard, backyard, road, meadow, eastWoods, westWoods, creek],
			adjacencies.AsReadOnly(),
			[bloom, caust, spice, vigor, umber, gelus],
			items,
			[painRequest, migraineRequest, foodPoisoningRequest]
		);
	}

	private void Serialize() {
		//MessagePackSerializer.Serialize(this, new MessagePackSerializerOptions())
	}
}
