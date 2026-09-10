using System.Collections.Generic;
using System.Linq;
using ContainerOfSoul.TargetRequirement;
using GameRandom;
using UnityEngine;

public class ItemRequirement
{
	public enum TargetType
	{
		None = 0,
		OpponentTable = 3,
		OwnerTable = 4,
		BothTable = 5
	}

	public CheckNode castCheckNode;

	public int costCount;

	public CheckNode targetCheckNode;

	public CastPrepare targetRequirement;

	public ItemTargetSelector targetSelector;

	public bool targetSelfAble;

	public TargetType targetType;

	public bool NeedTarget => targetType != TargetType.None;

	public static GameEvent GenerateTargetCheckEve(SpellCastInfo castInfo, Item item)
	{
		return new GameEvent(GameEvent.Type.SetSpellTargetCheck, GameEvent.TriggerType.Real)
		{
			CasterItem = castInfo.spellItem,
			TargetItem = item,
			SourceItem = castInfo.spellItem,
			spellInfo = castInfo,
			gameState = castInfo.gameState
		};
	}

	public bool CheckTargetAccept(SpellCastInfo castInfo, Item item)
	{
		if (NeedTarget && (targetCheckNode == null || targetCheckNode.Check(GenerateTargetCheckEve(castInfo, item)) == true))
		{
			if (targetType == TargetType.None)
			{
				goto IL_00d0;
			}
			if (targetType == TargetType.OwnerTable)
			{
				ItemContainer owner = item.owner;
				if (owner != null && owner.type == ItemContainer.ContainerType.Table && item.OwnerBft == castInfo.spellItem.OwnerBft)
				{
					goto IL_00d0;
				}
			}
			if (targetType == TargetType.OpponentTable)
			{
				ItemContainer owner2 = item.owner;
				if (owner2 != null && owner2.type == ItemContainer.ContainerType.Table && item.OwnerBft != castInfo.spellItem.OwnerBft)
				{
					goto IL_00d0;
				}
			}
			if (targetType == TargetType.BothTable)
			{
				ItemContainer owner3 = item.owner;
				if (owner3 != null && owner3.type == ItemContainer.ContainerType.Table)
				{
					goto IL_00d0;
				}
			}
		}
		return false;
		IL_00d0:
		if (!targetSelfAble)
		{
			return item != castInfo.spellItem;
		}
		return true;
	}

	public bool CheckCastCheckNodeAccept(SpellCastInfo castInfo)
	{
		GameEvent gameEvent = new GameEvent(GameEvent.Type.SpellCastNodeCheck, GameEvent.TriggerType.Real);
		gameEvent.CasterItem = castInfo.spellItem;
		gameEvent.SourceItem = castInfo.spellItem;
		gameEvent.spellInfo = castInfo;
		gameEvent.gameState = castInfo.gameState;
		if (castCheckNode != null)
		{
			return castCheckNode.Check(gameEvent) == true;
		}
		return true;
	}

	public Item GetDefaultTargetItem(Item sourceItem, GameStateInfo gameState)
	{
		if (targetSelector != null)
		{
			GameEvent eve = Effect.GenerateDisplayEvent(gameState, sourceItem);
			return targetSelector.GetSingleItem(eve);
		}
		return GetTargetableItems(sourceItem, gameState).LastOrDefault();
	}

	public Item GetRandomTargetItem(Item sourceItem, GameStateInfo gameState)
	{
		if (targetSelector != null)
		{
			GameEvent eve = Effect.GenerateDisplayEvent(gameState, sourceItem);
			return targetSelector.GetTargetItems(eve).SelectRandom(gameState.randomManager.NextDouble);
		}
		return GetTargetableItems(sourceItem, gameState).SelectRandom(gameState.randomManager.NextDouble);
	}

	public List<Item> GetTargetableItems(Item sourceItem, GameStateInfo gameState)
	{
		IEnumerable<Item> allBattleFieldItems = gameState.battleState.GetAllBattleFieldItems();
		DisplaySpellCastInfo displayCastInfo = new DisplaySpellCastInfo(gameState)
		{
			spellItem = sourceItem
		};
		return allBattleFieldItems.Where((Item it) => CheckTargetAccept(displayCastInfo, it)).ToList();
	}

	public int GetFinalCostValue(SpellCastInfo castInfo, GameEvent.TriggerType triggerType)
	{
		int num = costCount;
		if (castInfo?.spellItem != null)
		{
			num += (int)castInfo.spellItem.deltaCost;
		}
		if (castInfo?.gameState?.gameEventManager != null)
		{
			GameEvent.ResolveInfo resolveInfo = castInfo.gameState.gameEventManager.TriggerGenerateSpellCastCostResolveInfo(castInfo, triggerType);
			num += resolveInfo.deltaValue;
		}
		return Mathf.Max(0, num);
	}

	public int GetBaseCostValue(SpellCastInfo castInfo)
	{
		int num = costCount;
		if (castInfo?.spellItem != null)
		{
			num += (int)castInfo.spellItem.deltaCost;
		}
		return Mathf.Max(0, num);
	}
}
