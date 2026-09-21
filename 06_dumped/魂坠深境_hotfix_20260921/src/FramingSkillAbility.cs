using System.Collections.Generic;

public sealed class FramingSkillAbility : Ability
{
	private const string ActiveKey = "FramingSkillAbility.ActiveThisTurn";

	private const string RetainedKey = "FramingSkillAbility.RetainedAfterDurabilityClean";

	private const int DamageBonus = 3;

	public static void SetActive(GameSkill skill)
	{
		if (skill != null)
		{
			if (skill.tempValues == null)
			{
				skill.tempValues = new Dictionary<string, object>();
			}
			skill.tempValues["FramingSkillAbility.ActiveThisTurn"] = true;
		}
	}

	public static BattleFieldTransform GetDealTypeSourceOwnerOverride(GameStateInfo gameState, DamageCommand damageCommand)
	{
		PlayerGameStateInfo playerGameStateInfo = gameState?.playerState;
		BattleFieldTransform battleFieldTransform = playerGameStateInfo?.bft;
		if (battleFieldTransform == null || (damageCommand?.source as IBftOwnable)?.OwnerBft != battleFieldTransform)
		{
			return null;
		}
		foreach (GameSkill allSkill in playerGameStateInfo.AllSkills)
		{
			if (IsActive(allSkill) && HasFramingAbility(allSkill))
			{
				return gameState.battleState?.GetOpponent(battleFieldTransform);
			}
		}
		return null;
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.GenerateDamageDamageValueResolve;
		yield return GameEvent.Type.PlayerTurnEnd;
		yield return GameEvent.Type.BattleStart;
		yield return GameEvent.Type.BeforeBattleEnd;
		yield return GameEvent.Type.SkillDurabilityClean;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		GameSkill eventSkill = GetEventSkill(eve);
		if (eventSkill == null)
		{
			return false;
		}
		if ((!base.CheckTriggerGameEventAble(eve) && !IsRetained(eventSkill)) || !IsActive(eventSkill))
		{
			return false;
		}
		if (eve.type == GameEvent.Type.PlayerTurnEnd || eve.type == GameEvent.Type.BattleStart || eve.type == GameEvent.Type.BeforeBattleEnd || eve.type == GameEvent.Type.SkillDurabilityClean)
		{
			return true;
		}
		if (eve.type == GameEvent.Type.GenerateDamageDamageValueResolve && eve.resolveInfo != null && eve.damageResolveType == Damage.ResolveType.DamageValue)
		{
			return (eve.damagecommand?.source as IBftOwnable)?.OwnerBft == eventSkill.OwnerBft;
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (!CheckTriggerGameEventAble(eve))
		{
			return;
		}
		GameSkill eventSkill = GetEventSkill(eve);
		if (eve.type == GameEvent.Type.GenerateDamageDamageValueResolve)
		{
			eve.resolveInfo.sourceDelta += 3;
		}
		else if (eve.type == GameEvent.Type.SkillDurabilityClean)
		{
			GameSkill gameSkill = eventSkill;
			if (gameSkill.tempValues == null)
			{
				gameSkill.tempValues = new Dictionary<string, object>();
			}
			eventSkill.tempValues["FramingSkillAbility.RetainedAfterDurabilityClean"] = true;
			PlayerGameStateInfo playerState = eve.gameState.playerState;
			if (playerState.consumedSkills == null)
			{
				playerState.consumedSkills = new List<GameSkill>();
			}
			eve.gameState.playerState.consumedSkills.AddUnique(eventSkill);
		}
		else
		{
			eventSkill.tempValues?.Remove("FramingSkillAbility.ActiveThisTurn");
			eventSkill.tempValues?.Remove("FramingSkillAbility.RetainedAfterDurabilityClean");
		}
	}

	private static bool IsActive(GameSkill skill)
	{
		if (skill?.tempValues != null && skill.tempValues.TryGetValue("FramingSkillAbility.ActiveThisTurn", out var value))
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
		if (skill?.tempValues != null && skill.tempValues.TryGetValue("FramingSkillAbility.RetainedAfterDurabilityClean", out var value))
		{
			if (value is bool)
			{
				return (bool)value;
			}
			return false;
		}
		return false;
	}

	private static GameSkill GetEventSkill(GameEvent eve)
	{
		object obj = eve?.CasterSkill;
		if (obj == null)
		{
			if (eve == null)
			{
				return null;
			}
			obj = eve.TargetSkill;
		}
		return (GameSkill)obj;
	}

	private static bool HasFramingAbility(GameSkill skill)
	{
		if (skill == null)
		{
			return false;
		}
		return skill.skillDef?.abilities?.Exists((Ability ability) => ability is FramingSkillAbility) == true;
	}
}
