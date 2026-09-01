using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Godot;
using Tomlyn;

namespace Apothecary;

public class World {
	public ImmutableArray<RegionModel> Regions { get; }
	private ReadOnlyDictionary<string, RegionModel> RegionIdMap { get; }
	public ReadOnlyDictionary<RegionModel, HashSet<RegionModel>> Adjacencies { get; }

	public ImmutableArray<Aspect> Aspects { get; }
	private ReadOnlyDictionary<string, Aspect> AspectIdMap { get; }

	public ImmutableArray<ItemModel> Items { get; }
	private ReadOnlyDictionary<string, ItemModel> ItemIdMap { get; }
	
	public ImmutableArray<VisitorType> Visitors { get; }
	private ReadOnlyDictionary<string, VisitorType> VisitorIdMap { get; }

	public ImmutableArray<RequestModel> Requests { get; }
	private ReadOnlyDictionary<string, RequestModel> RequestIdMap { get; }

	public World(
		ImmutableArray<RegionModel> regions,
		ReadOnlyDictionary<RegionModel, HashSet<RegionModel>> adjacencies,
		ImmutableArray<Aspect> aspects,
		ImmutableArray<ItemModel> items,
		ImmutableArray<VisitorType> visitors,
		ImmutableArray<RequestModel> requests
	) {
		Regions = regions;
		RegionIdMap = regions.ToDictionary(x => x.Id).AsReadOnly();
		Adjacencies = adjacencies;

		Aspects = aspects;
		AspectIdMap = aspects.ToDictionary(x => x.Id).AsReadOnly();

		Items = items;
		foreach (var (item, i) in Items.Select((item, i) => (item, i))) {
			item.Index = i;
		}
		ItemIdMap = items.ToDictionary(x => x.Id).AsReadOnly();

		Visitors = visitors;
		VisitorIdMap = visitors.ToDictionary(x => x.Id).AsReadOnly();

		Requests = requests;
		RequestIdMap = requests.ToDictionary(x => x.Id).AsReadOnly();
	}

	public RegionModel? GetRegionModel(string id) {
		return RegionIdMap.GetValueOrDefault(id);
	}

	public ItemModel? GetItemModel(string id) {
		return ItemIdMap.GetValueOrDefault(id);
	}

	public Aspect? GetAspect(string id) {
		return AspectIdMap.GetValueOrDefault(id);
	}

	public RequestModel? GetRequest(string id) {
		return RequestIdMap.GetValueOrDefault(id);
	}


	private record AspectData(float[] color, string mutates_into);

	private record RegionData(
		int max_forage, 
		double forage_recovery, 
		bool woodcutting = false, 
		string? unlock_requirement = null, 
		int? unlock_requirement_amount = null, 
		string[]? adjacent = null
	);

	private record ItemData(string[] aspects, string where, string? when = null, string? rarity = null);

	private record VisitorData(string[] first_names, string[] last_names, string[] reward, string[] tip);

	private record RequestData(string visitor, string text, string[] aspects, int reward);

	private record WorldData(
		Dictionary<string, AspectData> aspects, 
		Dictionary<string, RegionData> regions, 
		Dictionary<string, ItemData> items, 
		Dictionary<string, VisitorData> visitors, 
		Dictionary<string, RequestData> requests
	);

