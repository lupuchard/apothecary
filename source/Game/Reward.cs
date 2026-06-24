using System.Collections.Generic;
using System.Linq;

namespace Apothecary;

public struct Reward(IEnumerable<(Resource resource, int amount)> Rewards) {
	public readonly List<(Resource, int)> Rewards = [
		..Rewards.Where(reward => reward.amount > 0)
			.GroupBy(reward => reward.resource)
			.Select(group => (group.Key, group.Sum(reward => reward.amount)))
	];
}
