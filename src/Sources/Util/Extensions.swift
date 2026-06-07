
public extension Set where Element: RawRepresentable & Hashable & CaseIterable {
	var rawValue: Int {
		var rawValue = 0
		for (index, element) in Element.allCases.enumerated() where contains(element) {
			rawValue |= (1 << index)
		}
		return rawValue
	}

	func without(_ e: Element) -> Self {
		var copy = self
		copy.remove(e)
		return copy
	}

	func with(_ e: Element) -> Self {
		var copy = self
		copy.insert(e)
		return copy
	}
}
