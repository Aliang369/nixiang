public class ChangePointLimitRuleEffect : PVPAvatarEditRuleEffect
{
	public int deltaPointLimit;

	public override void Apply(PVPDefenderProperty property)
	{
		property.diyPoint += deltaPointLimit;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(deltaPointLimit)
		};
	}
}
