using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using ContainerOfSoul.Display.Fortress.DotManagers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress.IndexDisplay;

public class FortressIndexUIPanel : UIBase, IShowAsync, IHideAsync
{
	[Serializable]
	public class IndexPageConfig
	{
		public SwitchTag tag;

		public HintRedDot redDot;

		public IndexPagePanel panel;
	}

	[SerializeField]
	public CanvasGroup canvasGroup;

	[SerializeField]
	public GameButton backButton;

	[SerializeField]
	private SwitchPanel switchPanel;

	[SerializeField]
	public List<IndexPageConfig> indexPageConfigs;

	[SerializeField]
	public TextMeshProUGUI allPageAvgDiscoverPercentTextDisplay;

	[SerializeField]
	public GameObject pageProgressArea;

	[SerializeField]
	public TextMeshProUGUI pageDiscoverNameText;

	[SerializeField]
	public TextMeshProUGUI pageDiscoverPercentTextDisplay;

	[SerializeField]
	public TextMeshProUGUI pageDiscoverCountTextDisplay;

	[SerializeField]
	public Slider pageDiscoverPercentProgressBar;

	[SerializeField]
	public GameObject currentPageNextLayerRewardArea;

	[SerializeField]
	public TextMeshProUGUI currentPageNextLayerRewardTextDisplay;

	[SerializeField]
	public GlobalItemDisplay currentPageNextLayerRewardItemDisplay;

	[SerializeField]
	public GameButton getPageNextLayerRewardButton;

	[SerializeField]
	public TextMeshProUGUI allLayerFinishedTextDisplay;

	[SerializeField]
	public GameObject allLayerFinishedMask;

	[SerializeField]
	public ItemInfoPanel itemInfoPanel;

	[SerializeField]
	private RectTransform itemInfoDisplayPivot;

	[NonSerialized]
	private IndexPagePanel currentIndexPanel;

	[NonSerialized]
	private bool claimingPageRewards;

	public bool CanClaimPageRewards => !claimingPageRewards;

