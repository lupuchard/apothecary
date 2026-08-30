using System.Text.Json.Serialization;

namespace Apothecary;

[method: JsonConstructor]
public class Visitor(RequestModel Request, int RemainingDays, string Name, string RequestText) {
	public int RemainingDays { get; set; } = RemainingDays;
	public RequestModel Request { get; } = Request;
	public string RequestText { get; } = RequestText;
	public string Name { get; } = Name;
	
	public Visitor(RequestModel request, ref Rando rando)
		: this(request, 3, GenerateName(request, ref rando), request.GenText(ref rando)) { }

	public static string GenerateName(RequestModel request, ref Rando rando) {
		return rando.Pick(request.Type.FirstNames) + " " + rando.Pick(request.Type.LastNames);
	}
}
