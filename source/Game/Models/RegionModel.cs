using Godot;
using MessagePack;
using MessagePack.Formatters;

namespace Apothecary;

[MessagePackFormatter(typeof(RegionModelFormatter))]
public partial class RegionModel(string id, int max_forage, double forage_recovery, UnlockRequirement unlock_requirement) : RefCounted {
	public string Id { get; } = id;
	public int MaxForage { get; } = max_forage;
	public double ForageRecovery { get; } = forage_recovery;
	public UnlockRequirement UnlockRequirement { get; } = unlock_requirement;

	public static readonly RegionModel UnknownRegionModel = new("unknown", 0, 0, UnlockRequirement.None);

	public class RegionModelFormatter : IMessagePackFormatter<RegionModel?> {
		public void Serialize(ref MessagePackWriter writer, RegionModel? value, MessagePackSerializerOptions options) {
			if (value == null) {
				writer.WriteNil();
			} else {
				options.Resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Id, options);
			}
		}

		public RegionModel? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			if (reader.IsNil) {
				return null;
			} else {
				var id = options.Resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				return Game.Instance.World.GetRegionModel(id) ?? UnknownRegionModel;
			}
		}
	}
}
