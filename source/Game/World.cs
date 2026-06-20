using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;

namespace Apothecary;

public class World {
	public ImmutableArray<RegionModel> Regions { get; }
	private ReadOnlyDictionary<string, RegionModel> LocationIdMap { get; }
	public ReadOnlyDictionary<RegionModel, HashSet<RegionModel>> Adjacencies { get; }

	public ImmutableArray<Aspect> Aspects { get; }
	private ReadOnlyDictionary<string, Aspect> AspectIdMap { get; }

	public ImmutableArray<ItemModel> Items { get; }
	private ReadOnlyDictionary<string, ItemModel> ItemIdMap { get; }

	public ImmutableArray<RequestModel> Requests { get; }
	private ReadOnlyDictionary<string, RequestModel> RequestIdMap { get; }

	public World(
		ImmutableArray<RegionModel> regions,
		ReadOnlyDictionary<RegionModel, HashSet<RegionModel>> adjacencies,
		ImmutableArray<Aspect> aspects,
		ImmutableArray<ItemModel> items,
		ImmutableArray<RequestModel> requests
	) {
		Regions = regions;
		LocationIdMap = regions.ToDictionary(x => x.Id).AsReadOnly();
		Adjacencies = adjacencies;

		Aspects = aspects;
		AspectIdMap = aspects.ToDictionary(x => x.Id).AsReadOnly();

		Items = items;
		foreach (var (item, i) in Items.Select((item, i) => (item, i))) {
			item.Index = i;
		}
		ItemIdMap = items.ToDictionary(x => x.Id).AsReadOnly();

		Requests = requests;
		RequestIdMap = requests.ToDictionary(x => x.Id).AsReadOnly();
	}

	public RegionModel? GetRegionModel(string id) {
		return LocationIdMap.GetValueOrDefault(id);
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
}
