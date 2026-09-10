using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpiderAcidCorrosionAbility : Ability, ISpiderPerTargetPerformanceAbility, IEnemyIntentDisplayAbility
{
	public TransformNFXConfig transformVFXEffect;

	[Obsolete("Legacy field kept so old serialized assets do not break.")]
	public int damage = 1;

	public int baseRange = 2;

	public bool elite;

	[Obsolete("Legacy field kept so old serialized assets do not break.")]
	public int eliteRangeIncreasePerCast = 1;

	public ItemDefinition temporaryArmor;

	[Obsolete("Legacy field kept so old serialized assets do not break.")]
	public float fallbackDamageMultiplier;

	[Obsolete("Legacy field kept so old serialized assets do not break.")]
	public EnchantDef temporaryArmorEnchant;

	[Obsolete("Legacy field kept so old serialized assets do not break.")]
	public EnchantDef rustEnchant;

	[Obsolete("Legacy field kept so old serialized assets do not break.")]
	public int rustStacks = 1;

	private ItemDefinition TemporaryArmor => temporaryArmor;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.AfterSpellCast;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (!base.CheckTriggerGameEventAble(eve))
		{
			return false;
		}
		if (SpiderContractEnemyAbilityUtility.IsSelfSpellCast(eve))
		{
			return TemporaryArmor != null;
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (!CheckTriggerGameEventAble(eve))
		{
			return;
		}
		ItemDefinition itemDefinition = TemporaryArmor;
		foreach (Item acidTarget in GetAcidTargets(eve))
		{
			ProtectedInt counter = acidTarget.counter;
			SpiderContractEnemyAbilityUtility.EnqueueAction2(eve);
			transformVFXEffect?.Play(eve, eve.CasterPivot, acidTarget);
			eve.gameState.actionManager.EnqueueAction(new ConvertItemToDefinitionAction(acidTarget, itemDefinition));
			eve.gameState.actionManager.EnqueueAction(new SetCounterAction(counter, acidTarget, eve.CasterItem));
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int finalRange = SpiderAcidPhase2RangeAbility.GetFinalRange(eve, this);
		return new NodeDisplayInfo
		{
			baseValue = new NodeDisplayInfo.Element<int?>(baseRange),
			deltaValue = new NodeDisplayInfo.Element<int?>(finalRange - baseRange),
			value = new NodeDisplayInfo.Element<int?>(finalRange)
		};
	}

	private List<Item> GetAcidTargets(GameEvent eve)
	{
		List<Item> allItems = eve.gameState.playerState.TableContainer.GetAllItems();
		if (allItems.Count == 0)
		{
			return new List<Item>();
		}
		int num = baseRange;
		List<Item> candidates = allItems.Skip(Mathf.Max(0, allItems.Count - num)).ToList();
		if (elite)
		{
			candidates.AddRange(from item in allItems.Take(num)
				where !candidates.Contains(item)
				select item);
		}
		return allItems.Where((Item item) => candidates.Contains(item) && IsConvertibleArmor(item)).ToList();
	}

	private bool IsConvertibleArmor(Item item)
	{
		if (item != null && item.OwnerBft?.isPlayer == true)
		{
			ItemContainer owner = item.owner;
			if (owner != null && owner.type == ItemContainer.ContainerType.Table && item.Defence && (int)item.counter > 0)
			{
				return item.GetDef() != TemporaryArmor;
			}
		}
		return false;
	}
}
