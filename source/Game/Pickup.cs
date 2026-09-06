namespace Apothecary;

public class Pickup(PickupType Type, ItemModel? Item = null, Resource? Resource = null) {
	public PickupType Type { get; } = Type;
	public ItemModel? Item { get; } = Item;
	public Resource? Resource { get; } = Resource;
	
	public static readonly Pickup Empty = new(PickupType.Empty);

	public static Pickup ItemModel(ItemModel item) {
		return new Pickup(PickupType.ItemModel, Item: item);
	}
	
	public static Pickup Material(Resource resource) {
		return new Pickup(PickupType.Material, Resource: resource);
	}
}
