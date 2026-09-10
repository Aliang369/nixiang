using System.Collections.Generic;

public sealed class SpiderInsecticideItemEffect : Effect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (!(eve?.gameState?.contractState?.selectedContract?.contractDef?.contractRule is SpiderContractRuleDef spiderContractRuleDef) || eve.TargetItem == null || eve.TargetItem.OwnerBft != eve.OpponentBFT)
		{
			return NodeCastResult.Failure;
		}
		Effect.EnqueueAction(eve, new ConsumeItemAction(eve.TargetItem, ConsumeItemAction.ConsumeType.Effect));
		spiderContractRuleDef.NotifyPotionUsed(eve.gameState);
		int potionTempHealth = spiderContractRuleDef.GetPotionTempHealth(eve.gameState);
		if (potionTempHealth > 0)
		{
			Effect.EnqueueAction(eve, new ChangeTempHealthAction(potionTempHealth, eve.CasterOwner));
		}
		int potionDamage = spiderContractRuleDef.GetPotionDamage(eve.gameState);
		if (potionDamage > 0)
		{
			Effect.EnqueueAction(eve, new DamageAction(new DamageCommand
			{
				baseDamage = potionDamage,
				source = eve.CasterItem,
				targetBFT = eve.OpponentBFT,
				spellCastInfo = eve.spellInfo,
				overflow = true
			}));
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode>();
	}
}
