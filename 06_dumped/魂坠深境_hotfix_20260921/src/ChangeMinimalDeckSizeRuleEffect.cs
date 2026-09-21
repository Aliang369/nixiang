public class ChangeMinimalDeckSizeRuleEffect : PVPAvatarEditRuleEffect
{
	public int deltaMinimalDeckSize;

	public override void Apply(PVPDefenderProperty property)
	{
		property.minimalDeckSize += deltaMinimalDeckSize;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(deltaMinimalDeckSize)
		};
	}
}
