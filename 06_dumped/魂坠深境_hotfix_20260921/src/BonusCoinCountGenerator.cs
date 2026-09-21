using UnityEngine;

public class BonusCoinCountGenerator : ValueGenerator
{
	[SerializeField]
	private int baseValue;

	public override int? GenerateValue(GameEvent eve)
	{
		if (eve.gameState?.globalVariableStateInfo == null)
		{
			return null;
		}
		return baseValue;
	}
}
