using UnityEngine;

public class PredefinedValueGenerator : ValueGenerator
{
	[SerializeField]
	public int value;

	public override int? GenerateValue(GameEvent eve)
	{
		return value;
	}
}
