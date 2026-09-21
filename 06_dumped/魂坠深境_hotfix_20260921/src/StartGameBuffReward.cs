using UnityEngine;

public class StartGameBuffReward : GlobalRewardSetup
{
	public StartGameBuffDef startGameBuffDef;

	public int lifetimeDays = 7;

	public override Sprite GlobalItemCellIcon => startGameBuffDef.icon;

	public override string GlobalItemTitle => FGlobalItemTitle(startGameBuffDef.buffName);

	public override int GlobalItemCount => 1;

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		StartGameBuff startGameBuff = StartGameBuff.CreateBuffOfLifeTime(startGameBuffDef, lifetimeDays);
		return base.GlobalItemStringConfig.GetGlobalItemDescription.FormatWith(TextDataPreset.FormatStartGameBuffDescription(startGameBuff, applicationState, withHintAndExpireData: true, out generator), startGameBuff.GetTimeExpireDescription(applicationState));
	}

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		return new GlobalRewardInfo
		{
			getStartGameBuff = StartGameBuff.CreateBuffOfLifeTime(startGameBuffDef, lifetimeDays)
		};
	}

	public override string GetRewardBrief()
	{
		return FGetRewardBrief(startGameBuffDef?.buffName);
	}
}
