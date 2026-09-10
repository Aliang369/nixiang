using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Damage
{
	public enum ResolveType
	{
		BFT = 1,
		Pierce,
		DamageValue,
		DamageTimes,
		RejectFinalDamage
	}

	[Serializable]
	public class ReplacementArmorConsumeInfo
	{
		[SaveField(null, null)]
		public Enchant enchant;

		[SaveField(null, null)]
		public int absorb;
	}

	[SaveField(null, null)]
	public int overflowDamage;

	[SaveField(null, null)]
	public int subCounter;

	[SaveField(null, null)]
	public bool allowOverflow;

	[SaveField(null, null)]
	public int baseDamage;

	[SaveField(null, null)]
	public int castDamage;

	[SaveField(null, null)]
	public int decreasedDamage;

	[SaveField(null, null)]
	public int finalDamage;

	[SaveField(null, null)]
	public int itemCounterAbsorb;

	[SaveField(null, null)]
	public int replacementArmorAbsorb;

	[SaveField(null, null)]
	public Item targetItem;

	[SaveField(null, null)]
	public List<ReplacementArmorConsumeInfo> replacementArmorConsumes;

	[NonSerialized]
	public Action breakChoiceContinuation;

	[NonSerialized]
	public bool breakChoicePending;

	[NonSerialized]
	public bool breakChoiceStarted;

	public bool pierce;

	public object source;

	public BattleFieldTransform sourceOwnerOverride;

	public Item lastDamageItem;

	public SpellCastInfo sourceSpellInfo;

	public BattleFieldTransform targetBFT;

	public BattleFieldTransform SourceOwnerBft
	{
		get
		{
			BattleFieldTransform ownerBft = sourceOwnerOverride;
			if (ownerBft == null)
			{
				IBftOwnable obj = source as IBftOwnable;
				if (obj == null)
				{
					return null;
				}
				ownerBft = obj.OwnerBft;
			}
			return ownerBft;
		}
	}

	public bool IsFromSpell => sourceSpellInfo != null;

	public bool IsRealDamageDealt => finalDamage > 0;

	public bool isOverflowDamage => lastDamageItem != null;

	public bool isHealthDamage
	{
		get
		{
			if (targetItem == null)
			{
				return targetBFT != null;
			}
			return false;
		}
	}

	public bool IsWithDmaageOverflow => overflowDamage > 0;

	private Damage()
	{
	}

	public static Damage GenerateDamage(DamageCommand damageCommand, GameStateInfo gameState, GameEvent.TriggerType triggerType)
	{
		GameEvent.ResolveInfo resolveInfo = new GameEvent.ResolveInfo();
		Item item = GetDamageDefendItem(gameState, damageCommand.targetBFT, damageCommand.pierce, damageCommand.lastDamageInfo);
		if (gameState?.gameEventManager != null)
		{
			resolveInfo = gameState.gameEventManager.TriggerGenerateDamageResolveInfo(damageCommand, triggerType, ResolveType.BFT, item);
			if (resolveInfo.overrideBftTarget != null && resolveInfo.overrideBftTarget != damageCommand.targetBFT)
			{
				item = GetDamageDefendItem(gameState, resolveInfo.overrideBftTarget, damageCommand.pierce | resolveInfo.pierce, damageCommand.lastDamageInfo);
				damageCommand.targetBFT = resolveInfo.overrideBftTarget;
			}
			resolveInfo = gameState.gameEventManager.TriggerGenerateDamageResolveInfo(damageCommand, triggerType, ResolveType.Pierce, item);
			if (damageCommand.pierce | resolveInfo.pierce)
			{
				item = null;
			}
			resolveInfo = gameState.gameEventManager.TriggerGenerateDamageResolveInfo(damageCommand, triggerType, ResolveType.DamageValue, item);
		}
		Damage damage = new Damage();
		damage.baseDamage = damageCommand.baseDamage;
		damage.allowOverflow = damageCommand.overflow && !resolveInfo.noOverflow;
		if (item != null)
		{
			damage.targetItem = item;
		}
		else
		{
			damage.targetBFT = ((resolveInfo.overrideBftTarget != null) ? resolveInfo.overrideBftTarget : damageCommand.targetBFT);
		}
		damage.sourceSpellInfo = damageCommand.spellCastInfo;
		damage.sourceOwnerOverride = damageCommand.sourceOwnerOverride;
		damage.pierce = damageCommand.pierce | resolveInfo.pierce;
		damage.source = damageCommand.source;
		damage.lastDamageItem = damageCommand.lastDamageInfo?.lastDamageItem;
		damage.replacementArmorConsumes = new List<ReplacementArmorConsumeInfo>();
		if (damageCommand.ignoreSourceDamageModifiers)
		{
			resolveInfo.sourceDelta = 0;
			resolveInfo.sourceDeltaPercent = 0f;
		}
		damage.castDamage = RoundNumber((float)damage.baseDamage * ((100f + resolveInfo.sourceDeltaPercent) / 100f) + (float)resolveInfo.sourceDelta);
		if (resolveInfo.isFixedTargetValue)
		{
			damage.castDamage = CombatValueLimit.Clamp(resolveInfo.fixedTargetValue);
		}
		damage.finalDamage = GetFinalDamage(damage.castDamage, resolveInfo);
		damage.decreasedDamage = Mathf.Max(0, damage.finalDamage - damage.castDamage);
		if (gameState?.gameEventManager != null)
		{
			resolveInfo = gameState.gameEventManager.TriggerGenerateDamageResolveInfo(damageCommand, triggerType, ResolveType.RejectFinalDamage, item, damage);
		}
		if (resolveInfo.reject)
		{
			damage.finalDamage = 0;
		}
		BattleFieldTransform battleFieldTransform = damage.targetBFT;
		int num = damage.targetItem?.GetCurrentDefendCounter() ?? CombatValueLimit.Clamp((long)(int)(battleFieldTransform?.health ?? ((ProtectedInt)0)) + (long)(int)(battleFieldTransform?.tempHealth ?? ((ProtectedInt)0)));
		damage.subCounter = Mathf.Min(damage.finalDamage, num);
		if (damage.targetItem != null)
		{
			int num2 = damage.subCounter;
			List<Item.ReplacementArmorSource> replacementArmorSources = damage.targetItem.GetReplacementArmorSources();
			int b = CombatValueLimit.Clamp(((IEnumerable<Item.ReplacementArmorSource>)replacementArmorSources).Sum((Func<Item.ReplacementArmorSource, long>)((Item.ReplacementArmorSource source) => source.armor)));
			damage.replacementArmorAbsorb = Mathf.Min(num2, b);
			num2 -= damage.replacementArmorAbsorb;
			damage.itemCounterAbsorb = Mathf.Min(num2, Mathf.Max(0, damage.targetItem.counter));
			int num3 = damage.replacementArmorAbsorb;
			foreach (Item.ReplacementArmorSource item2 in replacementArmorSources)
			{
				if (num3 <= 0)
				{
					break;
				}
				int num4 = Mathf.Min(num3, item2.armor);
				num3 -= num4;
				damage.replacementArmorConsumes.Add(new ReplacementArmorConsumeInfo
				{
					enchant = item2.enchant,
					absorb = num4
				});
			}
		}
		damage.overflowDamage = ((damage.finalDamage > num) ? (damage.castDamage - (damage.subCounter + damage.decreasedDamage)) : 0);
		if (resolveInfo.reject || damage.targetItem == null)
		{
			damage.overflowDamage = 0;
		}
		return damage;
	}

	private static int GetFinalDamage(int castDamage, GameEvent.ResolveInfo resolveInfo)
	{
		int a = RoundNumber((float)castDamage * ((100f + (float)resolveInfo.targetDeltaPercent) / 100f) + (float)resolveInfo.targetDelta);
		a = Mathf.Min(a, resolveInfo.upperBoundary);
		a = Mathf.Max(a, resolveInfo.lowerBoundary);
		if (resolveInfo.reject)
		{
			return 0;
		}
		return CombatValueLimit.Clamp(a);
	}

	private static int RoundNumber(float f)
	{
		if (float.IsNaN(f) || f <= 0f)
		{
			return 0;
		}
		if (f > 0f && f <= 1f)
		{
			return 1;
		}
		if (f >= 9999999f)
		{
			return 9999999;
		}
		return Mathf.FloorToInt(f);
	}

	private static Item GetDamageDefendItem(GameStateInfo gs, BattleFieldTransform targetBFT, bool pierce, DamageCommand.OverflowLastDamageInfo overflowLastDamageInfo)
	{
		if ((targetBFT == null) | pierce)
		{
			return null;
		}
		foreach (Item allItem in targetBFT.tableContainer.GetAllItems(orderReverse: true))
		{
			if (IsValidDamageDefendItem(allItem))
			{
				return allItem;
			}
		}
		foreach (Item allItem2 in targetBFT.companionContainer.GetAllItems(orderReverse: true))
		{
			if (IsValidDamageDefendItem(allItem2))
			{
				return allItem2;
			}
		}
		return null;
		bool IsValidDamageDefendItem(Item item)
		{
			if (!item.DamageAble)
			{
				return false;
			}
			if (overflowLastDamageInfo != null && overflowLastDamageInfo.damagedItems.Contains(item))
			{
				return false;
			}
			if (gs.gameEventManager.TriggerItemDefendDamageAbleResolve(item).reject)
			{
				return false;
			}
			return true;
		}
	}
}
