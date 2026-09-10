using System.Collections.Generic;
using UnityEngine;

public class SteelGearCostEffect : Effect
{
	[SerializeField]
	private EnchantDef sturdyEnchant;

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			refEnchantDefs = new NodeDisplayInfo.Element<List<EnchantDef>>((sturdyEnchant == null) ? null : new List<EnchantDef> { sturdyEnchant })
		};
	}

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item item = eve?.TargetItem;
		BattleFieldTransform battleFieldTransform = eve?.CasterInfluence?.OwnerBft;
		if (item == null || battleFieldTransform == null || item.OwnerBft != battleFieldTransform || !item.DefenceLike)
		{
			return NodeCastResult.Failure;
		}
		if (eve.resolveInfo != null)
		{
			eve.resolveInfo.reject = true;
		}
		if (eve.triggerType == GameEvent.TriggerType.Real)
		{
			Effect.EnqueueAction(eve, new ChangeEnchantAction(item, eve.CasterItem, sturdyEnchant, 1));
			Effect.EnqueueAction(eve, new MoveToSlotAction(0, item, eve.CasterItem));
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode>();
	}
}
