using System.Collections.Immutable;
using Godot;

namespace Apothecary;

public partial class ExploreTabUi : TabBaseUi {
	private Label? Description;
	private SlowButton? Button;

	private static readonly ImmutableArray<(Feature, string, Reward?)> features = [
		(Feature.Kitchen, "EXPLORE_KITCHEN", null),
		(Feature.Bedroom, "EXPLORE_BEDROOM", null),
		(Feature.Journal, "EXPLORE_JOURNAL", null),
		(Feature.Grinder, "EXPLORE_GRINDER", null),
		(Feature.Firewood, "EXPLORE_FIREWOOD", new Reward([(Resource.Stamina, 2), (Resource.StaminaMax, 2)]))
	];

	public override void _Ready() {
		Description = GetNode<Label>("%ExploreDescription");
		Button = GetNode<SlowButton>("%ExploreButton");
		Button.Pressed += OnExplore;
	}

	public override void Update() {
		Description?.Text = "";
		Button?.Update();
	}

	private void OnExplore() {
		foreach (var (feature, description, reward) in features) {
			if (!Game.Instance.IsUnlocked(feature)) {
				Game.Instance.UnlockFeature(feature);
				Game.Instance.ModifyResource(Resource.Focus, -1);
				if (reward != null) Game.Instance.GetReward(reward.Value);
				Game.Instance.PassTime();
				Description?.Text = Tr(description);
				return;
			}
		}
	}

	public override bool IsUnlocked() {
		return !Game.Instance.IsUnlocked(features[^1].Item1);
	}
}
