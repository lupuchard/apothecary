using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;

namespace Apothecary;

public partial class Game : RefCounted {
	public static Game Instance { get; private set; } = new();
	
	[Signal] public delegate void TimeChangedEventHandler();
	
	private const int MAX_FORAGE_RESULTS = 3;
	private const int END_OF_DAY = 6;
	private const int DAYS_IN_SEASON = 36;
	private const float VISITOR_CHANCE = 0.4f;

	public readonly World World = CreateWorld();
	private Rando rando = new (seed: 2);

	private int day = 0;
	private Season season = Season.Estival;
	public int TimeOfDay { get; private set; } = 0;
	private int year = 1;

	private readonly Dictionary<(long, RegionModel), List<ItemModel>> foraging_possibilities_cache = [];
	private List<ItemModel?> current_foraging_results = [];

	private readonly List<Region> locations = [];
	private readonly Dictionary<string, Region> location_lookup = [];

	private readonly Dictionary<Item, (Item, int)> inventory = [];
	private List<(Item, int)>? sorted_inventory = [];
	private Aspect? inventory_aspect_filter = null;
	private ItemType? inventory_type_filter = null;
	private InventorySortMethod inventory_sort_method = InventorySortMethod.Index;
	private bool inventory_sort_descending = false;

	public Visitor? VisitorAtDoor { get; private set; } = null;
	private List<Visitor> current_requests = [];
	public IReadOnlyList<Visitor> CurrentRequests => current_requests;

	private readonly List<int> resources = [];

	public Game() {
		foreach (var region in World.Locations) {
			locations.Add(new Region(region));
			location_lookup.Add(region.Id, locations.Last());
		}

		for (var i = 0; i < (int)Resource.COUNT; i++) {
			resources.Add(0);
		}
	}
	
	public void PassTime() {
		if (TimeOfDay < END_OF_DAY) {
			TimeOfDay += 1;
			EmitSignalTimeChanged();
		}
	}

