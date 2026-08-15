using Godot;

namespace Apothecary;

public static class BbCodeUtil {
	public static string Img(string path, Color color) {
		return $"[img color={color.ToHtml()}]{path}[/img]";
	}
}
