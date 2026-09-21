using UnityEngine;

public class GameOverInfoPanel : CommonInfoPanelBase
{
	[SerializeField]
	private SkillInfoDisplay skillInfoDisplay;

	[SerializeField]
	private InfluenceDisplay influenceDisplay;

	[SerializeField]
	private ItemInfoDisplayBase itemInfoDisplay;

	protected override void Init()
	{
		base.Init();
		InitUI(skillInfoDisplay);
		InitUI(influenceDisplay);
		InitUI(itemInfoDisplay);
		skillInfoDisplay.gameObject.SetActive(value: false);
		influenceDisplay.gameObject.SetActive(value: false);
		itemInfoDisplay.gameObject.SetActive(value: false);
	}

	protected override void UpdateInfo()
	{
		if (currentCommonInfoData?.target == null)
		{
			skillInfoDisplay.Unset();
			influenceDisplay.Unset();
			itemInfoDisplay.Unset();
		}
		else if (currentCommonInfoData.target is Influence setupData)
		{
			influenceDisplay.Setup(setupData);
		}
		else if (currentCommonInfoData.target is GameSkill setupData2)
		{
			skillInfoDisplay.Setup(setupData2);
		}
		else if (currentCommonInfoData.target is ProfileSkill profileSkill)
		{
			skillInfoDisplay.Setup(profileSkill.GetGameSkill());
		}
		else if (currentCommonInfoData.target is Item setupData3)
		{
			itemInfoDisplay.Setup(setupData3);
		}
		else if (currentCommonInfoData.target is GlobalRewardInfo globalRewardInfo)
		{
			if (globalRewardInfo.skill != null)
			{
				skillInfoDisplay.Setup(globalRewardInfo.skill.GetGameSkill());
			}
			else if (globalRewardInfo.unlockItemSkin != null)
			{
				itemInfoDisplay.Setup(new Item(globalRewardInfo.unlockItemSkin.attachItem).SetSkin(globalRewardInfo.unlockItemSkin));
			}
		}
	}
}
