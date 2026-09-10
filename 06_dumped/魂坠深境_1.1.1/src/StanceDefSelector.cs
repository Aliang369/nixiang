using System.Collections.Generic;

public abstract class StanceDefSelector : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			refStanceDef = new NodeDisplayInfo.Element<StanceDef>(GenerateSingleStanceDef(eve))
		};
	}

	public virtual List<IEffectNode> GetChildren()
	{
		return null;
	}

	public abstract List<StanceDef> GenerateStanceDefs(GameEvent eve);

	public StanceDef GetSingleStanceDef(GameEvent eve)
	{
		List<StanceDef> list = GenerateStanceDefs(eve);
		if (list == null || list.Count == 0)
		{
			return null;
		}
		return list[0];
	}

	public abstract StanceDef GenerateSingleStanceDef(GameEvent eve);
}
