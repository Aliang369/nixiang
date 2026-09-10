using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class YuanQiActivityPanel : UIBase, IStackPanel
{
	[Serializable]
	private class LanguageToImage
	{
		public Image Image;

		public LocalizationManager.LanguageCode Code;
	}

	[SerializeField]
	private TextMeshProUGUI remainTimeText;

	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private List<YuanQiActivityMissionCell> missionCells = new List<YuanQiActivityMissionCell>();

	[SerializeField]
	private List<LanguageToImage> LanguageToImages;

	protected override void Init()
	{
		backButton.onClick.AddListener(((UIBase)this).HideUI);
		foreach (YuanQiActivityMissionCell missionCell in missionCells)
		{
			if (!(missionCell == null))
			{
				InitUI(missionCell);
				missionCell.SetActions(OnMissionGoClicked, OnMissionClaimClicked);
			}
		}
	}

	protected override void UpdateDisplay()
	{
		YuanQiActivityManager yuanQiActivityManager = UIBase.applicationState.yuanQiActivityManager;
		remainTimeText.text = yuanQiActivityManager.GetRemainTimeText();
		RefreshTitleImage();
		IReadOnlyList<YuanQiActivityMissionItem> missionItems = yuanQiActivityManager.Model.MissionItems;
		for (int i = 0; i < missionCells.Count; i++)
		{
			YuanQiActivityMissionCell yuanQiActivityMissionCell = missionCells[i];
			if (!(yuanQiActivityMissionCell == null))
			{
				bool flag = i < missionItems.Count;
				yuanQiActivityMissionCell.gameObject.SetActive(flag);
				if (flag)
				{
					yuanQiActivityMissionCell.Setup(missionItems[i]);
				}
			}
		}
	}

	private void OnMissionGoClicked(YuanQiActivityMissionItem missionItem)
	{
		if (missionItem.CanJump)
		{
			this.HideUI();
			UIBase.applicationState.yuanQiActivityManager.JumpToMission(missionItem.Data.missionId);
		}
	}

	private void OnMissionClaimClicked(YuanQiActivityMissionItem missionItem)
	{
		if (missionItem.CanClaim)
		{
			UIBase.applicationState.yuanQiActivityManager.ClaimMissionReward(missionItem.Data.missionId);
			SetDirty();
		}
	}

	private void OnEnable()
	{
		RefreshTitleImage();
	}

	private void RefreshTitleImage()
	{
		for (int i = 0; i < LanguageToImages.Count; i++)
		{
			LanguageToImages[i].Image.gameObject.SetActive(LanguageToImages[i].Code == ScriptableObjectSingleton<LocalizationManager>.instance.currentLanguage);
		}
	}
}
