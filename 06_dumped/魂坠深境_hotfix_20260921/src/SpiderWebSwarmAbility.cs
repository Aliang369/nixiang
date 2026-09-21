using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;

public class SpiderWebSwarmAbility : Ability
{
	public List<EnchantDef> webbedEnchants;

	[NonSerialized]
	private Dictionary<BattleFieldTransform, int> triggerCounts;

	private Dictionary<BattleFieldTransform, int> TriggerCounts => triggerCounts ?? (triggerCounts = new Dictionary<BattleFieldTransform, int>());

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.EnemyTurnStart;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (base.CheckTriggerGameEventAble(eve))
		{
			BattleFieldTransform casterOwner = eve.CasterOwner;
			if (casterOwner != null && casterOwner.isEnemy && GetTriggerCount(eve.CasterOwner) < SpiderContractEnemyAbilityUtility.GetActivityUseLimit(eve.gameState))
			{
				return GetWebbedPlayerItems(eve).Count > 0;
			}
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve))
		{
			Item item = GetWebbedPlayerItems(eve).SelectRandom(eve.gameState.randomManager.NextDouble);
			if (item != null)
			{
				SpiderContractEnemyAbilityUtility.AddSpawnItemEnchant(item);
				TriggerCounts[eve.CasterOwner] = GetTriggerCount(eve.CasterOwner) + 1;
				eve.gameState.actionManager.EnqueueAction(new TransferItemAction(item, eve.CasterOwner.tableContainer, null, TransferItemAction.TransferAnimType.Transfer));
			}
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			str = new NodeDisplayInfo.Element<string>(SpiderContractEnemyAbilityUtility.FormatActivityValueWithLevel(SpiderContractEnemyAbilityUtility.GetActivityUseLimit(eve?.gameState), eve?.gameState))
		};
	}

	private List<Item> GetWebbedPlayerItems(GameEvent eve)
	{
		return eve.gameState?.playerState?.TableContainer?.GetAllItems().Where((Item item) => SpiderContractEnemyAbilityUtility.HasAnyEnchant(item, webbedEnchants)).ToList() ?? new List<Item>();
	}

	private int GetTriggerCount(BattleFieldTransform enemy)
	{
		if (enemy == null || !TriggerCounts.TryGetValue(enemy, out var value))
		{
			return 0;
		}
		return value;
	}
}
