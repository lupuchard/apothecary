using System;

namespace Apothecary;

public class Region(RegionModel Model) {
	public RegionModel Model { get; } = Model; 
	public int Remaining { get; set; } = Model.MaxForage;
	public bool Unlocked { get; set; }
	public bool Known { get; set; } = true;

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
