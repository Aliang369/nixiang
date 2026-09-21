using System.Collections.Generic;

public sealed class EvilBaitOverflowResolveAbility : Ability
{
	public ItemSkinDef evilSkin;

	public string enemyText = "敌方";

	public string friendlyText = "我方";

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.GenerateOverflowDamageCommandResolveInfo;
		yield return GameEvent.Type.BeforeBreakItem;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (base.CheckTriggerGameEventAble(eve) && eve.TargetItem != null && eve.TargetItem.skin == evilSkin && eve.damage != null && eve.damage.targetItem == eve.TargetItem && !eve.damage.breakChoiceStarted)
		{
			if (eve.type != GameEvent.Type.BeforeBreakItem)
			{
				return eve.damage.IsWithDmaageOverflow;
			}
			return true;
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (!CheckTriggerGameEventAble(eve))
		{
			return;
		}
		SelectChoiceInfo selectChoiceInfo = new SelectChoiceInfo
		{
			selectorBft = eve.TargetItem.OwnerBft,
			panelHint = "选择溢出伤害方向"
		};
		SelectChoiceInfo.SelectChoice selectChoice = new SelectChoiceInfo.SelectChoice
		{
			choiceText = enemyText,
			selectable = () => true
		};
		selectChoice.onSelect = delegate
		{
			ResolveChoice(eve, friendly: false);
		};
		SelectChoiceInfo.SelectChoice selectChoice2 = new SelectChoiceInfo.SelectChoice
		{
			choiceText = friendlyText,
			selectable = () => true
		};
		selectChoice2.onSelect = delegate
		{
			ResolveChoice(eve, friendly: true);
		};
		selectChoiceInfo.choices = new List<SelectChoiceInfo.SelectChoice> { selectChoice, selectChoice2 };
		eve.damage.breakChoiceStarted = true;
		if (eve.type == GameEvent.Type.GenerateOverflowDamageCommandResolveInfo)
		{
			eve.resolveInfo.reject = true;
		}
		else
		{
			eve.damage.breakChoicePending = true;
			eve.damage.breakChoiceContinuation = delegate
			{
				eve.gameState.gameEventManager.TriggerAfterBreakItem(eve.TargetItem, eve.GetEveObj(GameEvent.EveObjType.Source), eve.damage, eve.breakItemOriginalDefendCounter);
			};
		}
		eve.gameState.actionManager.EnqueueAction(new SelectChoiceAction(selectChoiceInfo));
	}

	private void ResolveChoice(GameEvent eve, bool friendly)
	{
		Damage damage = eve.damage;
		if (eve.type == GameEvent.Type.BeforeBreakItem)
		{
			eve.damage.breakChoicePending = false;
			if (!friendly)
			{
				eve.gameState.actionManager.EnqueueAction(new ConsumeItemAction(eve.TargetItem, ConsumeItemAction.ConsumeType.Damage));
			}
			eve.damage.breakChoiceContinuation?.Invoke();
			return;
		}
		if (friendly)
		{
			eve.resolveInfo.suppressBreak = true;
			EnqueueFriendlyOverflowDamage(eve, damage);
		}
		if ((int)damage.overflowDamage > 0)
		{
			DamageCommand damageCommand = new DamageCommand
			{
				baseDamage = damage.overflowDamage,
				lastDamageInfo = new DamageCommand.OverflowLastDamageInfo
				{
					lastDamageItem = damage.targetItem,
					damagedItems = new List<Item> { damage.targetItem }
				},
				source = damage.source,
				sourceOwnerOverride = damage.sourceOwnerOverride,
				targetBFT = ((damage.targetItem.OwnerBft == null) ? null : (friendly ? damage.targetItem.OwnerBft : eve.gameState.battleState.GetOpponent(damage.targetItem.OwnerBft))),
				overflow = false,
				spellCastInfo = damage.sourceSpellInfo,
				pierce = damage.pierce
			};
			eve.gameState.actionManager.EnqueueAction(new DamageAction(damageCommand));
		}
	}

	private void EnqueueFriendlyOverflowDamage(GameEvent eve, Damage damage)
	{
		if (damage != null && (int)damage.overflowDamage > 0 && eve.TargetItem?.OwnerBft != null)
		{
			DamageCommand damageCommand = new DamageCommand
			{
				baseDamage = damage.overflowDamage,
				source = damage.source,
				sourceOwnerOverride = damage.sourceOwnerOverride,
				targetBFT = damage.targetItem.OwnerBft,
				overflow = false,
				spellCastInfo = damage.sourceSpellInfo,
				pierce = damage.pierce
			};
			eve.gameState.actionManager.EnqueueAction(new DamageAction(damageCommand));
		}
	}
}
