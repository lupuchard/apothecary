namespace Apothecary;

public class Pickup {
	private Pickup(PickupType type, ItemModel? item = null, Resource? resource = null) {
		Type = type;
		Item = item;
		Resource = resource;
	}
	
	public PickupType Type { get; }
	public ItemModel? Item { get; }
	public Resource? Resource { get; }
	
	public static readonly Pickup Empty = new(PickupType.Empty);

	public static Pickup ItemModel(ItemModel item) {
		return new Pickup(PickupType.ItemModel, item: item);
	}
	
	public static Pickup Material(Resource resource) {
		return new Pickup(PickupType.Material, resource: resource);
	}
}
