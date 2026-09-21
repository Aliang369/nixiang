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
	public ProtectedInt overflowDamage;

	[SaveField(null, null)]
	public ProtectedInt subCounter;

	[SaveField(null, null)]
	public bool allowOverflow;

	[SaveField(null, null)]
	public ProtectedInt baseDamage;

	[SaveField(null, null)]
	public ProtectedInt castDamage;

	[SaveField(null, null)]
	public ProtectedInt decreasedDamage;

	[SaveField(null, null)]
	public ProtectedInt finalDamage;

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

	public bool ignoreSourceDamageModifiers;

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

	public bool IsRealDamageDealt => (int)finalDamage > 0;

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

	public bool IsWithDmaageOverflow => (int)overflowDamage > 0;

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
		int num = damageCommand.baseDamage;
		damage.baseDamage = num;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
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
		damage.ignoreSourceDamageModifiers = damageCommand.ignoreSourceDamageModifiers;
		damage.source = damageCommand.source;
		damage.lastDamageItem = damageCommand.lastDamageInfo?.lastDamageItem;
		damage.replacementArmorConsumes = new List<ReplacementArmorConsumeInfo>();
		if (damageCommand.ignoreSourceDamageModifiers)
		{
			resolveInfo.sourceDelta = 0;
			resolveInfo.sourceDeltaPercent = 0f;
		}
		num2 = RoundNumber((float)num * ((100f + resolveInfo.sourceDeltaPercent) / 100f) + (float)resolveInfo.sourceDelta);
		if (resolveInfo.isFixedTargetValue)
		{
			num2 = CombatValueLimit.Clamp(resolveInfo.fixedTargetValue);
		}
		num3 = GetFinalDamage(num2, resolveInfo);
		num4 = Mathf.Max(0, num3 - num2);
		damage.castDamage = num2;
		damage.finalDamage = num3;
		damage.decreasedDamage = num4;
		if (gameState?.gameEventManager != null)
		{
			resolveInfo = gameState.gameEventManager.TriggerGenerateDamageResolveInfo(damageCommand, triggerType, ResolveType.RejectFinalDamage, item, damage);
		}
		if (resolveInfo.reject)
		{
			num3 = 0;
			damage.finalDamage = num3;
		}
		BattleFieldTransform battleFieldTransform = damage.targetBFT;
		int num6 = damage.targetItem?.GetCurrentDefendCounter() ?? CombatValueLimit.Clamp((long)(int)(battleFieldTransform?.health ?? ((ProtectedInt)0)) + (long)(int)(battleFieldTransform?.tempHealth ?? ((ProtectedInt)0)));
		num5 = Mathf.Min(num3, num6);
		if (damage.targetItem != null)
		{
			int num7 = num5;
			List<Item.ReplacementArmorSource> replacementArmorSources = damage.targetItem.GetReplacementArmorSources();
			int b = CombatValueLimit.Clamp(((IEnumerable<Item.ReplacementArmorSource>)replacementArmorSources).Sum((Func<Item.ReplacementArmorSource, long>)((Item.ReplacementArmorSource source) => source.armor)));
			damage.replacementArmorAbsorb = Mathf.Min(num7, b);
			num7 -= damage.replacementArmorAbsorb;
			damage.itemCounterAbsorb = Mathf.Min(num7, Mathf.Max(0, damage.targetItem.counter));
			int num8 = damage.replacementArmorAbsorb;
			foreach (Item.ReplacementArmorSource item2 in replacementArmorSources)
			{
				if (num8 <= 0)
				{
					break;
				}
				int num9 = Mathf.Min(num8, item2.armor);
				num8 -= num9;
				damage.replacementArmorConsumes.Add(new ReplacementArmorConsumeInfo
				{
					enchant = item2.enchant,
					absorb = num9
				});
			}
		}
		int num10 = ((num3 > num6) ? (num2 - (num5 + num4)) : 0);
		if (resolveInfo.reject || damage.targetItem == null)
		{
			num10 = 0;
		}
		damage.subCounter = num5;
		damage.overflowDamage = num10;
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
