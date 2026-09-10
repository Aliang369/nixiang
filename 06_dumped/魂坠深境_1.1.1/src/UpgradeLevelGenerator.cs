using UnityEngine;

public class UpgradeLevelGenerator : ValueGenerator
{
	[SerializeField]
	private ItemTargetSelector targetSelector;

	public override int? GenerateValue(GameEvent eve)
	{
		return targetSelector.GetSingleItem(eve)?.upgradeLevel;
	}
}
