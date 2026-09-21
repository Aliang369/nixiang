using System.Collections.Generic;

public class ForSpells : Effect
{
	public ValueGenerator timesGenerator;

	public Effect node;

	public bool CastEffectInActionQueue;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		int? value = timesGenerator.GetValue(eve);
		if (!value.HasValue)
		{
			return NodeCastResult.Failure;
		}
		if (CastEffectInActionQueue)
		{
			GameEvent neve = eve.ShallowCopy();
			for (int i = 0; i < value.Value; i++)
			{
				Effect.EnqueueAction(eve, new ActionAction(delegate
				{
					node.Cast(neve);
				}));
			}
		}
		else
		{
			for (int num = 0; num < value.Value; num++)
			{
				node.Cast(eve);
			}
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (node != null)
		{
			list.Add(node);
		}
		return list;
	}
}
