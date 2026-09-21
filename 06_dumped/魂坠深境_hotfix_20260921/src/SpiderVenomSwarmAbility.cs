using System.Collections.Generic;
using UnityEngine;

public class SpiderVenomSwarmAbility : Ability
{
	public EnchantDef venomEnchant;

	public int counterMultiplier = 2;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.ChangeEnchantResolve;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (!base.CheckTriggerGameEventAble(eve) || SpiderContractEnemyAbilityUtility.GetSpiderRule(eve?.gameState) == null)
		{
			return false;
		}
		Item eveObj = eve.GetEveObj<Item>(GameEvent.EveObjType.Destination);
		if (venomEnchant != null && eve.resolveInfo != null && eve.deltaValue > 0 && eve.TargetEnchant?.def == venomEnchant)
		{
			if (eveObj == null)
			{
				return false;
			}
			return eveObj.OwnerBft?.isPlayer == true;
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve))
		{
			eve.resolveInfo.deltaValue += GetExtraVenom(eve);
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int extraVenom = GetExtraVenom(eve);
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(extraVenom),
			str = new NodeDisplayInfo.Element<string>(SpiderContractEnemyAbilityUtility.FormatActivityValueWithLevel(extraVenom, eve?.gameState))
		};
	}

	private int GetExtraVenom(GameEvent eve)
	{
		return GetExtraVenomForDisplay(eve?.gameState);
	}

	public int GetExtraVenomForDisplay(GameStateInfo gameState)
	{
		int num = (gameState?.contractState?.selectedContract?.contractDef?.contractRule as SpiderContractRuleDef)?.GetNestActivityCurrent(gameState) ?? 0;
		return Mathf.FloorToInt(1f + Mathf.Pow(num, 1.32f));
	}
}
