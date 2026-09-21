public class ChangeHealthRuleEffect : PVPAvatarEditRuleEffect
{
	public int deltaHealth;

	public override void Apply(PVPDefenderProperty property)
	{
		property.health += deltaHealth;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(deltaHealth)
		};
	}
}
