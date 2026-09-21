using System;
using System.Collections.Generic;
using UnityEngine;

public class SpiderStingingArmorAbility : Ability
{
	public EnchantDef venomEnchant;

	public int venomStacks = 1;

	public EnchantDef painEnchant;

	public int painStacks;

	public int elitePainStacks = 1;

	[NonSerialized]
	private HashSet<SpellCastInfo> enchantedSpellCasts;

	private HashSet<SpellCastInfo> EnchantedSpellCasts => enchantedSpellCasts ?? (enchantedSpellCasts = new HashSet<SpellCastInfo>());

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.AfterDamage;
		yield return GameEvent.Type.AfterSpellCast;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (!base.CheckTriggerGameEventAble(eve))
		{
			return false;
		}
		if (eve.type == GameEvent.Type.AfterSpellCast)
		{
			if (eve.spellInfo != null)
			{
				return EnchantedSpellCasts.Contains(eve.spellInfo);
			}
			return false;
		}
		SpellCastInfo spellCastInfo = eve.damage?.sourceSpellInfo;
		Item damageSourceItem = GetDamageSourceItem(eve);
		BattleFieldTransform battleFieldTransform = eve.damage?.targetItem?.OwnerBft ?? eve.damage?.targetBFT;
		if (eve.type == GameEvent.Type.AfterDamage)
		{
			Item casterItem = eve.CasterItem;
			if (casterItem != null && casterItem.OwnerBft?.isEnemy == true)
			{
				ItemContainer owner = eve.CasterItem.owner;
				if (owner != null && owner.type == ItemContainer.ContainerType.Table && (int)eve.CasterItem.counter > 0 && battleFieldTransform == eve.CasterItem.OwnerBft && damageSourceItem != null && damageSourceItem.OwnerBft?.isPlayer == true && spellCastInfo != null && spellCastInfo.casterType == SpellCastInfo.CasterType.Player && !spellCastInfo.isTriggerCast && eve.damage != null && (int)eve.damage.subCounter > 0 && !EnchantedSpellCasts.Contains(spellCastInfo) && venomEnchant != null)
				{
					return venomStacks > 0;
				}
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
		if (eve.type == GameEvent.Type.AfterSpellCast)
		{
			EnchantedSpellCasts.Remove(eve.spellInfo);
			return;
		}
		SpellCastInfo spellCastInfo = eve.damage?.sourceSpellInfo;
		Item damageSourceItem = GetDamageSourceItem(eve);
		if (damageSourceItem != null)
		{
			if (spellCastInfo != null)
			{
				EnchantedSpellCasts.Add(spellCastInfo);
			}
			SpiderContractEnemyAbilityUtility.EnqueueAction2(eve);
			eve.gameState.actionManager.EnqueueAction(new ChangeEnchantAction(damageSourceItem, eve.CasterItem, venomEnchant, venomStacks));
			int num = GetPainStacks(eve.CasterItem);
			if (painEnchant != null && num > 0)
			{
				eve.gameState.actionManager.EnqueueAction(new ChangeEnchantAction(damageSourceItem, eve.CasterItem, painEnchant, num));
			}
		}
	}

	private static Item GetDamageSourceItem(GameEvent eve)
	{
		object obj = eve?.damage?.source;
		object obj2 = obj as Item;
		if (obj2 == null)
		{
			obj2 = (obj as IAttachment)?.AttachTarget as Item;
			if (obj2 == null)
			{
				if (eve == null)
				{
					return null;
				}
				Damage damage = eve.damage;
				if (damage == null)
				{
					return null;
				}
				SpellCastInfo sourceSpellInfo = damage.sourceSpellInfo;
				if (sourceSpellInfo == null)
				{
					return null;
				}
				obj2 = sourceSpellInfo.spellItem;
			}
		}
		return (Item)obj2;
	}

	private int GetPainStacks(Item item)
	{
		if (painStacks > 0)
		{
			return painStacks;
		}
		if (item == null || item.OwnerBft?.characterAI?.enemyEncounterType != EnemyEncounterType.Elite)
		{
			return 0;
		}
		return Mathf.Max(0, elitePainStacks);
	}
}
