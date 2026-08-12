using MessagePack;

namespace Apothecary;

[MessagePackObject]
public class Visitor {
	[Key("remaining_days")] public int RemainingDays { get; set; }
	[Key("request")] public RequestModel Request { get; }
	[Key("request_text")] public string RequestText { get; }
	[Key("name")] public string Name { get; }

	public Visitor(RequestModel request, ref Rando rando) {
		Request = request;
		RequestText = request.GenText(ref rando);
		RemainingDays = 3;
		Name = rando.Pick(request.Type.FirstNames) + " " + rando.Pick(request.Type.LastNames);
	}

	public Visitor(RequestModel request, int remainingDays, string name, string requestText) {
		Request = request;
		RemainingDays = remainingDays;
		Name = name;
		RequestText = requestText;
	}
}
