public class UnlockPVPCompReward : GlobalRewardSetup
{
	public ItemDefinition unlockItem;

	public RuneDef unlockRune;

	public InfluenceDef unlockInfluence;

	public DeckPresetCompDef UnlockPVPDeckPresetComp
	{
		get
		{
			DeckPresetCompDef result = null;
			if (unlockItem != null)
			{
				result = new DeckPresetCompDef(unlockItem);
			}
			if (unlockRune != null)
			{
				result = new DeckPresetCompDef(unlockRune);
			}
			if (unlockInfluence != null)
			{
				result = new DeckPresetCompDef(unlockInfluence);
			}
			return result;
		}
	}

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		GlobalRewardInfo globalRewardInfo = new GlobalRewardInfo();
		if (unlockItem != null)
		{
			globalRewardInfo.unlockPVPDeckPresetComp = new DeckPresetCompDef(unlockItem);
		}
		if (unlockRune != null)
		{
			globalRewardInfo.unlockPVPDeckPresetComp = new DeckPresetCompDef(unlockRune);
		}
		if (unlockInfluence != null)
		{
			globalRewardInfo.unlockPVPDeckPresetComp = new DeckPresetCompDef(unlockInfluence);
		}
		return globalRewardInfo;
	}
}
