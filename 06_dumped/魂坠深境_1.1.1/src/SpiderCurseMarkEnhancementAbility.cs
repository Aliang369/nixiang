using UnityEngine;

public class SpiderCurseMarkEnhancementAbility : GameEventTriggerAbility
{
	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (base.CheckTriggerGameEventAble(eve))
		{
			return SpiderContractEnemyAbilityUtility.GetSpiderRule(eve?.gameState) != null;
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve) && eve.resolveInfo != null)
		{
			eve.resolveInfo.deltaValue += GetExtraStacks(eve.gameState);
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int extraStacks = GetExtraStacks(eve?.gameState);
		return new NodeDisplayInfo
		{
			str = new NodeDisplayInfo.Element<string>(SpiderContractEnemyAbilityUtility.FormatActivityValueWithLevel(extraStacks, eve?.gameState))
		};
	}

	private static int GetExtraStacks(GameStateInfo gameState)
	{
		return GetExtraStacks(SpiderContractEnemyAbilityUtility.GetNestActivity(gameState));
	}

	private static int GetExtraStacks(int activity)
	{
		return Mathf.FloorToInt(1f + (float)Mathf.Max(0, activity) * 2f);
	}

	public int GetExtraStacksForDisplay(GameStateInfo gameState)
	{
		return GetExtraStacks(gameState);
	}
}
