using UnityEngine;

public class UnlockCharacter : GlobalRewardSetup
{
	[SaveField(null, null)]
	public PlayerCharacterDef character;

	public MatterDef obsession => character.obsession;

	public int amount => 1;

	public override Sprite GlobalItemCellIcon => obsession.icon;

	public override string GlobalItemTitle => obsession.matterName;

	public override PlayerCharacterDef GlobalItemRefPlayerCharacter => character;

	public override int? GetGlobalItemStorageCount(ApplicationStateInfo applicationState)
	{
		return applicationState.matterManager.GetMatterCount(obsession);
	}

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return obsession.description;
	}

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		return new GlobalRewardInfo
		{
			matter = new MatterInfo(obsession, 1)
		};
	}

	public override string GetRewardBrief()
	{
		return FGetRewardBrief(amount, obsession?.matterName);
	}
}
