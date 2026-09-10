using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress.IndexDisplay;

public class IndexBookContentMask : UIBase
{
	public GameObject singleRewardMask;

	public GameObject rewardRedDot;

	public GameObject newDiscoverTag;

	public Image rewardIcon;

	public TextMeshProUGUI rewardAmountText;

	public GameButton getRewardButton;

	private IndexPagePanel indexPagePanel;

	private ICTAsset target;

	private InputModuleBase contentInputModule;

	private FortressIndexUIPanel indexPanel;

	protected override void Init()
	{
		indexPanel = GetUIInParents<FortressIndexUIPanel>();
		getRewardButton.onClick.AddListener(_OnGetRewardButtonClick);
		contentInputModule = GetComponentInParent<InputModuleBase>(includeInactive: true);
		if (contentInputModule != null)
		{
			InputModuleBase inputModuleBase = contentInputModule;
			inputModuleBase.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(inputModuleBase.onHandleInput, new Action<InputModuleBase>(_OnContentInput));
		}
	}

	protected override void Dispose()
	{
		if (contentInputModule != null)
		{
			InputModuleBase inputModuleBase = contentInputModule;
			inputModuleBase.onHandleInput = (Action<InputModuleBase>)Delegate.Remove(inputModuleBase.onHandleInput, new Action<InputModuleBase>(_OnContentInput));
		}
	}

	public void Setup(ICTAsset target)
	{
		indexPagePanel = GetUIInParents<IndexPagePanel>();
		this.target = target;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		if (indexPagePanel == null || target == null || !UIBase.applicationState.discoverManager.IsDiscovered(target))
		{
			SetHintActive(rewardActive: false, newDiscoverActive: false);
			return;
		}
		IndexBookPage pageType = indexPagePanel.GetIndexPageType();
		IndexBookConfig indexBookConfig = ScriptableObjectSingleton<FortressDataManager>.instance.indexBookConfigs.FirstOrDefault((IndexBookConfig c) => c.page == pageType);
		bool flag = indexBookConfig?.singleDiscoverReward != null && !UIBase.applicationState.discoverManager.IsRewarded(target);
		bool newDiscoverActive = ((DiscoverManager.GetNewDiscoverTagMode(pageType) == IndexBookNewDiscoverTagMode.Reward) ? flag : UIBase.applicationState.discoverManager.IsNewDiscoveredUnviewed(target));
		SetHintActive(flag, newDiscoverActive);
		if (flag)
		{
			rewardIcon.sprite = indexBookConfig.singleDiscoverReward.GlobalItemCellIcon;
			rewardAmountText.text = TextDisplayHelper.NumToStringWithSign(indexBookConfig.singleDiscoverReward.GlobalItemCount);
		}
		getRewardButton.interactable = flag && (indexPanel?.CanClaimPageRewards ?? false);
	}

	private void SetHintActive(bool rewardActive, bool newDiscoverActive)
	{
		singleRewardMask.SetActive(rewardActive);
		rewardIcon.gameObject.SetActive(rewardActive);
		rewardAmountText.gameObject.SetActive(rewardActive);
		getRewardButton.gameObject.SetActive(rewardActive);
		if (rewardRedDot != null)
		{
			rewardRedDot.SetActive(rewardActive);
		}
		if (newDiscoverTag != null)
		{
			newDiscoverTag.SetActive(newDiscoverActive);
		}
	}

	private void _OnContentInput(InputModuleBase inputModule)
	{
		if (inputModule.pointerClicked || inputModule.pointerDown)
		{
			MarkTargetViewed();
		}
	}

	public void MarkTargetViewed()
	{
		if (target != null && !(indexPagePanel == null) && DiscoverManager.GetNewDiscoverTagMode(indexPagePanel.GetIndexPageType()) == IndexBookNewDiscoverTagMode.View && UIBase.applicationState.discoverManager.IsNewDiscoveredUnviewed(target))
		{
			UIBase.applicationState.discoverManager.MarkDiscoveredContentViewed(target);
			SetDirty();
		}
	}

	private void _OnGetRewardButtonClick()
	{
		MarkTargetViewed();
		if (!(indexPagePanel == null) && !(indexPanel == null))
		{
			indexPanel.ClaimAllPageRewards();
		}
	}
}
