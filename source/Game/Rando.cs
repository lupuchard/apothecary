using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Numerics;

namespace Apothecary;

public struct Rando {
	private ulong s0;
	private ulong s1;
	private ulong s2;
	private ulong s3;

	public Rando(ulong seed) {
		var z = seed;
		s0 = InitS(ref z);
		s1 = InitS(ref z);
		s2 = InitS(ref z);
		s3 = InitS(ref z);
	}

	public Rando(ulong s0, ulong s1, ulong s2, ulong s3) {
		this.s0 = s0;
		this.s1 = s1;
		this.s2 = s2;
		this.s3 = s3;
	}

	public (ulong, ulong, ulong, ulong) State() {
		return (s0, s1, s2, s3);
	}

	private static ulong InitS(ref ulong z) {
		unchecked {
			z += 0x9e3779b97f4a7c15;
			z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9;
			z = (z ^ (z >> 27)) * 0x94d049bb133111eb;
			return z ^ (z >> 31);
		}
	}

	private static ulong Rotl(ulong x, int k) {
		unchecked {
			return (x << k) | (x >> (64 - k));
		}
	}

	public ulong Rand() {
		unchecked {
			var result = Rotl(s1 * 5, 7) * 9;
			var t = s1 << 17;
			s2 ^= s0;
			s3 ^= s1;
			s1 ^= s2;
			s0 ^= s3;
			s2 ^= t;
			s3 = Rotl(s3, 45);
			return result;
		}
	}

	public T RandInt<T>(T low, T high) where T : IBinaryInteger<T> {
		unchecked { return low + T.Abs(T.CreateTruncating(Rand())) % (high - low); }
	}

	public int Rand(System.Range range) {
		return RandInt(range.Start.Value, range.End.Value);
	}

	public bool RandBool() {
		return RandInt(0, 2) == 1;
	}

	public double RandDouble() {
		return Rand() / (double)ulong.MaxValue;
	}

	public ulong Next() {
		return Rand();
	}

	[Pure] public List<T> Shuffle<T>(IList<T> list) {
		var new_list = new List<T>(list);
		for (var i = 0; i < new_list.Count; i++) {
			var k = RandInt(0, new_list.Count);
			(new_list[i], new_list[k]) = (new_list[k], new_list[i]);
		}
		return new_list;
	}

	public T Pick<T>(IList<T> list) {
		return list[RandInt(0, list.Count)];
	}
}

/*public class RandoSerdeObj : ISerde<Rando> {
	public ISerdeInfo SerdeInfo { get; } = StringProxy.SerdeInfo.WithName("Rando");
	
	public void Serialize(Rando rando, ISerializer serializer) {
		var (s0, s1, s2, s3) = rando.State();
		var type_ser = serializer.WriteType(SerdeInfo);
		type_ser.WriteU64(SerdeInfo, 0, s0);
		type_ser.WriteU64(SerdeInfo, 1, s1);
		type_ser.WriteU64(SerdeInfo, 2, s2);
		type_ser.WriteU64(SerdeInfo, 3, s3);
		type_ser.End(SerdeInfo);
	}

	public Rando Deserialize(IDeserializer deserializer) {
		var type_ser = deserializer.ReadType(SerdeInfo);
		var s0 = type_ser.ReadU64(SerdeInfo, 0);
		var s1 = type_ser.ReadU64(SerdeInfo, 1);
		var s2 = type_ser.ReadU64(SerdeInfo, 2);
		var s3 = type_ser.ReadU64(SerdeInfo, 3);
		type_ser.End(SerdeInfo);
		return new Rando(s0, s1, s2, s3);
	}
}*/
