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

	public static Vector2 LinearInterpolation (float percentage, Vector3 p1, Vector3 p2) {
		return p1 + (percentage * (p2 - p1));
	}

	public static bool CloseEnough (Vector3 vector1, Vector3 vector2) {
		return Vector3.Distance(vector1, vector2) < 0.01f;
	}

	#region Random Methods

	public static float RandFloat (float min, float max) {
		return (float) (random.NextDouble( ) * (max - min)) + min;
	}

	public static Vector2 RandNormVect2 ( ) {
		return new Vector2(RandFloat(-1, 1), RandFloat(-1, 1)).normalized;
	}

	public static Vector2 RandVect2OnArc (Vector2 arcCenter, float angleRangeDeg) {
		float angleChange = Mathf.Deg2Rad * RandFloat(-angleRangeDeg / 2, angleRangeDeg / 2);
		float centerAngle = Mathf.Atan2(arcCenter.y, arcCenter.x) + (Mathf.PI / 2);

		return new Vector2(Mathf.Cos(angleChange + centerAngle), Mathf.Sin(angleChange + centerAngle)) * arcCenter.magnitude;
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
