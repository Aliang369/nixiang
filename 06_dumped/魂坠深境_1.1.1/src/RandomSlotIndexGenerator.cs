using System.Collections.Generic;
using UnityEngine;

public class RandomSlotIndexGenerator : ValueGenerator
{
	[SerializeField]
	private GameEvent.EventBFTType bType;

	[SerializeField]
	private GameEvent.ContainerType cType;

	[SerializeField]
	private ValueGenerator excludeValue;

	public override int? GenerateValue(GameEvent eve)
	{
		ItemContainer ownerContainer = eve.GetOwnerContainer(bType, cType);
		if (ownerContainer == null)
		{
			return null;
		}
		GameRandomManager randomManager = eve.gameState.randomManager;
		if (randomManager == null)
		{
			return null;
		}
		if (excludeValue == null)
		{
			return randomManager.Next() % (ownerContainer.ItemsCount + 1);
		}
		int num = randomManager.Next(0, ownerContainer.ItemsCount + 1);
		return (num >= excludeValue.GenerateValue(eve)) ? (num + 1) : num;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { excludeValue };
	}
}
