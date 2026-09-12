using System.Text.Json.Serialization;
using Godot;

namespace Apothecary;

[method: JsonConstructor]
public class Visitor(RequestModel? Request, SpecialRequest Special, int RemainingDays, string Name, string RequestText, int? Amount) {
	public int RemainingDays { get; set; } = RemainingDays;
	public RequestModel? Request { get; } = Request;
	public SpecialRequest Special { get; } = Special;
	public int? Amount { get; } = Amount;
	public string RequestText { get; } = RequestText;
	public string Name { get; } = Name;
	
	public Visitor(RequestModel request, ref Rando rando)
		: this(request, SpecialRequest.None, 3, GenerateName(request, ref rando), request.GenText(ref rando), null) { }

	public Visitor(SpecialRequest request, int days, string request_text, int amount)
		: this(null, request, days, TranslationServer.Translate(request.TrString()), request_text, amount) { }

	public static string GenerateName(RequestModel request, ref Rando rando) {
		return rando.Pick(request.Type.FirstNames) + " " + rando.Pick(request.Type.LastNames);
	}

	public bool CanReject() {
		return Special != SpecialRequest.Bills;
	}
}
