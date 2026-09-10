using System.Collections.Generic;

public sealed class DancerCharmPlayedCardAbility : Ability
{
	private const string ProcessingKey = "DancerCharmPlayedCardAbility.Processing";

	private const string ActiveKey = "DancerCharmPlayedCardAbility.ActiveThisTurn";

	private const string RetainedKey = "DancerCharmPlayedCardAbility.RetainedAfterDurabilityClean";

	public Effect effect;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.AfterSpellCast;
		yield return GameEvent.Type.PlayerTurnEnd;
		yield return GameEvent.Type.BattleStart;
		yield return GameEvent.Type.BeforeBattleEnd;
		yield return GameEvent.Type.SkillDurabilityClean;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if ((!base.CheckTriggerGameEventAble(eve) && !IsRetained(eve.CasterSkill)) || !IsActive(eve.CasterSkill))
		{
			return false;
		}
		if (eve.type == GameEvent.Type.PlayerTurnEnd || eve.type == GameEvent.Type.BattleStart || eve.type == GameEvent.Type.BeforeBattleEnd || eve.type == GameEvent.Type.SkillDurabilityClean)
		{
			return true;
		}
		if (eve.type == GameEvent.Type.AfterSpellCast && (eve.CasterItem != null || eve.CasterSkill != null) && eve.spellInfo?.spellItem != null && eve.spellInfo.casterType == SpellCastInfo.CasterType.Player && !eve.spellInfo.isTriggerCast)
		{
			BattleFieldTransform ownerBft = eve.spellInfo.spellItem.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer)
			{
				return !IsProcessing(eve.CasterItem, eve.CasterSkill);
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
		if (eve.type == GameEvent.Type.PlayerTurnEnd || eve.type == GameEvent.Type.BattleStart || eve.type == GameEvent.Type.BeforeBattleEnd)
		{
			eve.CasterSkill.tempValues?.Remove("DancerCharmPlayedCardAbility.ActiveThisTurn");
			eve.CasterSkill.tempValues?.Remove("DancerCharmPlayedCardAbility.RetainedAfterDurabilityClean");
		}
		else if (eve.type == GameEvent.Type.SkillDurabilityClean)
		{
			GameSkill casterSkill = eve.CasterSkill;
			if (casterSkill.tempValues == null)
			{
				casterSkill.tempValues = new Dictionary<string, object>();
			}
			eve.CasterSkill.tempValues["DancerCharmPlayedCardAbility.RetainedAfterDurabilityClean"] = true;
			PlayerGameStateInfo playerState = eve.gameState.playerState;
			if (playerState.consumedSkills == null)
			{
				playerState.consumedSkills = new List<GameSkill>();
			}
			eve.gameState.playerState.consumedSkills.AddUnique(eve.CasterSkill);
		}
		else
		{
			Item caster = eve.CasterItem;
			GameSkill skill = eve.CasterSkill;
			SetProcessing(caster, skill, value: true);
			TriggerEffect(eve, effect);
			eve.gameState.actionManager.EnqueueAction(new ActionAction(delegate
			{
				SetProcessing(caster, skill, value: false);
			}));
		}
	}

	public static void SetActive(GameSkill skill)
	{
		if (skill != null)
		{
			if (skill.tempValues == null)
			{
				skill.tempValues = new Dictionary<string, object>();
			}
			skill.tempValues["DancerCharmPlayedCardAbility.ActiveThisTurn"] = true;
		}
	}

	private static bool IsActive(GameSkill skill)
	{
		if (skill?.tempValues != null && skill.tempValues.TryGetValue("DancerCharmPlayedCardAbility.ActiveThisTurn", out var value))
		{
			if (value is bool)
			{
				return (bool)value;
			}
			return false;
		}
		return false;
	}

	private static bool IsRetained(GameSkill skill)
	{
		if (skill?.tempValues != null && skill.tempValues.TryGetValue("DancerCharmPlayedCardAbility.RetainedAfterDurabilityClean", out var value))
		{
			if (value is bool)
			{
				return (bool)value;
			}
			return false;
		}
		return false;
	}

	private static bool IsProcessing(Item item, GameSkill skill)
	{
		if (item?.tempValues != null && item.tempValues.TryGetValue("DancerCharmPlayedCardAbility.Processing", out var value) && value is bool && (bool)value)
		{
			return true;
		}
		if (skill?.tempValues != null && skill.tempValues.TryGetValue("DancerCharmPlayedCardAbility.Processing", out var value2))
		{
			if (value2 is bool)
			{
				return (bool)value2;
			}
			return false;
		}
		return false;
	}

	private static void SetProcessing(Item item, GameSkill skill, bool value)
	{
		if (item != null)
		{
			if (item.tempValues == null)
			{
				item.tempValues = new Dictionary<string, object>();
			}
			item.tempValues["DancerCharmPlayedCardAbility.Processing"] = value;
		}
		if (skill != null)
		{
			if (skill.tempValues == null)
			{
				skill.tempValues = new Dictionary<string, object>();
			}
			skill.tempValues["DancerCharmPlayedCardAbility.Processing"] = value;
		}
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { triggerCheckNode, effect };
	}
}
