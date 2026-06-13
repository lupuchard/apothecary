using System.Collections.Generic;
namespace Apothecary;

public struct Reward(List<(Resource, int)> Rewards) {
	public List<(Resource, int)> Rewards = Rewards;
}
