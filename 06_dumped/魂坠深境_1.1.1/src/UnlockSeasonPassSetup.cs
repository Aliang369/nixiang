public class UnlockSeasonPassSetup : GlobalRewardSetup
{
	public SeasonDef seasonDef;

	public override bool isHidden => true;

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		return new GlobalRewardInfo
		{
			unlockSeasonPass = seasonDef
		};
	}

	public override string GetRewardBrief()
	{
		return FGetRewardBrief(seasonDef?.seasonName);
	}
}
