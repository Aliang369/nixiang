using System.Collections.Generic;

public class Heal : Effect
{
	public ValueGenerator healValueGenerator;

	public GameEvent.EventBFTType targetBft;

	public ValueGenerator timesGenerator;

	public HitNFX3DConfig hitEffect3D;

	public TransformNFX3DConfig transformEffect3D;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		int finalHeal = GetFinalHeal(eve);
		if (targetBft != GameEvent.EventBFTType.None)
		{
			for (int i = 0; i < GetHealTimes(eve); i++)
			{
				BattleFieldTransform eventBFT = eve.GetEventBFT(targetBft);
				transformEffect3D?.Play(eve, eve.GetEventBFT(GameEvent.EventBFTType.Owner), eventBFT);
				Effect.EnqueueAction(eve, new HealAction(eve.Caster, eventBFT, finalHeal));
				hitEffect3D?.Play(eve, eventBFT);
			}
		}
		return NodeCastResult.Success;
	}

	public int GetBaseHealValue(GameEvent eve)
	{
		int? num = healValueGenerator?.GenerateValue(eve);
		if (!num.HasValue)
		{
			return 1;
		}
		return num.Value;
	}

	public int GetDeltaHealValue(GameEvent eve)
	{
		return 0;
	}

	public int GetFinalHeal(GameEvent eve)
	{
		return GetBaseHealValue(eve) + GetDeltaHealValue(eve);
	}

	public int GetHealTimes(GameEvent eve)
	{
		if (timesGenerator == null)
		{
			return 1;
		}
		return timesGenerator.GenerateValue(eve).Value;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			baseValue = new NodeDisplayInfo.Element<int?>(GetBaseHealValue(eve)),
			deltaValue = new NodeDisplayInfo.Element<int?>(GetDeltaHealValue(eve)),
			value = new NodeDisplayInfo.Element<int?>(GetFinalHeal(eve)),
			times = new NodeDisplayInfo.Element<int?>(GetHealTimes(eve))
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { healValueGenerator };
	}
}
