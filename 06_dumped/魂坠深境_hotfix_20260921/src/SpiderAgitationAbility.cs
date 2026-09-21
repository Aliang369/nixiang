using System.Collections.Generic;

public class SpiderAgitationAbility : Ability, IEnemyIntentDisplayAbility
{
	public List<ItemDefinition> swarmItems;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.AfterSpellCast;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (base.CheckTriggerGameEventAble(eve) && SpiderContractEnemyAbilityUtility.IsSelfSpellCast(eve))
		{
			Item casterItem = eve.CasterItem;
			if (casterItem == null)
			{
				return false;
			}
			return casterItem.OwnerBft?.isEnemy == true;
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve))
		{
			SpiderContractEnemyAbilityUtility.GetSpiderRule(eve.gameState)?.ChangeNestActivity(eve.gameState, 1);
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(1),
			str = new NodeDisplayInfo.Element<string>(SpiderContractEnemyAbilityUtility.GetNestActivity(eve?.gameState).ToString())
		};
	}
}
