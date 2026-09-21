using System.Collections.Generic;

public class RuneConstraint : ItemConstraint
{
	public RuneDef rune;

	public override bool? CheckItemAccept(Item item)
	{
		return rune.CheckCanAddRuneTo(item);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		NodeDisplayInfo nodeDisplayInfo = new NodeDisplayInfo();
		if (rune == null)
		{
			nodeDisplayInfo.refRuneDefs = new NodeDisplayInfo.Element<List<RuneDef>>(null);
		}
		else
		{
			nodeDisplayInfo.refRuneDefs = new NodeDisplayInfo.Element<List<RuneDef>>(new List<RuneDef> { rune });
		}
		return nodeDisplayInfo;
	}
}
