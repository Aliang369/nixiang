using UnityEngine;

public class DeckInfoPanel : CommonInfoPanelBase
{
	[SerializeField]
	private InfluenceDisplay influenceDisplay;

	[SerializeField]
	private SkillInfoDisplay skillInfoDisplay;

	[SerializeField]
	private ItemInfoDisplayBase itemInfoDisplay;

	[SerializeField]
	private ClavisInfoDisplay clavisInfoDisplay;

	protected override void Init()
	{
		base.Init();
		InitUI(influenceDisplay);
		InitUI(skillInfoDisplay);
		InitUI(itemInfoDisplay);
		InitUI(clavisInfoDisplay);
	}

	protected override void UpdateInfo()
	{
		if (currentCommonInfoData == null)
		{
			skillInfoDisplay.Unset();
			influenceDisplay.Unset();
			itemInfoDisplay.Unset();
			clavisInfoDisplay.Unset();
		}
		else if (currentCommonInfoData.target is Influence setupData)
		{
			influenceDisplay.Setup(setupData);
		}
		else if (currentCommonInfoData.target is GameSkill setupData2)
		{
			skillInfoDisplay.Setup(setupData2);
		}
		else if (currentCommonInfoData.target is GameSkillSlot { gameSkill: not null } gameSkillSlot)
		{
			skillInfoDisplay.Setup(gameSkillSlot.gameSkill);
		}
		else if (currentCommonInfoData.target is Item setupData3)
		{
			itemInfoDisplay.Setup(setupData3);
		}
		else if (currentCommonInfoData.target is ClavisDef setupData4)
		{
			clavisInfoDisplay.Setup(setupData4);
		}
	}

	public void RefreshCurrentInfo()
	{
		UpdateInfo();
	}
}
