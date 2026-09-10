using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatterCell : CellUI<MatterInfo>, IDisplayModulesController
{
	public class DisplayModule : UIDisplayModule
	{
		protected MatterCell matterCell => parent as MatterCell;

		protected MatterInfo matterInfo => matterCell.setupData;
	}

	[SerializeField]
	private Image matterImage;

	[SerializeField]
	private TextMeshProUGUI matterNameText;

	[SerializeField]
	private TextMeshProUGUI matterDescriptionText;

	[SerializeField]
	private MatterHintPanel matterHintPanel;

	[NonSerialized]
	public MatterInfo matterInfo;

	protected override void Init()
	{
		if (GetModule<InfoModule>() != null)
		{
			if (matterHintPanel != null)
			{
				GetModule<InfoModule>().infoPanel = matterHintPanel;
			}
			else if (CellUI<MatterInfo>.fortressState != null)
			{
				GetModule<InfoModule>().infoPanel = CellUI<MatterInfo>.fortressState.uiManager.universalCommonInfoPanel;
			}
		}
		if (GetModule<RankModule>() != null && GetModule<RankModule>().fQuality == null)
		{
			GetModule<RankModule>().fQuality = () => ((IGlobalItemDisplaySetup)matterInfo).GlobalItemQuality;
		}
	}

	protected override void OnSetup(MatterInfo matterInfo)
	{
		this.matterInfo = matterInfo;
		matterImage.sprite = matterInfo.matterDef.icon;
		if (matterNameText != null)
		{
			matterNameText.text = matterInfo.matterDef.NameWithColor;
		}
		if (matterDescriptionText != null)
		{
			matterDescriptionText.text = this.matterInfo.matterDef.description;
		}
		SetDirty();
	}
}
