public static class IGrowthValueExtend
{
	public static float GetFinalValue(this IGrowthValue v, float level)
	{
		return v.BaseValue + v.Derivative * level + v.SecondDerivative * level * level;
	}
}
