using System.Collections.Generic;

public class ChangeMInfluenceValue : Effect
{
	public enum OpType
	{
		Set,
		Change
	}

	public ValueGenerator valueGenerator;

	public OpType type;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (eve.CasterInfluence == null)
		{
			return NodeCastResult.Failure;
		}
		if (valueGenerator == null)
		{
			return NodeCastResult.Failure;
		}
		int value = valueGenerator.GenerateValue(eve).GetValueOrDefault();
		Influence castInfluence = eve.CasterInfluence;
		switch (type)
		{
		case OpType.Set:
			Effect.EnqueueAction(eve, new ActionAction(delegate
			{
				castInfluence.influenceValue = value;
			}));
			break;
		case OpType.Change:
			Effect.EnqueueAction(eve, new ActionAction(delegate
			{
				castInfluence.influenceValue += value;
			}));
			break;
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { valueGenerator };
	}
}
