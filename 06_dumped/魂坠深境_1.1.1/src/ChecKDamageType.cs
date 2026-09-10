using UnityEngine;

public class ChecKDamageType : CheckNode
{
	[SerializeField]
	private GameEvent.DealType dealType;

	[SerializeField]
	private bool mustWithSubCounter;

	[SerializeField]
	private AfterDamageCheckAbility.TargetItemTypeCondition targetType;

	public override bool? Check(GameEvent eve)
	{
		if (eve?.damage == null)
		{
			return null;
		}
		if (mustWithSubCounter && eve.damage.subCounter <= 0)
		{
			return false;
		}
		if (!eve.CheckDealType(dealType))
		{
			return false;
		}
		return targetType switch
		{
			AfterDamageCheckAbility.TargetItemTypeCondition.Any => true, 
			AfterDamageCheckAbility.TargetItemTypeCondition.Health => eve.damage.targetItem == null, 
			AfterDamageCheckAbility.TargetItemTypeCondition.Defence => eve.damage.targetItem != null && eve.damage.targetItem.Defence, 
			_ => false, 
		};
	}
}
