using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpiderAcidPhase2RangeAbility : Ability
{
	public ItemDefinition acidItem;

	public static int GetFinalRange(GameEvent eve, SpiderAcidCorrosionAbility acid)
	{
		if (acid == null)
		{
			return 0;
		}
		return acid.baseRange + GetFinalExtraRange(eve);
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.AfterSpellCast;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (base.CheckTriggerGameEventAble(eve))
		{
			BattleFieldTransform casterOwner = eve.CasterOwner;
			if (casterOwner != null && casterOwner.isEnemy && eve.SourceItem?.OwnerBft == eve.CasterOwner)
			{
				Item sourceItem = eve.SourceItem;
				if (sourceItem == null)
				{
					return false;
				}
				return sourceItem.def?.abilities?.OfType<SpiderAcidCorrosionAbility>().Any() == true;
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
		SpiderAcidCorrosionAbility spiderAcidCorrosionAbility = eve.SourceItem.def.abilities.OfType<SpiderAcidCorrosionAbility>().First();
		int finalExtraRange = GetFinalExtraRange(eve);
		if (finalExtraRange <= 0)
		{
			return;
		}
		List<Item> allItems = eve.gameState.playerState.TableContainer.GetAllItems();
		List<Item> baseTargets = GetTargets(allItems, spiderAcidCorrosionAbility.baseRange, spiderAcidCorrosionAbility.elite, spiderAcidCorrosionAbility.temporaryArmor);
		foreach (Item item in from item in GetTargets(allItems, spiderAcidCorrosionAbility.baseRange + finalExtraRange, spiderAcidCorrosionAbility.elite, spiderAcidCorrosionAbility.temporaryArmor)
			where !baseTargets.Contains(item)
			select item)
		{
			ProtectedInt counter = item.counter;
			spiderAcidCorrosionAbility.transformVFXEffect?.Play(eve, eve.SourceItem, item);
			eve.gameState.actionManager.EnqueueAction(new ConvertItemToDefinitionAction(item, spiderAcidCorrosionAbility.temporaryArmor));
			eve.gameState.actionManager.EnqueueAction(new SetCounterAction(counter, item, eve.SourceItem));
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int infiltrationRangeIncrease = SpiderContractEnemyAbilityUtility.GetInfiltrationRangeIncrease(eve?.gameState);
		return new NodeDisplayInfo
		{
			refItemDefs = new NodeDisplayInfo.Element<List<ItemDefinition>>((acidItem == null) ? new List<ItemDefinition>() : new List<ItemDefinition> { acidItem }),
			str = new NodeDisplayInfo.Element<string>(SpiderContractEnemyAbilityUtility.FormatActivityValueWithLevel(infiltrationRangeIncrease, eve?.gameState))
		};
	}

	private static int GetFinalExtraRange(GameEvent eve)
	{
		CharacterAI characterAI = (eve?.CasterOwner ?? eve?.CasterItem?.OwnerBft)?.characterAI;
		if (characterAI != null && characterAI.inPhase2 && characterAI.SecondPhaseAbilityEnable)
		{
			CharacterAIComponent component = characterAI.component;
			if ((object)component != null && component.secondPhaseAbilities?.OfType<SpiderAcidPhase2RangeAbility>().Any() == true)
			{
				return SpiderContractEnemyAbilityUtility.GetInfiltrationRangeIncrease(eve.gameState);
			}
		}
		return 0;
	}

	private static List<Item> GetTargets(IEnumerable<Item> queue, int range, bool elite, ItemDefinition temporaryArmor)
	{
		List<Item> list = queue.ToList();
		List<Item> candidates = list.Skip(Mathf.Max(0, list.Count - range)).ToList();
		if (elite)
		{
			candidates.AddRange(from item in list.Take(range)
				where !candidates.Contains(item)
				select item);
		}
		return list.Where(delegate(Item item)
		{
			if (candidates.Contains(item) && item != null && item.OwnerBft?.isPlayer == true)
			{
				ItemContainer owner = item.owner;
				if (owner != null && owner.type == ItemContainer.ContainerType.Table && item.Defence && (int)item.counter > 0)
				{
					return item.GetDef() != temporaryArmor;
				}
			}
			return false;
		}).ToList();
	}
}
