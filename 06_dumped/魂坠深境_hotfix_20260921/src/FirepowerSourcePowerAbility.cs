using System.Collections.Generic;
using UnityEngine;

public class FirepowerSourcePowerAbility : Ability
{
	[SerializeField]
	private EnchantDef sourcePowerEnchant;

	[SerializeField]
	private bool consumeTargetCostWhenTriggered = true;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.SpellCastEffectResolve;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (!CheckTriggerGameEventAble(eve))
		{
			return;
		}
		Item spellItem = eve.spellInfo.spellItem;
		Item targetItem = eve.TargetItem;
		if (spellItem != null && targetItem != null)
		{
			if (eve.resolveInfo != null)
			{
				eve.resolveInfo.reject = true;
			}
			if (eve.triggerType == GameEvent.TriggerType.Real)
			{
				eve.gameState.actionManager.EnqueueAction(new FirepowerSourcePowerAction(spellItem, targetItem, sourcePowerEnchant, consumeTargetCostWhenTriggered));
			}
		}
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (!base.CheckTriggerGameEventAble(eve))
		{
			return false;
		}
		Item item = eve.spellInfo?.spellItem;
		Enchant enchant = eve.spellInfo?.GetCastingOverrideSpellEnchant();
		ItemTag itemTag = ScriptableObjectSingleton<GameDataManager>.instance?.attackTag;
		if (sourcePowerEnchant != null && itemTag != null && item != null)
		{
			Item targetItem = eve.TargetItem;
			if (targetItem != null && targetItem.HasTag(itemTag) && eve.spellInfo != null && item == eve.CasterItem && item.IsCastingOverrideSpell(eve.spellInfo))
			{
				return enchant?.def == sourcePowerEnchant;
			}
		}
		return false;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { triggerCheckNode };
	}
}
