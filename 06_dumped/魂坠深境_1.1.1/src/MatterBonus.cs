using UnityEngine;

public class MatterBonus : BattleBonusObject, PickBonusSelectionInfoPanel.IShowTextHint
{
	[SaveField(null, null)]
	public MatterInfo matterInfo;

	public override object infoObject => matterInfo;

	public override Sprite PreviewIcon => matterInfo.matterDef.icon;

	public override string countText => matterInfo.count.ToString();

	public override string previewText => matterInfo.matterDef.matterName;

	string PickBonusSelectionInfoPanel.IShowTextHint.GetHint()
	{
		return previewText;
	}

	private MatterBonus()
	{
	}

	public MatterBonus(MatterInfo matterInfo)
	{
		this.matterInfo = matterInfo;
	}

	protected override void TriggerImpl(GameStateInfo gameState)
	{
		GlobalRewardInfo reward = new GlobalRewardInfo
		{
			matter = matterInfo
		};
		gameState.recordManager.inGameMatterRewards.Add(matterInfo);
		GlobalRewardPackage reward2 = new GlobalRewardPackage(reward);
		gameState.UIManager.globalRewardInfoDisplay.Setup(reward2);
		ConfirmGotBonus();
	}
}
