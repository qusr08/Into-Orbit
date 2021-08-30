using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils {
	private static System.Random random = new System.Random( );

	public static float Limit (float value, float min, float max) {
		if (value < min) {
			value = min;
		}

		if (value > max) {
			value = max;
		}

		return value;
	}

	#region Random Methods

	public static float RandFloat (float min, float max) {
		return (float) (random.NextDouble( ) * (max - min)) + min;
	}

	public static Vector2 RandNormVect2 ( ) {
		return new Vector2(RandFloat(-1, 1), RandFloat(-1, 1));
	}

	#endregion
}

[System.Serializable]
public class SerializableColor {
	// https://answers.unity.com/questions/772235/cannot-serialize-color.html

	public float[ ] colorValues = new float[3] { 1f, 1f, 1f };
	public Color Color {
		get {
			return new Color(colorValues[0], colorValues[1], colorValues[2]);
		}

		set {
			colorValues = new float[3] { value.r, value.g, value.b };
		}
	}

	// Makes this class usable as Color, Color normalColor = mySerializableColor;
	public static implicit operator Color (SerializableColor instance) {
		return instance.Color;
	}

	// Makes this class assignable by Color, SerializableColor myColor = Color.white;
	public static implicit operator SerializableColor (Color color) {
		return new SerializableColor { Color = color };
	}
}
