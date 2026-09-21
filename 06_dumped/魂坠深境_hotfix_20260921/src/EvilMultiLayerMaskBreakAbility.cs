using System;
using System.Collections.Generic;

public sealed class EvilMultiLayerMaskBreakAbility : Ability
{
	public ItemSkinDef evilSkin;

	public static bool ShouldGenerateNextLayer(int originalCounter)
	{
		return originalCounter > 1;
	}

	public static int GetNextLayerCounter(int originalCounter)
	{
		return Math.Max(0, originalCounter - 1);
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.GenerateOverflowDamageCommandResolveInfo;
		yield return GameEvent.Type.BeforeBreakItem;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (!base.CheckTriggerGameEventAble(eve) || eve.CasterItem != eve.TargetItem || eve.TargetItem == null || eve.TargetItem.skin != evilSkin || eve.damage == null)
		{
			return false;
		}
		if (eve.type != GameEvent.Type.GenerateOverflowDamageCommandResolveInfo)
		{
			if (eve.type == GameEvent.Type.BeforeBreakItem)
			{
				return eve.damage.targetItem == eve.TargetItem;
			}
			return false;
		}
		if (eve.damage.targetItem == eve.TargetItem)
		{
			return eve.damage.IsWithDmaageOverflow;
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (!CheckTriggerGameEventAble(eve))
		{
			return;
		}
		if (eve.type == GameEvent.Type.GenerateOverflowDamageCommandResolveInfo)
		{
			eve.resolveInfo.reject = true;
			return;
		}
		Item target = eve.TargetItem;
		Damage damage = eve.damage;
		int originalCounter = eve.breakItemOriginalDefendCounter ?? target.GetCurrentDefendCounter();
		ItemContainer originContainer = target.owner;
		int originIndex = originContainer?.GetRawIndex(target) ?? (-1);
		object damageSource = eve.GetEveObj(GameEvent.EveObjType.Source);
		BattleFieldTransform ownerBft = target.OwnerBft;
		ProtectedInt overflowDamage = damage.overflowDamage;
		damage.breakChoicePending = true;
		eve.gameState.actionManager.EnqueueAction(new ExileItemAction(target, target));
		if (ShouldGenerateNextLayer(originalCounter) && originContainer != null)
		{
			Item item = target.Copy();
			int nextLayerCounter = GetNextLayerCounter(originalCounter);
			item.counter = nextLayerCounter;
			item.resetInitCounter = nextLayerCounter;
			item.blackboard?.SetValue(ItemBlackBoard.ValueType.InitCounter, nextLayerCounter);
			eve.gameState.actionManager.EnqueueAction(new SpawnItemAction(item, originContainer, withAnim: true, target, () => Math.Max(0, Math.Min(originIndex, originContainer.ItemsCount))));
		}
		eve.gameState.actionManager.EnqueueAction(delegate
		{
			eve.gameState.gameEventManager.TriggerAfterBreakItem(target, damageSource, damage, originalCounter);
		});
		if ((int)overflowDamage > 0 && ownerBft != null)
		{
			DamageCommand damageCommand = new DamageCommand
			{
				baseDamage = overflowDamage,
				lastDamageInfo = new DamageCommand.OverflowLastDamageInfo
				{
					lastDamageItem = target,
					damagedItems = new List<Item> { target }
				},
				source = damage.source,
				sourceOwnerOverride = damage.sourceOwnerOverride,
				targetBFT = ownerBft,
				overflow = true,
				pierce = damage.pierce,
				ignoreSourceDamageModifiers = damage.ignoreSourceDamageModifiers,
				spellCastInfo = damage.sourceSpellInfo
			};
			eve.gameState.actionManager.EnqueueAction(new DamageAction(damageCommand));
		}
	}
}
