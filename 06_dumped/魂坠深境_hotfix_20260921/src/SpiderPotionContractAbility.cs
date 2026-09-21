using System.Collections.Generic;

public sealed class SpiderPotionContractAbility : Ability
{
	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.BeforeEnterNextStage;
		yield return GameEvent.Type.BattleStart;
		yield return GameEvent.Type.EnterLocation;
		yield return GameEvent.Type.ConsumeResetCounterResolve;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (eve?.gameState?.contractState?.selectedContract?.contractDef?.contractRule is SpiderContractRuleDef)
		{
			if (eve.type != GameEvent.Type.BeforeEnterNextStage && eve.type != GameEvent.Type.BattleStart && eve.type != GameEvent.Type.EnterLocation)
			{
				return eve.type == GameEvent.Type.ConsumeResetCounterResolve;
			}
			return true;
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (!CheckTriggerGameEventAble(eve))
		{
			return;
		}
		SpiderContractRuleDef spiderContractRuleDef = (SpiderContractRuleDef)eve.gameState.contractState.selectedContract.contractDef.contractRule;
		switch (eve.type)
		{
		case GameEvent.Type.BeforeEnterNextStage:
			TriggerStageUpgrade(eve.gameState, spiderContractRuleDef);
			break;
		case GameEvent.Type.BattleStart:
			spiderContractRuleDef.ResetPotionBattleUsage(eve.gameState, (eve.gameState.battleState?.enemy?.enemyEncounterType).GetValueOrDefault());
			break;
		case GameEvent.Type.EnterLocation:
			TriggerCampfireReplenish(eve, spiderContractRuleDef);
			break;
		case GameEvent.Type.ConsumeResetCounterResolve:
			if (spiderContractRuleDef.IsCurrentPotion(eve.gameState, eve.TargetItem))
			{
				eve.resolveInfo.reject = true;
			}
			break;
		}
	}

	private static void TriggerStageUpgrade(GameStateInfo gameState, SpiderContractRuleDef rule)
	{
		int stage = gameState.contractState.stage;
		if ((stage == 2 || stage == 3) && !(rule.potionUpgradeChoiceEvent == null) && rule.HasPotion(gameState))
		{
			gameState.actionManager.EnqueueAction(new EnumeratorAction(gameState.mainStateManager.HideCurrentRoomPanel));
			gameState.actionManager.EnqueueAction(new BlockingWorldEventAction(rule.potionUpgradeChoiceEvent, showUpgradeView: true));
		}
	}

	private static void TriggerCampfireReplenish(GameEvent eve, SpiderContractRuleDef rule)
	{
		PathLocation targetLocation = eve.TargetLocation;
		if (targetLocation != null && targetLocation.type == PathLocation.LocationType.HealFire && rule.HasPotionUpgrade(eve.gameState, SpiderContractRuleDef.PotionUpgrade.StepByStep))
		{
			rule.ReplenishPotion(eve.gameState, rule.GetCampfireReplenishAmount(eve.gameState));
		}
	}
}
