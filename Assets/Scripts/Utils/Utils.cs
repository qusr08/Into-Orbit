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

	public static float RandFloat (float min, float max) {
		return (float) (random.NextDouble( ) * (max - min)) + min;
	}

	public static Vector2 RandNormVect2 ( ) {
		return new Vector2(RandFloat(-1, 1), RandFloat(-1, 1));
	}
}
