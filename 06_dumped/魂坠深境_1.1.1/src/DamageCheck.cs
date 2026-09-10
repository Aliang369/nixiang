using System;
using UnityEngine;

public class DamageCheck
{
	[SerializeField]
	private bool sourceConstrain;

	[SerializeField]
	private GameEvent.EventBFTType source;

	[SerializeField]
	private bool targetConstrain;

	[SerializeField]
	private GameEvent.EventBFTType target;

	[SerializeField]
	private bool mustWithSubCounter;

	[SerializeField]
	private AfterDamageCheckAbility.TargetItemTypeCondition targetType;

	public bool? Check(GameEvent eve, Damage damage)
	{
		if (sourceConstrain)
		{
			BattleFieldTransform eventBFT = eve.GetEventBFT(source);
			if (eventBFT == null)
			{
				return null;
			}
			if (damage.SourceOwnerBft != eventBFT)
			{
				return false;
			}
		}
		if (targetConstrain)
		{
			BattleFieldTransform eventBFT2 = eve.GetEventBFT(target);
			if (eventBFT2 == null)
			{
				return null;
			}
			if (damage.targetItem?.OwnerBft != eventBFT2)
			{
				return false;
			}
		}
		if (mustWithSubCounter && damage.subCounter <= 0)
		{
			return false;
		}
		return targetType switch
		{
			AfterDamageCheckAbility.TargetItemTypeCondition.Any => true, 
			AfterDamageCheckAbility.TargetItemTypeCondition.Health => damage.targetItem == null, 
			AfterDamageCheckAbility.TargetItemTypeCondition.Defence => damage.targetItem != null && damage.targetItem.Defence, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
