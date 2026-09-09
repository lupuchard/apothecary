using System.Text.Json.Serialization;

namespace Apothecary;

[JsonConverter(typeof(JsonStringEnumConverter<Feature>))]
public enum Feature {
	None,
	Kitchen,
	Grinder,
	Roaster,
	Bedroom,
	Journal,
	Firewood,
	Study
}
