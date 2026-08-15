using System;
using Godot;
namespace Apothecary;

public partial class MapColor : Sprite2D {
	private Tween? tween;
	
	public void SetColor(int time_of_day, Season season, double transition_time = 0.0) {
		base._Ready();
		
		var sunset = GetSunset(season);
		var noon = sunset / 2.0f;

		float temp_kelvin;
		if (time_of_day < noon) {
			temp_kelvin = float.Lerp(3000.0f, 6500.0f, time_of_day / noon);
		} else if (time_of_day < sunset) {
			temp_kelvin = float.Lerp(6500.0f, 3000.0f, (time_of_day - noon) / sunset);
		} else {
			temp_kelvin = 4000.0f;
		}
		
		float light;
		if (time_of_day == 0 || time_of_day == sunset) {
			light = 0.9f;
		} else if (time_of_day > sunset) {
			light = 0.4f;
		} else {
			light = 1.0f;
		}

		var color = CalcColorTemp(temp_kelvin) * light;
		if (transition_time <= 0.0f) {
			SetModulate(color);
		} else {
			tween?.Kill();
			tween = CreateTween();
			tween.TweenProperty(this, "modulate", color, transition_time);
		}
	}

	private static Color CalcColorTemp(float kelvin) {
		var temp = kelvin / 100.0f;
		
		var red = temp <= 66.0f
			? 255.0f
			: Math.Clamp(329.698727446f * (float)Math.Pow(temp - 60.0f, -0.1332047592f), 0.0f, 255.0f);
		
		var green = temp <= 66.0f
			? Math.Clamp(99.4708025861f * (float)Math.Log(temp) - 161.1195681661f, 0.0f, 255.0f)
			: Math.Clamp(288.1221695283f * (float)Math.Pow(temp - 60.0f, -0.0755148492), 0.0f, 255.0f);

		var blue = temp is < 66.6f and > 19.0f
			? Math.Clamp(138.5177312231f * (float)Math.Log(temp - 10.0f) - 305.0447927307f, 0.0f, 255.0f)
			: (temp >= 66.6 ? 255.0f : 0.0f);
		
		return new Color(red / 255.0f, green / 255.0f, blue / 255.0f);
	}

	private static int GetSunset(Season season) {
		return season switch {
			Season.Prevernal => 3,
			Season.Vernal => 4,
			Season.Estival => 5,
			Season.Serotinal => 4,
			Season.Autumnal => 3,
			Season.Hibernal => 2,
			_ => throw new ArgumentOutOfRangeException(nameof(season), season, null)
		};
	}
}
