using UnityEngine;

public static class CombatValueLimit
{
	public const int Max = 9999999;

	public static int Clamp(long value)
	{
		if (value <= 0)
		{
			return 0;
		}
		if (value >= 9999999)
		{
			return 9999999;
		}
		return (int)value;
	}

	public static int SaturateToInt(long value)
	{
		if (value >= int.MaxValue)
		{
			return int.MaxValue;
		}
		if (value <= int.MinValue)
		{
			return int.MinValue;
		}
		return (int)value;
	}

	public static int TruncateToInt(float value)
	{
		if (float.IsNaN(value))
		{
			return 0;
		}
		if (value >= 2.1474836E+09f)
		{
			return int.MaxValue;
		}
		if (value <= -2.1474836E+09f)
		{
			return int.MinValue;
		}
		return (int)value;
	}

	public static int CeilToIntWithUpperLimit(float value)
	{
		if (float.IsNaN(value))
		{
			return 0;
		}
		if (value >= 9999999f)
		{
			return 9999999;
		}
		if (value <= -2.1474836E+09f)
		{
			return int.MinValue;
		}
		return Mathf.CeilToInt(value);
	}
}
