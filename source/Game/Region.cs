using System;
using MessagePack;

namespace Apothecary;

[MessagePackObject]
public partial class Region(RegionModel Model) {
	[Key("model")] public RegionModel Model { get; } = Model;
	[Key("remaining")] public int Remaining { get; private set; } = Model.MaxForage;
	[Key("unlocked")] public bool Unlocked { get; set; }
	[Key("known")] public bool Known { get; set; } = true;

	public void ConsumeForage(int amount = 1) {
		Remaining = Math.Max(0, Remaining - amount);
	}

	public void DailyRecovery(ref Rando rando) {
		var base_recovery = (int)Math.Floor(Model.ForageRecovery);
		Remaining += base_recovery;
		Remaining += (rando.Rand() < (Model.ForageRecovery - base_recovery)) ? 1 : 0;
		if (Remaining > Model.MaxForage) {
			Remaining = Model.MaxForage;
		}
	}

	public string TrString() {
		return Unlocked ? Model.Id.ToUpper() : Model.Id.ToUpper() + "_UNKNOWN";
	}
}
