using System;
using System.Collections.Generic;
using System.Linq;

public class SpiderSpreadSwarmAbility : Ability
{
	[NonSerialized]
	private Dictionary<object, int> protectedOverflowCurseCounts;

	public int counterCost = 1;

	private Dictionary<object, int> ProtectedOverflowCurseCounts => protectedOverflowCurseCounts ?? (protectedOverflowCurseCounts = new Dictionary<object, int>());

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.ConsumeResolveInfo;
		yield return GameEvent.Type.AddItem;
		yield return GameEvent.Type.AfterTransform;
		yield return GameEvent.Type.AfterConsume;
		yield return GameEvent.Type.SlotIndexChange;
		yield return GameEvent.Type.PlayerTurnStart;
		yield return GameEvent.Type.BeforePlayerTurnEnd;
		yield return GameEvent.Type.PlayerTurnEnd;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		BattleFieldTransform battleFieldTransform = eve?.CasterOwner;
		if (!base.CheckTriggerGameEventAble(eve) || battleFieldTransform == null || !battleFieldTransform.isEnemy || SpiderContractEnemyAbilityUtility.GetSpiderRule(eve.gameState) == null)
		{
			return false;
		}
		if (eve.CasterItem != null)
		{
			ItemContainer owner = eve.CasterItem.owner;
			if (owner == null || owner.type != ItemContainer.ContainerType.Table)
			{
				return false;
			}
		}
		if (eve.type == GameEvent.Type.ConsumeResolveInfo)
		{
			if (eve.resolveInfo != null && eve.consumeType == ConsumeItemAction.ConsumeType.Overflow)
			{
				Item targetItem = eve.TargetItem;
				if (targetItem != null && targetItem.OwnerBft?.isPlayer == true)
				{
					ItemContainer owner2 = eve.TargetItem.owner;
					if (owner2 != null && owner2.type == ItemContainer.ContainerType.Table && SpiderContractEnemyAbilityUtility.IsCurse(eve.TargetItem))
					{
						return GetProtectedOverflowCurseCount(GetCounterOwner(eve)) < SpiderContractEnemyAbilityUtility.GetNestActivity(eve.gameState);
					}
				}
			}
			return false;
		}
		if (eve.type == GameEvent.Type.PlayerTurnStart || eve.type == GameEvent.Type.BeforePlayerTurnEnd)
		{
			return true;
		}
		List<Item> playerTableItems = GetPlayerTableItems(eve);
		if (eve.type == GameEvent.Type.PlayerTurnEnd && playerTableItems.Count > 0)
		{
			return playerTableItems.All(SpiderContractEnemyAbilityUtility.IsCurse);
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve))
		{
			object counterOwner = GetCounterOwner(eve);
			if (eve.type == GameEvent.Type.ConsumeResolveInfo)
			{
				IncreaseProtectedOverflowCurseCount(counterOwner);
				eve.resolveInfo.reject = true;
				return;
			}
			if (eve.type == GameEvent.Type.PlayerTurnStart || eve.type == GameEvent.Type.BeforePlayerTurnEnd)
			{
				ProtectedOverflowCurseCounts.Remove(counterOwner);
				return;
			}
			BattleFieldTransform bft = eve.gameState.playerState.bft;
			eve.gameState.actionManager.EnqueueAction(new DamageAction(new DamageCommand
			{
				baseDamage = (int)bft.health + (int)bft.tempHealth,
				source = eve.Caster,
				targetBFT = bft,
				pierce = true
			}));
			ProtectedOverflowCurseCounts.Remove(counterOwner);
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int nestActivity = SpiderContractEnemyAbilityUtility.GetNestActivity(eve?.gameState);
		return new NodeDisplayInfo
		{
			str = new NodeDisplayInfo.Element<string>(SpiderContractEnemyAbilityUtility.FormatActivityValueWithLevel(nestActivity, eve?.gameState))
		};
	}

	private object GetCounterOwner(GameEvent eve)
	{
		if (eve?.CasterItem == null)
		{
			return eve?.CasterOwner;
		}
		return eve.CasterItem;
	}

	private int GetProtectedOverflowCurseCount(object counterOwner)
	{
		if (counterOwner == null || !ProtectedOverflowCurseCounts.TryGetValue(counterOwner, out var value))
		{
			return 0;
		}
		return value;
	}

	private void IncreaseProtectedOverflowCurseCount(object counterOwner)
	{
		if (counterOwner != null)
		{
			ProtectedOverflowCurseCounts[counterOwner] = GetProtectedOverflowCurseCount(counterOwner) + 1;
		}
	}

	private static List<Item> GetPlayerTableItems(GameEvent eve)
	{
		return eve.gameState.playerState.TableContainer.GetAllItems().ToList();
	}
}
