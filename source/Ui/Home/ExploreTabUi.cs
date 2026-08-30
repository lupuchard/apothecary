using System.Collections.Immutable;
using Godot;

namespace Apothecary;

public partial class ExploreTabUi : TabBaseUi {
	private Label? Description;
	private SlowButton? Button;

	private static readonly ImmutableArray<(Feature, string)> features = [
		(Feature.Kitchen, "EXPLORE_KITCHEN"),
		(Feature.Bedroom, "EXPLORE_BEDROOM"),
		(Feature.Journal, "EXPLORE_JOURNAL"),
		(Feature.Grinder, "EXPLORE_GRINDER"),
		(Feature.Firewood, "EXPLORE_FIREWOOD"),
		(Feature.Roaster, "EXPLORE_ROASTER"),
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
		foreach (var (feature, description) in features) {
			if (!Game.Instance.IsUnlocked(feature)) {
				Game.Instance.UnlockFeature(feature);
				Game.Instance.ModifyResource(Resource.Focus, -1);
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