	public void NextDay() {
		day += 1;
		TimeOfDay = 0;
		if (day >= DAYS_IN_SEASON) {
			season += 1;
			day = 0;
			if (season > Season.Hibernal) {
				season = Season.Prevernal;
				year += 1;
			}
		}

		foreach (var location in locations) {
			location.DailyRecovery(ref rando);
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

		var region = GetLocation(location.Id);
		if (region?.Remaining > 0) {
			region.ConsumeForage();
			current_foraging_results = [..GetForagingResults(location).Cast<ItemModel?>()];
		}

		PassTime();
	}

	public void AcquireForagingResult(int index) {
		if (index < current_foraging_results.Count && current_foraging_results[index] is { } item_model) {
			var item = new Item(item_model);
			ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(inventory, item, out var exists);
			if (!exists) value.Item1 = item;
			value.Item2 += 1;

			current_foraging_results[index] = null;
			sorted_inventory = null;
		}
	}

	public List<(Item, int)> GetInventory() {
		if (sorted_inventory == null) {
			sorted_inventory = [.. inventory.Values];
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

	public Region? GetLocation(string region_id) {
		return location_lookup.GetValueOrDefault(region_id);
	}

	private List<ItemModel> GetForagingResults(RegionModel location) {
		var forage_key = (GetCurrentConditions(), location);
		var possibilities = GetForagingPossibilities(forage_key);
		possibilities = rando.Shuffle([.. possibilities.Concat(possibilities)]);

		var results = new List<ItemModel>();
		foreach (var item in possibilities) {
			if (rando.RandDouble() < GetItemForageProbability(item)) {
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
			if (item.WhereFound == null) continue;

			if (item.WhenFound != null && (forage_key.conditions & (long)item.WhenFound) == 0) {
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
		return season switch {
			Season.Estival => true,
			Season.Vernal or Season.Serotinal => TimeOfDay < 5,
			Season.Prevernal or Season.Autumnal => TimeOfDay < 4,
			Season.Hibernal => TimeOfDay < 3,
			_ => throw new InvalidEnumArgumentException("season", (int)season, typeof(Season))
		};
	}

	public bool IsItMorning() {
		return season switch {
			Season.Estival => TimeOfDay < 3,
			Season.Vernal or Season.Serotinal => TimeOfDay < 2,
			Season.Prevernal or Season.Autumnal => TimeOfDay < 2,
			Season.Hibernal => TimeOfDay < 1,
			_ => throw new InvalidEnumArgumentException("season", (int)season, typeof(Season)),
		};
	}

	public bool IsItAfternoon() {
		return season switch {
			Season.Estival or Season.Vernal or Season.Serotinal => TimeOfDay >= 3,
			Season.Prevernal or Season.Autumnal or Season.Hibernal => TimeOfDay >= 2,
			_ => throw new InvalidEnumArgumentException("season", (int)season, typeof(Season))
		};
	}

	private void UpdateVisitors() {
		foreach (var visitor in visitors) {
			visitor.RemainingDays -= 1;
		}
		visitors = [.. visitors.Where(v => v.RemainingDays <= 0)];
		VisitorAtDoor = null;

		if (rando.RandDouble() < VISITOR_CHANCE) {
			var model = rando.Pick(World.Requests);
			VisitorAtDoor = new Visitor(model, ref rando);
		}
	}

	public (string?, Reward?) GiveVisitor(Visitor visitor, Item cure) {
		if (!inventory.ContainsKey(cure)) return (Tr("VISITOR_NO_ITEM"), null);
		if ((cure.Type & ItemType.Infusion) == 0) return (Tr("VISITOR_NOT_INFUSION"), null);

		var prevAspect = int.MaxValue;
		var quality = 0;
		foreach (var (aspect, amount) in visitor.Request.Aspects) {
			var aspectQuality = cure.Aspects.FirstOrDefault(x => x.Item1 == aspect).Item2 - amount;
			if (aspectQuality < 0) return (Tr("VISITOR_WRONG_ASPECT"), null);
			prevAspect = Math.Min(prevAspect, aspectQuality);
			quality += prevAspect;
		}

		resources[(int)Resource.Coins] += visitor.Request.Reward + quality;
		return (null, new Reward([(Resource.Coins, visitor.Request.Reward + quality)]));
	}

	public void RejectVisitor() {
		VisitorAtDoor = null;
	}

	private static World CreateWorld() {
		Aspect? bloom, caust = null, spice = null, vigor = null, umber = null, gelus = null;
		bloom = new Aspect("bloom", Aspect.SpringGreen, () => caust!);
		caust = new Aspect("caust", Aspect.Chartreuse, () => spice!);
		spice = new Aspect("spice", Aspect.Orange, () => vigor!);
		vigor = new Aspect("vigor", Aspect.Rose, () => umber!);
		umber = new Aspect("umber", Aspect.Violet, () => gelus!);
		gelus = new Aspect("gelus", Aspect.Azure, () => bloom!);

		var frontYard = new RegionModel("front_yard", 2, 0.25);
		var backyard = new RegionModel("backyard", 2, 0.25);
		var road = new RegionModel("road", 3, 0.5);
		var meadow = new RegionModel("meadow", 5, 0.5);
		var eastWoods = new RegionModel("east_woods", 5, 0.5);
		var westWoods = new RegionModel("west_woods", 5, 0.5);
		var creek = new RegionModel("creek", 4, 0.5);

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
			new ItemModel("meadowsweet", [(bloom, 1), (vigor, 1)], creek),
			new ItemModel("wild_laceroot", [(caust, 1), (spice, 1), (umber, 1)], eastWoods, ItemFindCondition.Morning),
			new ItemModel("mintflower", [(gelus, 1), (spice, 1), (bloom, 1)], westWoods),
			new ItemModel("feverfew", [(bloom, 2), (gelus, 1)], backyard, rarity: Rarity.Rare),
			new ItemModel("white_coneflower", [(bloom, 1), (gelus, 1)], meadow, ItemFindCondition.Afternoon),
			new ItemModel("chamomile", [(umber, 2), (gelus, 1), (bloom, 1)], meadow, rarity: Rarity.Rare)
		];

		// https://www.st-george-squadron.com/sgs/wiki/index.php/18th_century_names
		var villager = new VisitorType(
			"villager",
			["Abraham", "Adam", "Adrian", "Alexander", "Allen", "Ambrose", "Andrew", "Anthony", "Arthur", "Avery", "Barnaby", "Bartholomew", "Benedict", "Bernard", "Brian", "Bryan", "Caleb", "Charles", "Christopher", "Cuthbert", "Daniel", "David", "Edmund", "Edward", "Emmerson", "Frances", "Francis", "Fulke", "Geoffrey", "George", "Gerard", "Gilbert", "Giles", "Gregory", "Henry", "Hugh", "Humphrey", "Isaac", "James", "Jerome", "Johan", "John", "Jonathan", "Joseph", "Judd", "Julian", "Lancelot", "Lawrance", "Lawrence", "Leonard", "Luke", "Mark", "Martin", "Mathias", "Matthew", "Metcalfe", "Michael", "Miles", "Nathaniel", "Nicholas", "Oliver", "Oswyn", "Peter", "Philip", "Phillip", "Piers", "Raiph", "Ralph", "Reynold", "Richard", "Robert", "Roger", "Rowland", "Samuel", "Silas", "Simon", "Solomon", "Stephen", "Tamer", "Thomas", "Tobias", "Toby", "Valentine", "Walter", "William", "Addeline", "Agnes", "Alice", "Amelia", "Amy", "Ann", "Anne", "Audrey", "Augusta", "Avis", "Barbara", "Beatrice", "Blanche", "Bridget", "Carolina", "Caroline", "Catherine", "Cecily", "Charity", "Charlotte", "Christian", "Christina", "Clemence", "Constance", "Deborah", "Denise", "Dorothea", "Dorothy", "Edith", "Eleanor", "Elinor", "Eliza", "Elizabeth", "Ellen", "Ellener", "Ellin", "Elliner", "Emma", "Florence", "Fortune", "Frances", "Frideswide", "Gillian", "Grace", "Hannah", "Helen", "Isabel", "Isabell", "Jan", "Jane", "Janet", "Jennet", "Joan", "Josian", "Joyce", "Judith", "Julian", "Katherine", "Lettice", "Louisa", "Lucy", "Mabel", "Margaret", "Margery", "Maria", "Marie", "Marion", "Martha", "Mary", "Matilda", "Maud", "Mildred", "Millicent", "Parnell", "Phebe", "Philippa", "Rachel", "Rebecca", "Rose", "Ruth", "Sarah", "Sophia", "Susanna", "Sybil", "Thomasin", "Maud", "Mildred", "Millicent", "Parnell", "Phebe", "Philippa", "Rachel", "Rebecca", "Rose", "Ruth", "Sarah", "Sarah", "Sophia", "Susanna", "Susanna", "Sybil", "Thomasin", "Ursula", "Wilmot", "Winifred"],
			["Abell", "Ackworth", "Adams", "Addicock", "Alban", "Aldebourne", "Alfray", "Alicock", "Allard", "Allen", "Allington", "Amberden", "Amcotts", "Amondsham", "Andrews", "Annesley", "Ansty", "Archer", "Ardall", "Ardern", "Argentein", "Arnold", "Arthur", "Asger", "Ashby", "Ashcombe", "Ashenhurst", "Ashton", "Askew", "Asplin", "Astley", "Atherton", "Atkinson", "Atlee", "Attilburgh", "Aubrey", "Audeley", "Audlington", "Ayde", "Ayleward", "Aylmer", "Aynesworth", "Babham", "Babington", "Badby", "Bailey", "Baker", "Balam", "Baldwin", "Ballard", "Ballett", "Bammard", "Barber", "Bardolf", "Barefoot", "Barker", "Barnes", "Barre", "Barrentine", "Barrett", "Barstaple", "Bartelot", "Barton", "Basset", "Bathurst"]
		);

		var painText = """
			[I'd like something to help with my {0}.|I have {0}.|Do you have something for {0}? % arthritis|leg pain|cramping]
			[I stubbed my toe.][I think I've sprained something.]
		""";
		var painRequest = new RequestModel("pain", villager, painText, [(bloom, 1), (umber, 0)], 2);

		var migraineText = """
			[Migraine.][My headache hasn't gone away.][I've been having {0} migraines. % terrible|awful]
		""";
		var migraineRequest = new RequestModel("migraine", villager, migraineText, [(bloom, 1), (spice, 0)], 2);

		var foodPoisoningText = """
			[I shouldn't have eaten {0}. % that raw shellfish|those leftovers|than salami off the floor]
		""";
		var foodPoisoningRequest = new RequestModel("food_poisoning", villager, foodPoisoningText, [(spice, 1), (gelus, 0)], 2);

		return new World(
			[frontYard, backyard, road, meadow, eastWoods, westWoods, creek],
			adjacencies.AsReadOnly(),
			[bloom, caust, spice, vigor, umber, gelus],
			items,
			[painRequest, migraineRequest, foodPoisoningRequest]
		);
	}
}
