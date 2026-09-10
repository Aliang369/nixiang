using System.Collections.Generic;

public class LoopCastNode : Effect
{
	public bool CastEffectInActionQueue;

	public ValueGenerator loopTimeGenerator;

	public Effect node;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		int value = loopTimeGenerator.GenerateValue(eve).Value;
		if (node == null)
		{
			return NodeCastResult.Failure;
		}
		if (CastEffectInActionQueue)
		{
			GameEvent neve = eve.ShallowCopy();
			for (int i = 0; i < value; i++)
			{
				neve.CastNodeLoopIndex = i;
				Effect.EnqueueAction(eve, new ActionAction(delegate
				{
					node.Cast(neve);
				}));
			}
		}
		else
		{
			for (int num = 0; num < value; num++)
			{
				eve.CastNodeLoopIndex = num;
				node.Cast(eve);
			}
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { loopTimeGenerator, node };
	}
}
