using System;

namespace Apothecary;

public partial class RegionLocationLabel : LocationLabel {
	public RegionModel? Region { get; set; }
	
	public override string GetTitleText() {
		if (Region == null) return "ERROR_NO_REGION";
		return Game.Instance.GetRegion(Region.Id)?.TrString() ?? "ERROR_NO_REGION";
	}

	public override string GetSubtitleText() {
		if (Region == null || Game.Instance.GetRegion(Region.Id)?.Unlocked == true) return "";
		
		var req = Region?.UnlockRequirement ?? UnlockRequirement.None;
		var game = Game.Instance;
		return req.Type switch {
			UnlockRequirementType.None 
				=> "",
			UnlockRequirementType.Day 
				=> string.Format(Tr("UNLOCKED_ON_DAY"), req.Amount + 1),
			UnlockRequirementType.ResourceAcquired 
				=> string.Format(Tr("UNLOCKED_ON_RESOURCE"), req.Amount, Tr(req.Resource.TrString(req.Amount != 1)).ToLower(), game.GetResource(req.Resource)),
			UnlockRequirementType.ConfirmedJournalEntries 
				=> string.Format(Tr("UNLOCKED_ON_JOURNAL"), req.Amount, game.Journal.TotalConfirmed),
			_ => throw new ArgumentOutOfRangeException()
		};
	}
}
