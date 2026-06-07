using Godot;

namespace Apothecary;

public partial class RegionModel(string id, int max_forage, double forage_recovery) : RefCounted {
	public string Id { get; } = id;
	public int MaxForage { get; } = max_forage;
	public double ForageRecovery { get; } = forage_recovery;
}
