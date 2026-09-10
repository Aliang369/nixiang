using System.Collections.Generic;

public class DealDamage : Effect
{
	public ValueGenerator baseDamageGenerator;

	public ValueGenerator damageTimesGenerator;

	public ValueGenerator extraDamageGenerator;

	public GameEvent.EventBFTType targetBFT;

	public bool overflow = true;

	public bool pierce;

	public HitNFXConfig hitEffect;

	public HitNFX3DConfig hitEffect3D;

	public PersistentHitNFX3DConfig persistentHitEffect3D;

	public TransformNFXConfig transformVFXEffect;

	public TransformNFX3DConfig transformEffect3D;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		BattleFieldTransform eventBFT = eve.GetEventBFT(targetBFT);
		if (eventBFT == null)
		{
			return NodeCastResult.Success;
		}
		int? baseDamage = GetBaseDamage(eve);
		if (!baseDamage.HasValue)
		{
			return NodeCastResult.Failure;
		}
		object eveObj = eve.GetEveObj(GameEvent.EveObjType.Caster);
		eve.damagecommand = new DamageCommand
		{
			source = eveObj,
			targetBFT = eve.GetEventBFT(targetBFT),
			spellCastInfo = eve.spellInfo,
			transformSetup = transformVFXEffect,
			transform3DSetup = transformEffect3D,
			hitSetup = hitEffect,
			hit3DSetup = hitEffect3D,
			persistentHit3DSetup = persistentHitEffect3D
		};
		int? finalTimes = GetFinalTimes(eve);
		for (int i = 0; i < finalTimes.Value; i++)
		{
			Effect.EnqueueAction(eve, new DamageAction(new DamageCommand
			{
				baseDamage = baseDamage.Value,
				source = eveObj,
				targetBFT = eventBFT,
				overflow = overflow,
				pierce = pierce,
				spellCastInfo = ((eve.type == GameEvent.Type.NodeCastSpell) ? eve.spellInfo : null),
				transformSetup = transformVFXEffect,
				transform3DSetup = transformEffect3D,
				hitSetup = hitEffect,
				hit3DSetup = hitEffect3D,
				persistentHit3DSetup = persistentHitEffect3D
			}));
		}
		return NodeCastResult.Success;
	}

	public int? GetBaseDamage(GameEvent eve)
	{
		int? num = baseDamageGenerator.GenerateValue(eve);
		if (!num.HasValue)
		{
			return null;
		}
		long num2 = num.Value;
		if (extraDamageGenerator != null)
		{
			int? num3 = extraDamageGenerator.GenerateValue(eve);
			if (!num3.HasValue)
			{
				return null;
			}
			num2 += num3.Value;
		}
		return CombatValueLimit.SaturateToInt(num2);
	}

	private int? GetBaseTimes(GameEvent eve)
	{
		return ((damageTimesGenerator != null) ? damageTimesGenerator.GenerateValue(eve) : new int?(1)) ?? ((int?)null);
	}

	private int? GetDeltaTimes(GameEvent eve)
	{
		int num = 0;
		if (eve.gameState?.gameEventManager == null)
		{
			return num;
		}
		GameEvent.ResolveInfo resolveInfo = eve.gameState.gameEventManager.TriggerGenerateDamageResolveInfo(eve.damagecommand, eve.triggerType, Damage.ResolveType.DamageTimes, eve.TargetItem);
		return num + resolveInfo.deltaTimes;
	}

	private int? GetFinalTimes(GameEvent eve)
	{
		int? baseTimes = GetBaseTimes(eve);
		int? deltaTimes = GetDeltaTimes(eve);
		if (!baseTimes.HasValue)
		{
			return null;
		}
		return DamageAction.ClampBatchCount((long)baseTimes.Value + (long)deltaTimes.GetValueOrDefault());
	}

	public int? GetDeltaDamage(GameEvent eve)
	{
		return GetPreviewCastDamage(eve) - GetBaseDamage(eve);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		eve.damagecommand = new DamageCommand();
		eve.damagecommand.source = eve.CasterItem;
		eve.damagecommand.targetBFT = eve.GetEventBFT(targetBFT);
		eve.damagecommand.spellCastInfo = eve.spellInfo;
		eve.damagecommand.source = eve.CasterItem;
		return new NodeDisplayInfo
		{
			baseValue = new NodeDisplayInfo.Element<int?>(GetBaseDamage(eve)),
			deltaValue = new NodeDisplayInfo.Element<int?>(GetDeltaDamage(eve)),
			times = new NodeDisplayInfo.Element<int?>(GetFinalTimes(eve)),
			deltaTimes = new NodeDisplayInfo.Element<int?>(GetDeltaTimes(eve)),
			value = new NodeDisplayInfo.Element<int?>(GetPreviewCastDamage(eve)),
			damage = new NodeDisplayInfo.Element<bool>(value: true)
		};
	}

	public int? GetPreviewCastDamage(GameEvent eve)
	{
		int? baseDamage = GetBaseDamage(eve);
		if (!baseDamage.HasValue)
		{
			return null;
		}
		BattleFieldTransform battleFieldTransform = eve.GetEventBFT(targetBFT) ?? eve.GetEventBFT(GameEvent.EventBFTType.Opponent);
		Damage damage = Damage.GenerateDamage(new DamageCommand
		{
			baseDamage = baseDamage.Value,
			source = eve.CasterItem,
			overflow = overflow,
			pierce = pierce,
			spellCastInfo = eve.spellInfo,
			targetBFT = battleFieldTransform
		}, eve.gameState, eve.triggerType);
		if (damage == null)
		{
			return 0;
		}
		return damage.castDamage;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { baseDamageGenerator, extraDamageGenerator, damageTimesGenerator };
	}
}
