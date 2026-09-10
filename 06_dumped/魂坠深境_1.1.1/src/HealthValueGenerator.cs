using System.Collections.Generic;
using UnityEngine;

public class HealthValueGenerator : ValueGenerator
{
	[SerializeField]
	private GameEvent.EventBFTType eventBFT;

	[SerializeField]
	private bool total;

	public override int? GenerateValue(GameEvent eve)
	{
		BattleFieldTransform battleFieldTransform = eve.GetEventBFT(eventBFT);
		if (battleFieldTransform == null)
		{
			return null;
		}
		if (total)
		{
			return battleFieldTransform.GetTotalHealth();
		}
		return battleFieldTransform.health;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode>();
	}
}
