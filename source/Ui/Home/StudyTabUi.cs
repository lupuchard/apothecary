namespace Apothecary;

public partial class StudyTabUi : TabBaseUi {
	public override bool IsUnlocked() {
		return Game.Instance.IsUnlocked(Feature.Study);
	}
}
