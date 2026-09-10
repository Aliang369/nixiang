using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SpiderContractEnemyAbilityUtility
{
	public static SpiderContractRuleDef GetSpiderRule(GameStateInfo gameState)
	{
		return gameState?.contractState?.selectedContract?.contractDef?.contractRule as SpiderContractRuleDef;
	}

	public static bool SuppressInitialIntentPerformance(Item item)
	{
		if (item == null)
		{
			return false;
		}
		return item.def?.abilities?.Any((Ability ability) => ability is ISpiderPerTargetPerformanceAbility) == true;
	}

	public static void EnqueueAction2(GameEvent eve)
	{
		Item item = eve?.CasterItem;
		if (item != null && item.OwnerBft?.isEnemy == true)
		{
			eve.gameState.actionManager.EnqueueAction(new PlayPerformanceAction(item, item.OwnerBft, EnemyAnimationType.none, waitForCompletion: true));
		}
	}

	public static int GetNestActivity(GameStateInfo gameState)
	{
		return GetSpiderRule(gameState)?.GetNestActivityCurrent(gameState) ?? 0;
	}

	public static int GetNestActivityMaximum(GameStateInfo gameState)
	{
		return GetSpiderRule(gameState)?.GetNestActivityMaximum(gameState) ?? 0;
	}

	public static int GetActivityUseLimit(GameStateInfo gameState)
	{
		return Mathf.Max(0, GetNestActivity(gameState));
	}

	public static int GetActivityUseLimit(GameStateInfo gameState, int activity)
	{
		return Mathf.Max(0, activity);
	}

	public static int GetInfiltrationRangeIncrease(GameStateInfo gameState)
	{
		return GetInfiltrationRangeIncrease(GetNestActivity(gameState));
	}

	public static int GetInfiltrationRangeIncrease(int activity)
	{
		return Mathf.Max(0, activity - 1);
	}

	public static string FormatActivityValueWithLevel(int value, GameStateInfo gameState)
	{
		return FormatActivityValueWithLevel(value, GetNestActivity(gameState));
	}

	public static string FormatActivityValueWithLevel(int value, int activity)
	{
		return TextDataPreset.ValueFormat.FormatWith(value) + "（活性Lv" + TextDataPreset.ValueFormat.FormatWith(activity) + "）";
	}

	public static string FormatActivityLevelSuffix(int activity)
	{
		return "（活性Lv" + TextDataPreset.ValueFormat.FormatWith(activity) + "）";
	}

	public static void SyncActivityUseLimit(Item item, GameStateInfo gameState)
	{
		if (item != null)
		{
			int activityUseLimit = GetActivityUseLimit(gameState);
			if ((int)item.maxCounter == 0)
			{
				item.counter = activityUseLimit;
			}
			item.maxCounter = activityUseLimit;
			item.resetInitMaxCounter = activityUseLimit;
			item.counter = Mathf.Clamp(item.counter, 0, activityUseLimit);
		}
	}

	public static void AddSpawnItemEnchant(Item item)
	{
		EnchantDef enchantDef = ScriptableObjectSingleton<GameDataManager>.instance?.spawnItemEnchantDef;
		if (item != null && !(enchantDef == null) && !item.Enchants.Any((Enchant enchant) => enchant?.def == enchantDef))
		{
			item.AddEnchant(new Enchant(enchantDef));
		}
	}

	public static bool IsSelfSpellCast(GameEvent eve)
	{
		if (eve != null && eve.type == GameEvent.Type.AfterSpellCast && eve.SourceItem != null && eve.CasterItem != null && eve.SourceItem == eve.CasterItem)
		{
			return eve.spellInfo?.spellItem == eve.CasterItem;
		}
		return false;
	}

	public static Item GetEnemyTableItemWithAbility<TAbility>(GameEvent eve) where TAbility : Ability
	{
		return eve?.CasterOwner?.tableContainer?.GetAllItems().FirstOrDefault((Item item) => item != null && item.def?.abilities?.OfType<TAbility>().Any() == true);
	}

	public static bool HasEnchant(Item item, EnchantDef enchantDef)
	{
		if (item == null)
		{
			return false;
		}
		return item.Enchants?.Any((Enchant enchant) => enchant?.def == enchantDef && (int)enchant.value > 0) == true;
	}

	public static bool HasAnyEnchant(Item item, List<EnchantDef> enchantDefs)
	{
		if (item?.Enchants == null || enchantDefs == null)
		{
			return false;
		}
		return item.Enchants.Any((Enchant enchant) => (int?)enchant?.value > 0 && enchantDefs.Contains(enchant.def));
	}

	public static bool IsCurse(Item item)
	{
		ItemTag itemTag = ScriptableObjectSingleton<GameDataManager>.instance?.curseTag;
		if (item != null && itemTag != null)
		{
			return item.HasTag(itemTag);
		}
		return false;
	}

	public static IEnumerable<Item> GetAdjacentItems(Item item)
	{
		ItemContainer itemContainer = item?.owner;
		if (itemContainer == null)
		{
			yield break;
		}
		int index = itemContainer.GetRawIndex(item);
		if (index >= 0)
		{
			List<Item> items = itemContainer.GetAllItems();
			if (index > 0)
			{
				yield return items[index - 1];
			}
			if (index + 1 < items.Count)
			{
				yield return items[index + 1];
			}
		}
	}

	public static bool IsSpiderSwarm(Item item, List<ItemDefinition> swarmItems)
	{
		if (item?.def == null)
		{
			return false;
		}
		if (swarmItems != null && swarmItems.Contains(item.def))
		{
			return true;
		}
		return item.ItemName?.StartsWith("蛛群·") ?? false;
	}
}
