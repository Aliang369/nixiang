public class SmallRewardCell : CellUI<GlobalRewardInfo>
{
	public SkillDisplay skillDisplay;

	public MatterCell matterCell;

	public ItemDisplayBase itemDisplay;

	private GlobalRewardInfo reward;

	protected override void Init()
	{
		InitUI(skillDisplay);
		InitUI(matterCell);
		InitUI(itemDisplay);
	}

	protected override void OnSetup(GlobalRewardInfo t)
	{
		reward = t;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		if (reward != null)
		{
			if (reward.skill != null)
			{
				skillDisplay.Setup(new GameSkill(reward.skill));
				skillDisplay.SetActiveAndDirty(active: true);
			}
			else
			{
				skillDisplay.SetActiveAndDirty(active: false);
			}
			if (reward.matter != null)
			{
				matterCell.Setup(reward.matter);
				matterCell.SetActiveAndDirty(active: true);
			}
			else
			{
				matterCell.SetActiveAndDirty(active: false);
			}
			if (reward.unlockItemSkin != null)
			{
				itemDisplay.Setup(reward.unlockItemSkin.GenerateSkinedItem());
				itemDisplay.SetActiveAndDirty(active: true);
			}
			else
			{
				itemDisplay.SetActiveAndDirty(active: false);
			}
		}
	}
}
