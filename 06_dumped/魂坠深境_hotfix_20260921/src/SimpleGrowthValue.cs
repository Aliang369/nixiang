using System;

[Serializable]
public class SimpleGrowthValue : IGrowthValue
{
	public float baseValue;

	public float derivative;

	public float secondDerivative;

	public float BaseValue => baseValue;

	public float Derivative => derivative;

	public float SecondDerivative => secondDerivative;

	public static SimpleGrowthValue operator *(SimpleGrowthValue a, float b)
	{
		return new SimpleGrowthValue
		{
			baseValue = a.baseValue * b,
			derivative = a.derivative * b,
			secondDerivative = a.secondDerivative * b
		};
	}
}
