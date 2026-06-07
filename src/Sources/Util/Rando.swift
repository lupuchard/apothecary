import Foundation

public struct Rando : RandomNumberGenerator {
	private var s0: UInt64
	private var s1: UInt64
	private var s2: UInt64
	private var s3: UInt64

	public init(seed: UInt64) {
		var z = seed
		s0 = Rando.init_s(&z)
		s1 = Rando.init_s(&z)
		s2 = Rando.init_s(&z)
		s3 = Rando.init_s(&z)
	}

	private init(_ s0: UInt64, _ s1: UInt64, _ s2: UInt64, _ s3: UInt64) {
		self.s0 = s0
		self.s1 = s1
		self.s2 = s2
		self.s3 = s3
	}

	private static func init_s(_ z: inout UInt64) -> UInt64 {
		z &+= 0x9e3779b97f4a7c15
		z = (z ^ (z >> 30)) &* 0xbf58476d1ce4e5b9
		z = (z ^ (z >> 27)) &* 0x94d049bb133111eb
		return z ^ (z >> 31)
	}

	private static func rotl(_ x: UInt64, _ k: UInt64) -> UInt64 {
		return (x << k) | (x >> (64 &- k))
	}

	public mutating func next() -> UInt64 {
		let result = Rando.rotl(s1 &* 5, 7) &* 9
		let t = s1 << 17
		s2 ^= s0
		s3 ^= s1
		s1 ^= s2
		s0 ^= s3
		s2 ^= t
		s3 = Rando.rotl(s3, 45)
		return result
	}

	public mutating func rand<U: FloatingPoint>() -> U {
		U(next()) / U(UInt64.max)
	}

	public mutating func rand<U: FloatingPoint>(_ range: Range<U>) -> U {
		rand() / (range.upperBound - range.lowerBound) + range.lowerBound
	}

	public mutating func rand_bool() -> Bool {
		rand(0..<2) == 1
	}

	public mutating func weighted_random_select(weights: [Double]) -> Int {
		assert(!weights.isEmpty)

		var cumulative = 0.0
		let rand_val: Double = rand()
		for (i, weight) in weights.enumerated() {
			cumulative += weight
			if rand_val < cumulative {
				return i
			}
		}

		return weights.count - 1
	}
}