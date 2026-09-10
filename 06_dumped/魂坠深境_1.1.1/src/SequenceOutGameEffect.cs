using System.Collections.Generic;

public class SequenceOutGameEffect : Effect, IOutGameEffectNode, IEffectCastNode, IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public List<IOutGameEffectNode> effects = new List<IOutGameEffectNode>();

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		foreach (IOutGameEffectNode effect in effects)
		{
			effect.Cast(eve);
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode>(effects.OrEmptyIfNull());
	}
}
