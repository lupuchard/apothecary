using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace Apothecary;

[JsonConverter(typeof(AspectConverter))]
public class Aspect {
	public static readonly Color Orange = new(1, 0.5f, 0);
	public static readonly Color Chartreuse = new(0.5f, 1, 0);
	public static readonly Color SpringGreen = new(0, 1, 0.5f);
	public static readonly Color Azure = new(0, 0.5f, 1);
	public static readonly Color Violet = new(0.5f, 0, 1);
	public static readonly Color Rose = new(1, 0, 0.5f);
	
	public string Id { get; }
	public Texture2D Sprite { get; }
	public Color Color { get; }

	private readonly Func<Aspect> mutates_into;
	public Aspect MutatesInto => mutates_into();

	private static readonly Aspect UnknownAspect = new("unknown", Colors.Black, () => UnknownAspect!);

	public Aspect(string id, Color color, Func<Aspect> mutates_into) {
		Id = id;
		Color = color;
		Sprite = ResourceLoader.Load<Texture2D>($"res://assets/aspect/{id}.png");
		this.mutates_into = mutates_into;
	}
}

public class AspectConverter : JsonConverter<Aspect?> {
	public override Aspect? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		var id = reader.GetString();
		return id == null ? null : Game.Instance.World.GetAspect(id);
	}
	public override void Write(Utf8JsonWriter writer, Aspect? aspect, JsonSerializerOptions options) {
		writer.WriteStringValue(aspect?.Id);
	}
}
