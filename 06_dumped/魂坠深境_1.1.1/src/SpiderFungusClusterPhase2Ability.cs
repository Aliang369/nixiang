using System;
using System.Collections.Generic;
using System.Linq;

public class SpiderFungusClusterPhase2Ability : Ability, IEnemyIntentDisplayAbility
{
	public List<ItemDefinition> sporeCurseItems;

	[NonSerialized]
	private HashSet<BattleFieldTransform> triggeredEnemies;

	private HashSet<BattleFieldTransform> TriggeredEnemies => triggeredEnemies ?? (triggeredEnemies = new HashSet<BattleFieldTransform>());

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.EnemyEnterPhase2;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		BattleFieldTransform battleFieldTransform = eve?.CasterOwner;
		if (base.CheckTriggerGameEventAble(eve) && battleFieldTransform != null && battleFieldTransform.isEnemy && eve.TargetBft == battleFieldTransform)
		{
			CharacterAI characterAI = battleFieldTransform.characterAI;
			if (characterAI != null && characterAI.inPhase2 && battleFieldTransform.characterAI.SecondPhaseAbilityEnable)
			{
				return !TriggeredEnemies.Contains(battleFieldTransform);
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
		TriggeredEnemies.Add(casterOwner);
		BattleStateInfo battleState = eve.gameState.battleState;
		foreach (Item item in (from item in new ItemContainer[3] { battleState.PlayerTableContainer, battleState.PlayerDrawPileContainer, battleState.PlayerWastePileContainer }.Where((ItemContainer container) => container != null).SelectMany((ItemContainer container) => container.GetAllItems())
			where sporeCurseItems?.Contains(item?.def) ?? false
			select item).Distinct().ToList())
		{
			eve.gameState.actionManager.EnqueueAction(new ConsumeItemAction(item, ConsumeItemAction.ConsumeType.Effect));
			SpiderContractEnemyAbilityUtility.GetSpiderRule(eve.gameState)?.ChangeNestActivity(eve.gameState, 1);
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			str = new NodeDisplayInfo.Element<string>(SpiderContractEnemyAbilityUtility.FormatActivityLevelSuffix(SpiderContractEnemyAbilityUtility.GetNestActivity(eve?.gameState)))
		};
	}
}
