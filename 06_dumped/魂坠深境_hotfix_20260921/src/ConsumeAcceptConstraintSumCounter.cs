using System.Collections.Generic;
using UnityEngine;

public class ConsumeAcceptConstraintSumCounter : ValueGenerator
{
	[SerializeField]
	private ItemConstraint constraint;

	public override int? GenerateValue(GameEvent eve)
	{
		SpellCastInfo spellCastInfo = eve.spellInfo ?? eve.gameState?.playerState?.playerSpellInfo;
		if (spellCastInfo == null)
		{
			return null;
		}
		int num = 0;
		foreach (Item addedCost in spellCastInfo.addedCosts)
		{
			bool? flag = constraint.CheckItemAccept(addedCost);
			if (!flag.HasValue)
			{
				return null;
			}
			if (flag == true)
			{
				num += (int)addedCost.counter;
			}
		}
		return num;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { constraint };
	}
}
