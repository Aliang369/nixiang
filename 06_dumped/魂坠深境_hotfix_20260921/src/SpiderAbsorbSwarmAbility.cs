using System.Collections.Generic;

public class SpiderAbsorbSwarmAbility : Ability
{
	public int blockedCounterMultiplier = 1;

	public bool includeSelfCounter;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield break;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int infiltrationRangeIncrease = SpiderContractEnemyAbilityUtility.GetInfiltrationRangeIncrease(eve?.gameState);
		return new NodeDisplayInfo
		{
			str = new NodeDisplayInfo.Element<string>(SpiderContractEnemyAbilityUtility.FormatActivityValueWithLevel(infiltrationRangeIncrease, eve?.gameState))
		};
	}
}
