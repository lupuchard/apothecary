using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;

namespace Apothecary;

public class World {
	public ImmutableArray<RegionModel> Locations { get; }
	private ReadOnlyDictionary<string, RegionModel> LocationIdMap { get; }
	public ReadOnlyDictionary<RegionModel, HashSet<RegionModel>> Adjacencies { get; }
	
	public ImmutableArray<Aspect> Aspects { get; }
	private ReadOnlyDictionary<string, Aspect> AspectIdMap { get; }
	
	public ImmutableArray<ItemModel> Items { get; }
	private ReadOnlyDictionary<string, ItemModel> ItemIdMap { get; }

	public World(
		ImmutableArray<RegionModel> locations,
		ReadOnlyDictionary<RegionModel, HashSet<RegionModel>> adjacencies,
		ImmutableArray<Aspect> aspects,
		ImmutableArray<ItemModel> items
	) {
		Locations = locations;
		LocationIdMap = locations.ToDictionary(x => x.Id).AsReadOnly();
		Adjacencies = adjacencies;
		
		Aspects = aspects;
		AspectIdMap = aspects.ToDictionary(x => x.Id).AsReadOnly();
		
		Items = items;
		ItemIdMap = items.ToDictionary(x => x.Id).AsReadOnly();
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
}
