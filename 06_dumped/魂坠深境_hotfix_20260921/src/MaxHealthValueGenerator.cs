using System.Collections.Generic;
using UnityEngine;

public class MaxHealthValueGenerator : ValueGenerator
{
	[SerializeField]
	private GameEvent.EventBFTType eventBFT;

	public override int? GenerateValue(GameEvent eve)
	{
		BattleFieldTransform battleFieldTransform = eve.GetEventBFT(eventBFT);
		if (battleFieldTransform == null)
		{
			return null;
		}
		return battleFieldTransform.maxHealth;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode>();
	}
}
