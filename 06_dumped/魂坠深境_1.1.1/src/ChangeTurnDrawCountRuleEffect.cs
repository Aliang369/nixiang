public class ChangeTurnDrawCountRuleEffect : PVPAvatarEditRuleEffect
{
	public int deltaTurnDrawCount;

	public override void Apply(PVPDefenderProperty property)
	{
		property.turnDrawCount += deltaTurnDrawCount;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(deltaTurnDrawCount)
		};
	}
}
