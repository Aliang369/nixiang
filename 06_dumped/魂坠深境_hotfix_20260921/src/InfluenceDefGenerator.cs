using System.Collections.Generic;

public abstract class InfluenceDefGenerator : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		NodeDisplayInfo nodeDisplayInfo = new NodeDisplayInfo();
		List<Influence> list = GenerateInfluenceDefs(eve)?.FindAll((InfluenceDef id) => id != null).ConvertAll((InfluenceDef id) => new Influence(id));
		if (list != null)
		{
			nodeDisplayInfo.refInfluences = new NodeDisplayInfo.Element<List<Influence>>(list);
		}
		return nodeDisplayInfo;
	}

	public virtual List<IEffectNode> GetChildren()
	{
		return null;
	}

	public abstract List<InfluenceDef> GenerateInfluenceDefs(GameEvent eve);
}
