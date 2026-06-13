namespace Apothecary;

public class Visitor {
	public int RemainingDays { get; set; }
	public RequestModel Request { get; }
	public string RequestText { get; }
	public string Name { get; }

	public Visitor(RequestModel request, ref Rando rando) {
		Request = request;
		RequestText = request.GenText(ref rando);
		RemainingDays = 3;
		Name = rando.Pick(request.Type.FirstNames) + " " + rando.Pick(request.Type.LastNames);
	}
}
