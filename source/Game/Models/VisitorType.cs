using Godot;

namespace Apothecary;

public class VisitorType {
	public string Id { get; }
	public Texture2D Sprite { get; }
	public Texture2D SpriteSmall { get; }
	public string[] FirstNames { get; }
	public string[] LastNames { get; }

	public VisitorType(string id, string[] first_names, string[] last_names) {
		Id = id;
		FirstNames = first_names;
		LastNames = last_names;
		
		Sprite = ResourceLoader.Load<Texture2D>($"res://assets/visitor/{id}.png");
		SpriteSmall = ResourceLoader.Load<Texture2D>($"res://assets/visitor/{id}_small.png");
	}
}
