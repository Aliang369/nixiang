using System.Collections.Generic;
using UnityEngine;

public class MissingHealthValueGenerator : ValueGenerator
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
		int num = battleFieldTransform.health;
		return (int)battleFieldTransform.maxHealth - num;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode>();
	}
}
