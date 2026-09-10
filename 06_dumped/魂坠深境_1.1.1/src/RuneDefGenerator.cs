using System.Collections.Generic;

public abstract class RuneDefGenerator : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			refRuneDefs = new NodeDisplayInfo.Element<List<RuneDef>>(GenerateRuneDefs(eve))
		};
	}

	public virtual List<IEffectNode> GetChildren()
	{
		return null;
	}

	public abstract List<RuneDef> GenerateRuneDefs(GameEvent eve);
}
