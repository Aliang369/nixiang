using System;

public class DealFireControllerFusionFireDamage : FireControllerAttackEffect
{
	public int baseDamage = 8;

	public int upgradedBaseDamage = 12;

	public bool exileAfterCast = true;

	public HitNFXConfig hitEffect;

	public HitNFX3DConfig hitEffect3D;

	public TransformNFXConfig transformVFXEffect;

	public TransformNFX3DConfig transformEffect3D;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		int damagePerCost = GetDamagePerCost(eve);
		int totalBaseDamage = GetTotalBaseDamage(eve, damagePerCost);
		FireControllerExileEffect.DealDamageToOpponent(eve, new DamageCommand
		{
			baseDamage = totalBaseDamage,
			source = eve.CasterItem,
			targetBFT = eve.OpponentBFT,
			overflow = true,
			spellCastInfo = eve.spellInfo,
			transformSetup = transformVFXEffect,
			transform3DSetup = transformEffect3D,
			hitSetup = hitEffect,
			hit3DSetup = hitEffect3D
		});
		if (exileAfterCast && eve.CasterItem != null)
		{
			SpellCastInfo spellInfo = eve.spellInfo;
			if (spellInfo == null || !spellInfo.suppressRemoveAfterCast)
			{
				SpellCastInfo spellInfo2 = eve.spellInfo;
				if (spellInfo2 == null || !spellInfo2.triggerFromProtectedExile)
				{
					eve.CasterItem.SetRemoveAfterCast();
				}
			}
		}
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int damagePerCost = GetDamagePerCost(eve);
		NodeDisplayInfo nodeDisplayInfo = FireControllerExileEffect.DamageDisplay(eve, GetTotalBaseDamage(eve, damagePerCost));
		nodeDisplayInfo.value = new NodeDisplayInfo.Element<int?>(damagePerCost);
		return nodeDisplayInfo;
	}

	private int GetDamagePerCost(GameEvent eve)
	{
		return FireControllerExileEffect.GetTierValue(eve, FireControllerExileEffect.UseConfiguredOrDefault(baseDamage, 8), FireControllerExileEffect.UseConfiguredOrDefault(upgradedBaseDamage, 12));
	}

	private static int GetTotalBaseDamage(GameEvent eve, int damagePerCost)
	{
		long num = (long)Math.Max(0, eve.spellInfo?.addCostCount ?? 0) + 1L;
		return CombatValueLimit.SaturateToInt(damagePerCost * num);
	}
}
