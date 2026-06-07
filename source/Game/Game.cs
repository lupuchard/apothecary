using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using Godot;

namespace Apothecary;

public partial class Game : RefCounted {
	public static Game Instance { get; private set; } = new();
	
	[Signal] public delegate void TimeChangedEventHandler();
	
	private const int MAX_FORAGE_RESULTS = 3;
	private const int END_OF_DAY = 6;
	private const int DAYS_IN_SEASON = 36;

	public readonly World World = CreateWorld();
	private Rando rando = new (seed: 2);

	private int day = 0;
	private Season season = Season.Estival;
	public int TimeOfDay { get; private set; } = 0;
	private int year = 1;

	private readonly Dictionary<(long, RegionModel), List<ItemModel>> foraging_possibilities_cache = new();
	private List<ItemModel?> current_foraging_results = [];

	private readonly List<Region> locations = [];
	private readonly Dictionary<string, Region> location_lookup = new();
	private readonly List<Item> inventory = [];

	public Game() {
		foreach (var region in World.Locations) {
			locations.Add(new Region(region));
			location_lookup.Add(region.Id, locations.Last());
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
		if (index < current_foraging_results.Count && current_foraging_results[index] is { } item) {
			inventory.Add(new Item(item));
			current_foraging_results[index] = null;
		}
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

	private static World CreateWorld() {
		Aspect? bloom = null, caust = null, spice = null, vigor = null, umber = null, gelus = null;
		bloom = new Aspect("bloom", () => caust!);
		caust = new Aspect("caust", () => spice!);
		spice = new Aspect("spice", () => vigor!);
		vigor = new Aspect("vigor", () => umber!);
		umber = new Aspect("umber", () => gelus!);
		gelus = new Aspect("gelus", () => bloom!);

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
			new ItemModel("meadowsweet", [(bloom, 1), (vigor, 1), (caust, 1)], creek),
			new ItemModel("wild_laceroot", [(caust, 1), (spice, 1), (umber, 1)], eastWoods, ItemFindCondition.Morning),
			new ItemModel("mintflower", [(gelus, 1), (bloom, 1)], westWoods),
			new ItemModel("feverfew", [(bloom, 2), (umber, 1)], backyard, rarity: Rarity.Rare),
			new ItemModel("white_coneflower", [(bloom, 1), (gelus, 1)], meadow, ItemFindCondition.Afternoon),
			new ItemModel("chamomile", [(umber, 2), (gelus, 1), (bloom, 1)], meadow, rarity: Rarity.Rare)
		];

		return new World(
			[frontYard, road, meadow, eastWoods, westWoods, creek],
			adjacencies.AsReadOnly(),
			[bloom, caust, spice, vigor, umber, gelus],
			items
		);
	}
}
