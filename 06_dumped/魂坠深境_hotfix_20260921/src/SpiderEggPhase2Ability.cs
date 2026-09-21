using System;
using System.Collections.Generic;

public class SpiderEggPhase2Ability : Ability, IEnemyIntentDisplayAbility
{
	[NonSerialized]
	private Dictionary<BattleFieldTransform, int> turnCounts;

	[NonSerialized]
	private HashSet<BattleFieldTransform> initializedEnemies;

	private Dictionary<BattleFieldTransform, int> TurnCounts => turnCounts ?? (turnCounts = new Dictionary<BattleFieldTransform, int>());

	private HashSet<BattleFieldTransform> InitializedEnemies => initializedEnemies ?? (initializedEnemies = new HashSet<BattleFieldTransform>());

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.EnemyTurnStart;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		BattleFieldTransform battleFieldTransform = eve?.CasterOwner;
		if (base.CheckTriggerGameEventAble(eve) && battleFieldTransform != null && battleFieldTransform.isEnemy)
		{
			CharacterAI characterAI = battleFieldTransform.characterAI;
			if (characterAI != null && characterAI.inPhase2)
			{
				return battleFieldTransform.characterAI.SecondPhaseAbilityEnable;
			}
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (!CheckTriggerGameEventAble(eve))
		{
			return;
		}
		BattleFieldTransform casterOwner = eve.CasterOwner;
		if (InitializedEnemies.Add(casterOwner))
		{
			SpiderContractEnemyAbilityUtility.GetSpiderRule(eve.gameState)?.ChangeNestActivity(eve.gameState, 1);
			TurnCounts[casterOwner] = 0;
			return;
		}
		TurnCounts[casterOwner] = ((!TurnCounts.TryGetValue(casterOwner, out var value)) ? 1 : (value + 1));
		if (TurnCounts[casterOwner] >= 2)
		{
			TurnCounts[casterOwner] = 0;
			SpiderContractEnemyAbilityUtility.GetSpiderRule(eve.gameState)?.ChangeNestActivity(eve.gameState, 1);
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(SpiderContractEnemyAbilityUtility.GetNestActivity(eve?.gameState))
		};
	}
}
