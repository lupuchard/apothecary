using System.Text.Json.Serialization;

namespace Apothecary;

[JsonConverter(typeof(JsonStringEnumConverter<InventorySortMethod>))]
public enum InventorySortMethod {
	Index,
	Name,
	Type,
}