	protected override void Init()
	{
		foreach (IndexPageConfig indexPageConfig in indexPageConfigs)
		{
			InitUI(indexPageConfig.panel);
			InitUI(indexPageConfig.redDot);
			indexPageConfig.redDot.Setup(new CustomModule
			{
				shouldShow = () => indexPageConfig.panel.WithIndexBookRewardToGet()
			});
		}
		InitUI(switchPanel);
		InitUI(itemInfoPanel);
		InitUI(currentPageNextLayerRewardItemDisplay);
		SwitchPanel obj = switchPanel;
		obj.onTagSwitched = (Action)Delegate.Combine(obj.onTagSwitched, new Action(_OnTagSwitched));
		backButton.onClick.AddListener(_OnBackButtonClicked);
		getPageNextLayerRewardButton.onClick.AddListener(_OnConfirmGetPageNextLayerRewardButtonClicked);
		UIBase.applicationState.eventManager.objectDiscoveredEvent.AddListener(_OnObjectDiscovered);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyUIAreaClicked);
		base.gameObject.SetActive(value: false);
		itemInfoDisplayPivot.gameObject.SetActive(value: true);
		switchPanel.Select(indexPageConfigs[0].tag);
	}

	IEnumerator IShowAsync.ShowUIAsync()
	{
		yield return UIBase.applicationState.discoverManager.FetchDiscoverData();
		SetActiveAndDirtyWithFadeAnim(active: true);
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "档案馆-图鉴");
		SetDirty();
		yield return _RouteToFirstClaimableRewardOnOpen();
	}

	IEnumerator IHideAsync.HideAnim()
	{
		SetActiveAndDirtyWithFadeAnim(active: false);
		yield return null;
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.objectDiscoveredEvent.RemoveListener(_OnObjectDiscovered);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyUIAreaClicked);
	}

	protected override void UpdateDisplay()
	{
		UpdateProgressDisplay();
	}

	private void UpdateProgressDisplay()
	{
		if (currentIndexPanel == null)
		{
			return;
		}
		float allPageCollectAvgRatio = UIBase.applicationState.discoverManager.GetAllPageCollectAvgRatio();
		allPageAvgDiscoverPercentTextDisplay.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_index_book_all_page_collect_ratio_arg0.FormatWith(TextDisplayHelper.NumToPercentageWithSmallPercentSign(allPageCollectAvgRatio));
		if (currentIndexPanel.GetIndexPageAllContent<ICTAsset>()?.ToList() != null)
		{
			IndexBookPage page = currentIndexPanel.GetIndexPageType();
			pageProgressArea.gameObject.SetActive(value: true);
			int indexPageMaxCount = DiscoverManager.GetIndexPageMaxCount(page, UIBase.applicationState.assetCatalog);
			int indexPageDiscoveredCount = UIBase.applicationState.discoverManager.GetIndexPageDiscoveredCount(page);
			float pageCollectRatio = UIBase.applicationState.discoverManager.GetPageCollectRatio(page);
			IndexBookConfig indexBookConfig = ScriptableObjectSingleton<FortressDataManager>.instance.indexBookConfigs.FirstOrDefault((IndexBookConfig c) => c.page == page);
			pageDiscoverNameText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_index_book_discover_progress_arg0.FormatWith(indexBookConfig?.pageTitle);
			pageDiscoverPercentTextDisplay.text = TextDisplayHelper.NumToPercentageWithSmallPercentSign(pageCollectRatio);
			pageDiscoverPercentProgressBar.value = pageCollectRatio;
			pageDiscoverCountTextDisplay.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_per_arg1.FormatWith(indexPageDiscoveredCount, indexPageMaxCount);
			IndexBookConfig.IndexBookProgressLayer pageNextToRewardProgressLayerConfig = UIBase.applicationState.discoverManager.GetPageNextToRewardProgressLayerConfig(currentIndexPanel.GetIndexPageType());
			if (pageNextToRewardProgressLayerConfig == null)
			{
				currentPageNextLayerRewardArea.SetActive(value: false);
				allLayerFinishedTextDisplay.gameObject.SetActive(value: true);
				allLayerFinishedMask.gameObject.SetActive(value: true);
				currentPageNextLayerRewardTextDisplay.gameObject.SetActive(value: false);
				return;
			}
			currentPageNextLayerRewardArea.SetActive(value: true);
			allLayerFinishedTextDisplay.gameObject.SetActive(value: false);
			allLayerFinishedMask.gameObject.SetActive(value: false);
			currentPageNextLayerRewardTextDisplay.gameObject.SetActive(value: true);
			int num = Mathf.Max(0, pageNextToRewardProgressLayerConfig.needDiscoverCount - indexPageDiscoveredCount);
			currentPageNextLayerRewardTextDisplay.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_index_book_get_next_page_layer_reward_with_arg0_count_discover.FormatWith(num);
			currentPageNextLayerRewardItemDisplay.Setup(pageNextToRewardProgressLayerConfig.reward);
			getPageNextLayerRewardButton.gameObject.SetActive(num == 0);
			getPageNextLayerRewardButton.interactable = !claimingPageRewards;
		}
		else
		{
			pageProgressArea.gameObject.SetActive(value: false);
		}
	}

	private void _OnTagSwitched()
	{
		currentIndexPanel?.ClearTransientSelection();
		foreach (IndexPageConfig indexPageConfig in indexPageConfigs)
		{
			if (switchPanel.targetShowTag == indexPageConfig.tag)
			{
				currentIndexPanel = indexPageConfig.panel;
				indexPageConfig.panel.gameObject.SetActive(value: true);
				if (base.gameObject.activeInHierarchy)
				{
					ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, indexPageConfig.panel.TASubPanelTagName);
				}
			}
			else
			{
				indexPageConfig.panel.gameObject.SetActive(value: false);
			}
		}
		currentIndexPanel?.ClearTransientSelection();
		itemInfoPanel.HideAllCommonInfo();
		SetDirty();
	}

	private void _OnBackButtonClicked()
	{
		manager.HideUI(this);
	}

	private void _OnConfirmGetPageNextLayerRewardButtonClicked()
	{
		ClaimAllPageRewards();
	}

	private IEnumerator _RouteToFirstClaimableRewardOnOpen()
	{
		yield return null;
		if (!base.gameObject.activeInHierarchy || currentIndexPanel == null)
		{
			yield break;
		}
		currentIndexPanel?.ClearTransientSelection();
		itemInfoPanel.HideAllCommonInfo();
		IndexPageConfig indexPageConfig = indexPageConfigs.FirstOrDefault((IndexPageConfig config) => config.panel == currentIndexPanel);
		bool currentPageHasReward = UIBase.applicationState.discoverManager.CheckPageWithAnyRewardToGet(currentIndexPanel.GetIndexPageType());
		IndexPageConfig targetConfig = (currentPageHasReward ? indexPageConfig : indexPageConfigs.FirstOrDefault((IndexPageConfig config) => config.panel.WithIndexBookRewardToGet()));
		if (targetConfig == null)
		{
			yield break;
		}
		if (targetConfig.panel != currentIndexPanel)
		{
			switchPanel.Select(targetConfig.tag);
		}
		yield return null;
		if (base.gameObject.activeInHierarchy)
		{
			if (!currentPageHasReward)
			{
				targetConfig.panel.TrySelectFirstRewardPage();
			}
			targetConfig.panel.ClearTransientSelection();
			itemInfoPanel.HideAllCommonInfo();
			SetDirty();
			yield return null;
			if (base.gameObject.activeInHierarchy && !targetConfig.panel.CurrentPageHasReadyProgressReward())
			{
				targetConfig.panel.TryFocusFirstClaimableSingleReward();
			}
		}
	}

	public void ClaimAllPageRewards()
	{
		if (claimingPageRewards)
		{
			return;
		}
		List<IndexBookPage> list = (from page in ScriptableObjectSingleton<FortressDataManager>.instance.indexBookConfigs.Select((IndexBookConfig config) => config.page).Distinct()
			where UIBase.applicationState.discoverManager.CheckPageWithAnyRewardToGet(page)
			select page).ToList();
		if (list.Count != 0)
		{
			claimingPageRewards = true;
			getPageNextLayerRewardButton.interactable = false;
			currentIndexPanel?.ClearTransientSelection();
			itemInfoPanel.HideAllCommonInfo();
			SetDirty();
			List<GlobalRewardInfo> claimedRewards = new List<GlobalRewardInfo>();
			if (GlobalConfigManager.instance.enableNetwork)
			{
				ClaimNextPageRewardsOnline(list, 0, claimedRewards);
			}
			else
			{
				CompleteAllPageRewardClaim(claimedRewards);
			}
		}
	}

	private void ClaimNextPageRewardsOnline(List<IndexBookPage> claimablePages, int pageIndex, List<GlobalRewardInfo> claimedRewards)
	{
		if (pageIndex >= claimablePages.Count)
		{
			CompleteAllPageRewardClaim(claimedRewards);
			return;
		}
		IndexBookPage page = claimablePages[pageIndex];
		int pageReadySingleRewardCount = UIBase.applicationState.discoverManager.GetPageReadySingleRewardCount(page);
		int pageReadyProgressRewardCount = UIBase.applicationState.discoverManager.GetPageReadyProgressRewardCount(page);
		ClaimSingleRewardsOnline(page, pageReadySingleRewardCount > 0, pageReadyProgressRewardCount, claimedRewards, delegate
		{
			ClaimNextPageRewardsOnline(claimablePages, pageIndex + 1, claimedRewards);
		});
	}

	private void ClaimSingleRewardsOnline(IndexBookPage page, bool hasSingleRewards, int progressRewardCount, List<GlobalRewardInfo> claimedRewards, Action onPageComplete)
	{
		if (!hasSingleRewards)
		{
			ClaimNextProgressRewardOnline(page, progressRewardCount, claimedRewards, onPageComplete);
			return;
		}
		GetIndexPageUnGotSingleRewardRequest request = new GetIndexPageUnGotSingleRewardRequest
		{
			Page = (ChillyRoom.Services.ContainerOfSoul.IndexBookPage)page
		};
		GeneralGameAsyncOperation<GetIndexPageUnGotSingleRewardRequest, GetIndexPageUnGotSingleRewardsResult> operation = new GeneralGameAsyncOperation<GetIndexPageUnGotSingleRewardRequest, GetIndexPageUnGotSingleRewardsResult>(request, UIBase.applicationState.ChillyRoomSDK.IndexBook.GetIndexPageUnGotSingleRewardsAsync);
		bool operationSucceeded = false;
		operation.OnComplete(delegate
		{
			operationSucceeded = true;
			if (operation.OpResult?.Rewards == null || operation.OpResult.Rewards.Count == 0)
			{
				TraceLog.Debug($"图鉴页{page}服务端返回空单项奖励结果。");
			}
			else
			{
				NetworkConvertContext context = new NetworkConvertContext(UIBase.applicationState);
				List<GlobalRewardInfo> list = operation.OpResult.Rewards.Values.Select((ChillyRoom.Services.ContainerOfSoul.GlobalRewardInfo reward) => reward.ToClient(context)).ToList();
				new GlobalRewardPackage(list).GiveAll(UIBase.applicationState);
				claimedRewards.AddRange(list);
				foreach (string key in operation.OpResult.Rewards.Keys)
				{
					ICTAsset content = context.FullIdToCTAsset<ICTAsset>(key);
					UIBase.applicationState.discoverManager.MarkContentAsRewarded(content);
				}
				int num = Mathf.Max(1, DiscoverManager.GetIndexPageMaxCount(page, UIBase.applicationState.assetCatalog));
				float exploreProgress = (float)UIBase.applicationState.discoverManager.GetIndexPageDiscoveredCount(page) / (float)num;
				ThinkingAnalyticsExtends.TrackGetIndexbookReward(UIBase.applicationState, "单项发现奖励", page.ToString(), list.Count, 0, exploreProgress);
			}
		}).OnOver(delegate
		{
			if (operationSucceeded)
			{
				ClaimNextProgressRewardOnline(page, progressRewardCount, claimedRewards, onPageComplete);
			}
			else
			{
				CompleteAllPageRewardClaim(claimedRewards);
			}
		}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_fetching_index_reward);
	}

	private void ClaimNextProgressRewardOnline(IndexBookPage page, int remainingProgressRewardCount, List<GlobalRewardInfo> claimedRewards, Action onPageComplete)
	{
		if (remainingProgressRewardCount <= 0)
		{
			onPageComplete?.Invoke();
			return;
		}
		GetIndexPageProgressLayerRewardRequest request = new GetIndexPageProgressLayerRewardRequest
		{
			Page = (ChillyRoom.Services.ContainerOfSoul.IndexBookPage)page
		};
		GeneralGameAsyncOperation<GetIndexPageProgressLayerRewardRequest, GetIndexPageProgressLayerRewardResult> operation = new GeneralGameAsyncOperation<GetIndexPageProgressLayerRewardRequest, GetIndexPageProgressLayerRewardResult>(request, UIBase.applicationState.ChillyRoomSDK.IndexBook.GetIndexPageNextProgressLayerRewardAsync);
		bool rewardGranted = false;
		operation.OnComplete(delegate
		{
			if (operation.OpResult?.Reward != null)
			{
				rewardGranted = true;
				NetworkConvertContext context = new NetworkConvertContext(UIBase.applicationState);
				GlobalRewardInfo globalRewardInfo = operation.OpResult.Reward.ToClient(context);
				new GlobalRewardPackage(globalRewardInfo).GiveAll(UIBase.applicationState);
				claimedRewards.Add(globalRewardInfo);
				int rewardLevel = UIBase.applicationState.discoverManager.GetPageProgressRewardedLayerCount(page) + 1;
				UIBase.applicationState.discoverManager.IncrementPageProgressRewardedLayer(page);
				int num = Mathf.Max(1, DiscoverManager.GetIndexPageMaxCount(page, UIBase.applicationState.assetCatalog));
				float exploreProgress = (float)UIBase.applicationState.discoverManager.GetIndexPageDiscoveredCount(page) / (float)num;
				ThinkingAnalyticsExtends.TrackGetIndexbookReward(UIBase.applicationState, "进度层奖励", page.ToString(), 1, rewardLevel, exploreProgress);
			}
		}).OnOver(delegate
		{
			if (rewardGranted)
			{
				ClaimNextProgressRewardOnline(page, remainingProgressRewardCount - 1, claimedRewards, onPageComplete);
			}
			else
			{
				CompleteAllPageRewardClaim(claimedRewards);
			}
		}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_fetching_index_reward);
	}

	private void CompleteAllPageRewardClaim(List<GlobalRewardInfo> claimedRewards)
	{
		claimingPageRewards = false;
		SetDirty();
		foreach (IndexPageConfig indexPageConfig in indexPageConfigs)
		{
			indexPageConfig.panel.SetDirty();
			indexPageConfig.redDot.SetDirty();
		}
		UIBase.fortressState.uiManager.mainUIPanel.SetDirty();
		if (!claimedRewards.IsNullOrEmpty())
		{
			GlobalRewardPackage reward = new GlobalRewardPackage(GlobalRewardInfo.CompressRewardsForDisplay(claimedRewards));
			UIBase.fortressState.uiManager.globalRewardPackageDisplay.Setup(reward);
		}
		currentIndexPanel?.ClearTransientSelection();
		itemInfoPanel.HideAllCommonInfo();
	}

	private static void AppendRewards(List<GlobalRewardInfo> rewards, GlobalRewardPackage rewardPack)
	{
		if (rewardPack != null && !rewardPack.rewards.IsNullOrEmpty())
		{
			rewards.AddRange(rewardPack.rewards);
		}
	}

	private void _OnObjectDiscovered(ICTAsset obj)
	{
		SetDirty();
	}

	private void _OnEmptyUIAreaClicked()
	{
		itemInfoPanel.HideAllCommonInfo();
	}
}
