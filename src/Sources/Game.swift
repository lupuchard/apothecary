
public enum ItemType: Int, CaseIterable {
	case Raw, Ground, Roasted, Infusion
}

public struct Item : Hashable {
	let raw: [ItemModel]
	let aspects: [(Aspect, Int)]
	let type: Set<ItemType>

	public static func ==(lhs: Item, rhs: Item) -> Bool {
		return lhs.type == rhs.type && lhs.raw == rhs.raw && lhs.aspects.elementsEqual(rhs.aspects){ $0.0 == $1.0 && $0.1 == $1.1 }
	}

	public func hash(into hasher: inout Hasher) {
		hasher.combine(type)
		hasher.combine(raw)

		hasher.combine(aspects.count)
		for (aspect, amount) in aspects {
			hasher.combine(aspect)
			hasher.combine(amount)
		}
	}
}

let MAX_FORAGE_RESULTS = 3

public class Game {
	let world: World

	var rando: Rando

	var day: Int = 0
	var season: Season = .Estival
	var time_of_day: Int = 0

	private struct ForageKey : Hashable {
		let conditions: Set<ItemFindCondition>
		let location: RegionModel
	}

	private var foraging_possibilities_cache: [ForageKey : [ItemModel]] = [:]

	public init() {
		rando = Rando(seed: 2)
		world = Game.create_world()
	}

	public func get_forage_results(_ location: RegionModel) -> [ItemModel] {
		let forage_key = ForageKey(
			conditions: get_current_conditions(),
			location: location
		)

		var possibilities = get_foraging_possibilities(forage_key)
		possibilities.append(contentsOf: possibilities)
		possibilities.shuffle()

		var results: [ItemModel] = []
		for item in possibilities {
			if rando.rand() < get_item_forage_probability(item) {
				results.append(item)
				if results.count >= MAX_FORAGE_RESULTS {
					break
				}
			}
		}

		return results
	}

	private func get_item_forage_probability(_ item: ItemModel) -> Float {
		switch item.rarity {
			case .Common: return 0.8
			case .Rare:   return 0.09
			case .Scarce: return 0.01
		}
	}

	private func get_current_conditions() -> Set<ItemFindCondition> {
		var conditions: Set<ItemFindCondition> = []
		if !is_it_daytime() {
			conditions.insert(.Night)
		}
		if is_it_morning() {
			conditions.insert(.Morning)
		}
		if is_it_afternoon() {
			conditions.insert(.Afternoon)
		}
		return conditions
	}

	private func get_foraging_possibilities(_ forage_key: ForageKey) -> [ItemModel] {
		if let possibilities = foraging_possibilities_cache[forage_key] {
			return possibilities
		}

		var possibilities: [ItemModel] = []
		for item in world.items {
			guard let item_location = item.where_found else { continue }

			if let condition = item.when_found, !forage_key.conditions.contains(condition) {
				continue
			}

			if item_location == forage_key.location || (world.adjacencies[item_location]?.contains(forage_key.location) ?? false) {
				possibilities.append(item)
			}
		}

		foraging_possibilities_cache[forage_key] = possibilities
		return possibilities
	}

	public func is_it_daytime() -> Bool {
		switch season {
			case .Estival: return true
			case .Vernal, .Serotinal: return time_of_day < 5
			case .Prevernal, .Autumnal: return time_of_day < 4
			case .Hibernal: return time_of_day < 3
		}
	}

	public func is_it_morning() -> Bool {
		switch season {
			case .Estival: return time_of_day < 3
			case .Vernal, .Serotinal, .Prevernal, .Autumnal: return time_of_day < 2
			case .Hibernal: return time_of_day < 1
		}
	}

	public func is_it_afternoon() -> Bool {
		switch season {
			case .Estival, .Vernal, .Serotinal: return time_of_day >= 3
			case .Prevernal, .Autumnal, .Hibernal: return time_of_day >= 2
		}
	}

	public static func get_ground_item(_ item: Item) -> Item {
		var aspects = item.aspects
		modify_aspect(&aspects, 1, at: 0)
		modify_aspect(&aspects, -1, at: 1)
		return Item(
			raw: item.raw,
			aspects: aspects,
			type: item.type.without(.Raw).with(.Ground)
		)
	}

	public static func get_roasted_item(_ item: Item) -> Item {
		var aspects = item.aspects
		mutate_aspect(&aspects, at: -1)
		return Item(
			raw: item.raw,
			aspects: aspects,
			type: item.type.without(.Raw).with(.Roasted)
		)
	}

	public static func get_infusion(_ items: [Item]) -> Item {
		var aspects = items.reduce([], { combine_aspects($0, $1.aspects) })
		for i in (0..<aspects.count).reversed() {
			modify_aspect(&aspects, -1, at: i)
		}
		return Item(
			raw: items.reduce(into: [], { $0.append(contentsOf: $1.raw) }),
			aspects: aspects,
			type: [.Infusion]
		)
	}

	private static func get_index(pos: Int, list_length: Int) -> Int? {
		let pos = pos < 0 ? list_length + pos : pos
		return (pos < 0 || pos >= list_length) ? nil : pos
	}

	private static func modify_aspect(_ aspects: inout [(Aspect, Int)], _ amount: Int, at: Int) {
		if let index = get_index(pos: at, list_length: aspects.count) {
			aspects[index].1 += amount
			if aspects[index].1 <= 0 {
				aspects.remove(at: index)
			}
		}
	}

	private static func mutate_aspect(_ aspects: inout [(Aspect, Int)], at: Int) {
		if let index = get_index(pos: at, list_length: aspects.count) {
			if let mutates_into = aspects[index].0.mutates_into {
				aspects[index].0 = mutates_into
			}
		}
	}

	private static func combine_aspects(_ aspects1: [(Aspect, Int)], _ aspects2: [(Aspect, Int)]) -> [(Aspect, Int)] {
		var aspects = aspects1
		for (aspect, amount) in aspects2 {
			if let index = aspects.firstIndex(where: { $0.0 == aspect }) {
				aspects[index].1 += amount
			} else {
				aspects.append((aspect, amount))
			}
		}
		return aspects
	}

	private static func create_world() -> World {
		let aspects = [
			Aspect("bloom"),
			Aspect("caust"),
			Aspect("spice"),
			Aspect("vigor"),
			Aspect("umber"),
			Aspect("gelus")
		]

		return World(
			locations: [],
			adjacencies: [:],
			aspects: aspects,
			items: []
		)
	}
}
