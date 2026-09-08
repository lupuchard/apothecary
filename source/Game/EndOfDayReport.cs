using System.Collections.Generic;
using System.Collections.Immutable;

namespace Apothecary;

public record EndOfDayReport {
	public required List<Visitor> FailedRequests;
	public required ImmutableArray<(Resource resource, int amount)> ResourceSummary;
}
