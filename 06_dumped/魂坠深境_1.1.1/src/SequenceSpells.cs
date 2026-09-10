using System.Collections.Generic;

public class SequenceSpells : Effect
{
	public bool CastEffectInActionQueue;

	public CheckNode textUpgradeCheck;

	public List<Effect> nodes = new List<Effect>();

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (CastEffectInActionQueue)
		{
			GameEvent neve = eve.ShallowCopy();
			foreach (Effect spell in nodes)
			{
				Effect.EnqueueAction(eve, new ActionAction(delegate
				{
					spell.Cast(neve);
				}));
			}
		}
		else
		{
			foreach (Effect node in nodes)
			{
				node.Cast(eve);
			}
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (textUpgradeCheck != null)
		{
			list.Add(textUpgradeCheck);
		}
		if (nodes != null)
		{
			list.AddRange(nodes);
		}
		return list;
	}
}
