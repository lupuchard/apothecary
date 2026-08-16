using Godot;
using Serde;

namespace Apothecary;

[GenerateSerde(With = typeof(RegionModelSerdeObj))]
public partial class RegionModel(string id, int max_forage, double forage_recovery, UnlockRequirement unlock_requirement) : RefCounted {
	public string Id { get; } = id;
	public int MaxForage { get; } = max_forage;
	public double ForageRecovery { get; } = forage_recovery;
	public UnlockRequirement UnlockRequirement { get; } = unlock_requirement;

	public static readonly RegionModel UnknownRegionModel = new("unknown", 0, 0, UnlockRequirement.None);
}

public class RegionModelSerdeObj : ISerde<RegionModel?> {
	public ISerdeInfo SerdeInfo { get; } = StringProxy.SerdeInfo.WithName("RegionModel");

	public void Serialize(RegionModel? region, ISerializer serializer) {
		if (region == null) {
			serializer.WriteNull();
		} else {
			serializer.WriteString(region.Id);
		}
	}

	public RegionModel? Deserialize(IDeserializer deserializer) {
		return deserializer.TryReadNull() ? null : Game.Instance.World.GetRegionModel(deserializer.ReadString());
	}
}
