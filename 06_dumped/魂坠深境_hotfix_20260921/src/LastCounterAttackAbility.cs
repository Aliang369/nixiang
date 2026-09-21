using System.Collections.Generic;

public class LastCounterAttackAbility : Ability
{
	private const string LastCounterAttackInfluenceId = "3f2046245d34eb6bf941d04a3578af5f";

	private const string RecordedDamageKey = "LastCounterAttack.RecordedDamage";

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.AfterDamage;
		yield return GameEvent.Type.PlayerTurnStart;
		yield return GameEvent.Type.EnemyTurnStart;
		yield return GameEvent.Type.BattleStart;
		yield return GameEvent.Type.BeforeBattleEnd;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (!CheckTriggerGameEventAble(eve))
		{
			return;
		}
		Influence casterInfluence = eve.CasterInfluence;
		if (eve.type == GameEvent.Type.AfterDamage)
		{
			RecordDamage(casterInfluence, eve.damage.subCounter);
			return;
		}
		GameEvent.Type type = eve.type;
		if (type == GameEvent.Type.BattleStart || type == GameEvent.Type.BeforeBattleEnd)
		{
			ClearRecordedDamage(casterInfluence);
			return;
		}
		int recordedDamage = GetRecordedDamage(casterInfluence);
		ClearRecordedDamage(casterInfluence);
		if (recordedDamage > 0)
		{
			TriggerAnim(eve.gameState, casterInfluence);
			eve.gameState.actionManager.EnqueueAction(new DamageAction(new DamageCommand
			{
				baseDamage = recordedDamage,
				source = casterInfluence,
				targetBFT = eve.gameState.battleState?.GetOpponent(casterInfluence.OwnerBft),
				overflow = true,
				ignoreSourceDamageModifiers = true
			}));
		}
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (eve?.CasterInfluence?.OwnerBft == null || eve.CasterInfluence.def?.id != "3f2046245d34eb6bf941d04a3578af5f")
		{
			return false;
		}
		BattleFieldTransform ownerBft = eve.CasterInfluence.OwnerBft;
		if (eve.type == GameEvent.Type.AfterDamage)
		{
			if (eve.damage?.targetBFT == ownerBft && eve.damage.isHealthDamage && (int)eve.damage.subCounter > 0)
			{
				return (eve.damage.source as Influence)?.def?.id != "3f2046245d34eb6bf941d04a3578af5f";
			}
			return false;
		}
		if (eve.type == GameEvent.Type.PlayerTurnStart)
		{
			return ownerBft.isPlayer;
		}
		if (eve.type == GameEvent.Type.EnemyTurnStart)
		{
			return ownerBft.isEnemy;
		}
		GameEvent.Type type = eve.type;
		return type == GameEvent.Type.BattleStart || type == GameEvent.Type.BeforeBattleEnd;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode>();
	}

	private static int GetRecordedDamage(Influence influence)
	{
		object value = default(object);
		if (influence != null && influence.tempValues?.TryGetValue("LastCounterAttack.RecordedDamage", out value) == true && value is int)
		{
			return (int)value;
		}
		return 0;
	}

	private static void RecordDamage(Influence influence, int damage)
	{
		if (influence != null && damage > 0)
		{
			if (influence.tempValues == null)
			{
				influence.tempValues = new Dictionary<string, object>();
			}
			influence.tempValues["LastCounterAttack.RecordedDamage"] = CombatValueLimit.Clamp((long)GetRecordedDamage(influence) + (long)damage);
			influence.influenceValue = (int)influence.tempValues["LastCounterAttack.RecordedDamage"];
		}
	}

	private static void ClearRecordedDamage(Influence influence)
	{
		influence?.tempValues?.Remove("LastCounterAttack.RecordedDamage");
		if (influence != null)
		{
			influence.influenceValue = 0;
		}
	}
}
