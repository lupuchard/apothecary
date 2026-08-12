using System.Collections.Immutable;
using Godot;

namespace Apothecary;

public class VisitorType {
	public string Id { get; }
	public Texture2D Sprite { get; }
	public Texture2D SpriteSmall { get; }
	public ImmutableArray<string> FirstNames { get; }
	public ImmutableArray<string> LastNames { get; }
	public ImmutableArray<Resource> Reward { get; }
	public ImmutableArray<Resource> Tip { get; }
	
	public static readonly VisitorType UnknownVisitorType = new("unknown", [], [], [], []);

	public VisitorType(string id, string[] first_names, string[] last_names, Resource[] reward, Resource[] tip) {
		Id = id;
		FirstNames = [..first_names];
		LastNames = [..last_names];
		Reward = [..reward];
		Tip = [..tip];
		
		Sprite = ResourceLoader.Load<Texture2D>($"res://assets/visitor/{id}.png");
		SpriteSmall = ResourceLoader.Load<Texture2D>($"res://assets/visitor/{id}_small.png");
	}
}
