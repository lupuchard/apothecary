using System;
using Godot;
using MessagePack;
using MessagePack.Formatters;

namespace Apothecary;

[MessagePackFormatter(typeof(AspectFormatter))]
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
	
	public class AspectFormatter : IMessagePackFormatter<Aspect?> {
		public void Serialize(ref MessagePackWriter writer, Aspect? value, MessagePackSerializerOptions options) {
			if (value == null) {
				writer.WriteNil();
			} else {
				options.Resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Id, options);
			}
		}

		public Aspect? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			if (reader.IsNil) {
				return null;
			} else {
				var id = options.Resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				return Game.Instance.World.GetAspect(id) ?? UnknownAspect;
			}
		}
	}
}
