using UnityEngine;

public struct IntRange
{
	public int min;

	public int max;

	public float Average => (float)(min + max) / 2f;

	public int MiddleValue => Middle(min, max);

	public int RangeCount => max - min + 1;

	public bool IsFixed => min == max;

	public IntRange(int min, int max)
	{
		this.min = min;
		this.max = max;
	}

	public int Lerp(float lerp)
	{
		if (IsFixed)
		{
			return min;
		}
		return Mathf.FloorToInt(Mathf.Lerp(min, (float)(max + 1) - float.Epsilon, lerp));
	}

	public int ReverseLerp(float lerp)
	{
		if (IsFixed)
		{
			return min;
		}
		return Mathf.FloorToInt(Mathf.Lerp(min, (float)(max + 1) - float.Epsilon, 1f - lerp));
	}

	public int Middle(int a, int b)
	{
		return (a + b) / 2;
	}
}