	public World(string data) {
		var options = new TomlSerializerOptions { PropertyNameCaseInsensitive = true };
		var world_data = TomlSerializer.Deserialize<WorldData>(data, options);
		if (world_data == null) throw new Exception("Failed to load world data");

		Dictionary<string, Aspect> aspect_lookup = new();
		List<Aspect> aspects = [];
		foreach (var aspect_data in world_data.aspects) {
			var color = ArrayToColor(aspect_data.Value.color);
			var aspect = new Aspect(aspect_data.Key, color, () => aspect_lookup[aspect_data.Value.mutates_into]);
			aspects.Add(aspect);
			aspect_lookup.Add(aspect_data.Key, aspect);
		}
		Aspects = [..aspects];
		AspectIdMap = aspect_lookup.AsReadOnly();
		
		Regions = [..world_data.regions.Select(region => new RegionModel(
			region.Key,
			region.Value.max_forage,
			region.Value.forage_recovery,
			ParseUnlockRequirement(region.Value.unlock_requirement, region.Value.unlock_requirement_amount)
		))];
		RegionIdMap = Regions.ToDictionary(x => x.Id).AsReadOnly();
		
		Dictionary<RegionModel, HashSet<RegionModel>> region_adjacencies = new();
		foreach (var (id, region) in world_data.regions) {
			HashSet<RegionModel> adjacent = [];
			foreach (var region_id in region.adjacent ?? []) {
				var adjacent_region = RegionIdMap.GetValueOrDefault(region_id);
				if (adjacent_region == null) {
					LogParseError($"Unknown adjacent region '{region_id}'");
				} else {
					adjacent.Add(adjacent_region);
				}
			}

			region_adjacencies.Add(RegionIdMap[id], adjacent);
		}
		Adjacencies = region_adjacencies.AsReadOnly();

		List<ItemModel> items = [];
		foreach (var (id, item) in world_data.items) {
			var region = RegionIdMap.GetValueOrDefault(item.where);
			if (region == null) {
				LogParseError($"Unknown region '{item.where}'");
				continue;
			}
			var item_aspects = item.aspects.Select(aspect => ParseAspect(aspect, AspectIdMap)).ToImmutableArray();
			var condition = ItemFindConditions.FromString(item.when ?? "");
			if (item.when != null && condition == null) {
				LogParseError($"Unknown item find condition '{item.when}'");
			}
			var rarity = Rarities.FromString(item.rarity ?? "");
			if (item.rarity != null && rarity == null) {
				LogParseError($"Unknown rarity '{item.rarity}'");
			}
			items.Add(new ItemModel(id, item_aspects, region, condition ?? ItemFindCondition.None, rarity ?? Rarity.Common));
		}
		Items = [..items];
		ItemIdMap = Items.ToDictionary(x => x.Id).AsReadOnly();
		
		Visitors = [..world_data.visitors.Select(visitor => new VisitorType(
			visitor.Key, 
			visitor.Value.first_names,
			visitor.Value.last_names,
			[..visitor.Value.reward.Select(ParseResource)],
			[..visitor.Value.tip.Select(ParseResource)]
		))];
		VisitorIdMap = Visitors.ToDictionary(x => x.Id).AsReadOnly();

		List<RequestModel> requests = [];
		foreach (var (id, request) in world_data.requests) {
			if (!VisitorIdMap.TryGetValue(request.visitor, out var visitor)) {
				LogParseError($"Unknown visitor '{id}'");
				continue;
			}
			var request_aspects = request.aspects.Select(aspect => ParseAspect(aspect, AspectIdMap)).ToImmutableArray();
			requests.Add(new RequestModel(id, visitor, request.text, request_aspects, request.reward));
		}
		Requests = [..requests];
		RequestIdMap = Requests.ToDictionary(x => x.Id).AsReadOnly();
	}

	private static void LogParseError(string error) {
		GD.PrintErr("World parse error: " + error);
	}

	private static Color ArrayToColor(float[] elems) {
		if (elems.Length != 3) {
			LogParseError("Color array must have 3 elements");
			return Colors.White;
		}
		return new Color(elems[0], elems[1], elems[2]);
	}

	private static UnlockRequirement ParseUnlockRequirement(string? type, int? amount) {
		if (type == null) {
			return UnlockRequirement.None;
		} else if (type.Equals("day", StringComparison.OrdinalIgnoreCase)) {
			return UnlockRequirement.Day(amount ?? 1);
		} else if (type.Equals("journal", StringComparison.OrdinalIgnoreCase)) {
			return UnlockRequirement.ConfirmedJournalEntries(amount ?? 1);
		} else {
			var resource = Resources.FromString(type);
			if (resource == null) {
				LogParseError($"Unknown unlock type '{type}'");
				return UnlockRequirement.None;
			}
			return UnlockRequirement.ResourceAcquired(resource.Value, amount ?? 1);
		}
	}

	private static (Aspect, int) ParseAspect(string aspect_str, ReadOnlyDictionary<string, Aspect> aspect_lookup) {
		var parts = aspect_str.Split(" ", 2, StringSplitOptions.RemoveEmptyEntries).ToList();
		if (parts.Count == 0) {
			LogParseError($"Empty aspect string");
			return (Aspect.UnknownAspect, 1);
		} else if (parts.Count > 2) {
			LogParseError($"Invalid aspect string: '{aspect_str}'");
			return (Aspect.UnknownAspect, 1);
		}

		if (!aspect_lookup.TryGetValue(parts[^1], out var aspect)) {
			LogParseError($"Unknown aspect: '{parts[^1]}'");
			return (Aspect.UnknownAspect, 1);
		}

		var amount = 1;
		if (parts.Count == 2 && !int.TryParse(parts[0], out amount)) {
			LogParseError($"Invalid amount '{parts[0]}'");
			return (Aspect.UnknownAspect, 1);
		}

		return (aspect, amount);
	}

	private static Resource ParseResource(string resource_str) {
		var resource = Resources.FromString(resource_str);
		if (resource == null) {
			LogParseError($"Unknown resource '{resource_str}'");
		}
		return resource ?? Resource.None;
	}
}
