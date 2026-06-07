
public enum Season {
	case Estival, Serotinal, Autumnal, Hibernal, Prevernal, Vernal
}

public class Model : Hashable {
	public static func ==(lhs: Model, rhs: Model) -> Bool {
		return lhs === rhs
	}

	public func hash(into hasher: inout Hasher) {
		hasher.combine(ObjectIdentifier(self))
	}
}

public class Aspect : Model {
	let id: String
	weak var mutates_into: Aspect?

	public init(_ id: String) {
		self.id = id
	}
}

public class RegionModel : Model {
	let name: String

	public init(_ name: String) {
		self.name = name
	}
}

public enum ItemFindCondition: Int, CaseIterable {
	case Morning, Afternoon, Night
	case AfterRaining
	case InMoonlight
}

public enum Rarity {
	case Common, Rare, Scarce
}

public class ItemModel : Model {
	let name: String
	let aspects: [(Aspect, Int)]
	
	let where_found: RegionModel?
	let when_found: ItemFindCondition?
	let rarity: Rarity

	public init(
		_ name: String,
		aspects: [(Aspect, Int)],
		where_found: RegionModel? = nil,
		when_found: ItemFindCondition? = nil,
		rarity: Rarity = .Common
	) {
		self.name = name
		self.aspects = aspects
		self.where_found = where_found
		self.when_found = when_found
		self.rarity = rarity
	}
}

public class World {
	let locations: [RegionModel]
	let adjacencies: [RegionModel: Set<RegionModel>]
	let aspects: [Aspect]
	let items: [ItemModel]

	public init(
		locations: [RegionModel],
		adjacencies: [RegionModel: Set<RegionModel>],
		aspects: [Aspect],
		items: [ItemModel]
	) {
		self.locations = locations
		self.adjacencies = adjacencies
		self.aspects = aspects
		self.items = items
	}
}
