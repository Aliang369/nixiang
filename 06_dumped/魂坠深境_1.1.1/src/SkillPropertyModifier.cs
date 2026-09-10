using System.Collections.Generic;

public class SkillPropertyModifier : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public SkillDef.SkillPropertyType type;

	public int deltaValuePerStack;

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		NodeDisplayInfo nodeDisplayInfo = new NodeDisplayInfo();
		int num = eve.CasterSkillWord?.stack ?? 1;
		nodeDisplayInfo.value = new NodeDisplayInfo.Element<int?>(deltaValuePerStack * num);
		return nodeDisplayInfo;
	}

	public List<IEffectNode> GetChildren()
	{
		return null;
	}
}
