using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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

	public static float Map (float value, float rangeStart, float rangeEnd, float newRangeStart, float newRangeEnd) {
		return newRangeStart + ((newRangeEnd - newRangeStart) / (rangeEnd - rangeStart)) * (value - rangeStart);
	}

	public static bool CloseEnough (Vector3 vector1, Vector3 vector2, float checkValue = 0.01f) {
		return Vector3.Distance(vector1, vector2) < checkValue;
	}

	public static Color Hex2Color (string hexString) {
		// https://www.devx.com/tips/dot-net/c-sharp/convert-hex-to-rgb-190527095529.html

		// Make sure that the hex string doesn't break the code below (and so Unity doesn't throw 129830712 errors)
		if (hexString.Length != 6) {
			return Color.white;
		}

		// Get RGB values based on hex code
		int r, g, b = 0;
		r = int.Parse(hexString.Substring(0, 2), NumberStyles.AllowHexSpecifier);
		g = int.Parse(hexString.Substring(2, 2), NumberStyles.AllowHexSpecifier);
		b = int.Parse(hexString.Substring(4, 2), NumberStyles.AllowHexSpecifier);

		// Return the color with the RGB values on a scale from 0 to 1
		return new Color(r / 255f, g / 255f, b / 255f);
	}

	public static string Color2Hex (Color color) {
		string hexString = $"{color.r:X2}{color.g:X2}{color.b:X2}";

		// Make sure that the hex string doesn't break the code below (and so Unity doesn't throw 129830712 errors)
		if (hexString.Length != 6) {
			return "FFFFFF";
		}

		return hexString;
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

	public static Color RandColorInRange (Color color, float maxOffset, bool keepHue = true) {
		float newR, newG, newB = 0;

		if (!keepHue) {
			newR = color.r + RandFloat(-maxOffset, maxOffset);
			newG = color.g + RandFloat(-maxOffset, maxOffset);
			newB = color.b + RandFloat(-maxOffset, maxOffset);
		} else {
			float colorOffset = RandFloat(-maxOffset, maxOffset);

			newR = color.r + colorOffset;
			newG = color.g + colorOffset;
			newB = color.b + colorOffset;
		}

		return new Color(Limit(newR, 0f, 1f), Limit(newG, 0f, 1f), Limit(newB, 0f, 1f));
	}

	#endregion

	#region Vector Methods

	public static Vector3 LinearInterpolation (float percentage, Vector3 p1, Vector3 p2) {
		return p1 + (percentage * (p2 - p1));
	}

	public static Vector3 SetVectZ (Vector3 vector, float z) {
		return new Vector3(vector.x, vector.y, z);
	}

	public static Vector3[ ] SetVectZ (Vector3[ ] vectors, float z) {
		for (int i = 0; i < vectors.Length; i++) {
			vectors[i] = SetVectZ(vectors[i], z);
		}

		return vectors;
	}

	public static Vector3 LimitVect3 (Vector3 center, Vector3 point, float minDistance, float maxDistance) {
		float distance = Vector3.Distance(center, point);
		if (distance < minDistance) {
			point = LinearInterpolation(minDistance / distance, center, point);
		}

		if (distance > maxDistance) {
			point = LinearInterpolation(maxDistance / distance, center, point);
		}

		return point;
	}

	public static float GetAngleBetween (Vector2 center, Vector2 point) {
		return Mathf.Rad2Deg * Mathf.Atan2(point.y - center.y, point.x - center.x);
	}

	#endregion
}

[System.Serializable]
public class SerializableColor3 {
	// https://answers.unity.com/questions/772235/cannot-serialize-color.html

	public int[ ] colorValues = new int[3] { 255, 255, 255 };
	public Color Color {
		get {
			return new Color(colorValues[0] / 255f, colorValues[1] / 255f, colorValues[2] / 255f);
		}

		set {
			colorValues = new int[3] { (int) (value.r * 255), (int) (value.g * 255), (int) (value.b * 255) };
		}
	}

	public float R {
		get {
			return Color.r;
		}
	}

	public float G {
		get {
			return Color.g;
		}
	}

	public float B {
		get {
			return Color.b;
		}
	}

	// Makes this class usable as Color, Color normalColor = mySerializableColor;
	public static implicit operator Color (SerializableColor3 instance) {
		return instance.Color;
	}

	// Makes this class assignable by Color, SerializableColor myColor = Color.white;
	public static implicit operator SerializableColor3 (Color color) {
		return new SerializableColor3 { Color = color };
	}
}

[System.Serializable]
public class SerializableColor4 {
	public int[ ] colorValues = new int[4] { 255, 255, 255, 255 };
	public Color Color {
		get {
			return new Color(colorValues[0] / 255f, colorValues[1] / 255f, colorValues[2] / 255f, colorValues[3] / 255f);
		}

		set {
			colorValues = new int[4] { (int) (value.r * 255), (int) (value.g * 255), (int) (value.b * 255), (int) (value.a * 255) };
		}
	}

	public float R {
		get {
			return Color.r;
		}
	}

	public float G {
		get {
			return Color.g;
		}
	}

	public float B {
		get {
			return Color.b;
		}
	}

	public float A {
		get {
			return Color.a;
		}
	}

	// Makes this class usable as Color, Color normalColor = mySerializableColor;
	public static implicit operator Color (SerializableColor4 instance) {
		return instance.Color;
	}

	// Makes this class assignable by Color, SerializableColor myColor = Color.white;
	public static implicit operator SerializableColor4 (Color color) {
		return new SerializableColor4 { Color = color };
	}
}