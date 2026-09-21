using System;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using TMPro;
using UnityEngine;

public class ForerunnerLegacyOverviewPanel : UIBase, IStackPanel
{
	public SwitchPanel switchPanel;

	public SwitchTag currentLegacyAreaTag;

	public SwitchTag gotLegacyAreaTag;

	public SwitchTag leaveLegacyAreaTag;

	public GameObject currentLegacyArea;

	public GameObject gotLegacyArea;

	public GameObject leaveLegacyArea;

	public MatterCell gotCurrencyMatterCell;

	public GameButton shareRewardClaimButton;

	public GameObject shareRewardRedDot;

	public GameObject currentLocalLegacyContent;

	public TextMeshProUGUI currentLocalLegacyStayTimeText;

	public GameButton stopLocalLegacyButton;

	public ForerunnerLegacyRecordCell currentLocalLegacyCell;

	public GameObject currentShareLegacyContent;

	public TextMeshProUGUI currentShareLegacyStayTimeText;

	public GameButton stopShareLegacyButton;

	public ForerunnerLegacyRecordCell currentShareLegacyCell;

	public GameButton backButton;

	private bool claimingShareReward;

	[SerializeField]
	private CellUIPrefabPoolConfig<ForerunnerLegacyRecordCell, ForerunnerLegacy> gotLegacyHistoryPool;

	[SerializeField]
	private CellUIPrefabPoolConfig<ForerunnerLegacyRecordCell, ForerunnerLegacy> leaveLegacyHistoryPool;

	protected override void Init()
	{
		InitUI(switchPanel);
		InitUI(gotCurrencyMatterCell);
		InitUI(currentLocalLegacyCell);
		InitUI(currentShareLegacyCell);
		switchPanel.onTagSwitched = _OnTagSwitched;
		switchPanel.Select(currentLegacyAreaTag);
		stopLocalLegacyButton.onClick.AddListener(_OnStopLocalLegacyButtonClicked);
		stopShareLegacyButton.onClick.AddListener(_OnStopShareLegacyButtonClicked);
		shareRewardClaimButton.onClick.AddListener(_OnShareRewardClaimButtonClicked);
		backButton.onClick.AddListener(_OnBackButtonClicked);
		Init(gotLegacyHistoryPool);
		Init(leaveLegacyHistoryPool);
	}

	public void ShowUI()
	{
		SetActiveAndDirtyWithFadeAnim(active: true);
	}

	protected override void UpdateDisplay()
	{
		if (UIBase.applicationState.forerunnerManager.localForerunnerLegacy != null)
		{
			ForerunnerLegacy localForerunnerLegacy = UIBase.applicationState.forerunnerManager.localForerunnerLegacy;
			currentLocalLegacyContent.gameObject.SetActive(value: true);
			currentLocalLegacyCell.Setup(localForerunnerLegacy);
			currentLocalLegacyStayTimeText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_forerunner_legacy_remain_exist_time_arg0.FormatWith(TextDisplayHelper.GetRoughTimeSpanText(localForerunnerLegacy.expireTime - DateTime.UtcNow));
		}
		else
		{
			currentLocalLegacyContent.gameObject.SetActive(value: false);
		}
		if (UIBase.applicationState.forerunnerManager.shareForerunnerLegacy != null)
		{
			ForerunnerLegacy shareForerunnerLegacy = UIBase.applicationState.forerunnerManager.shareForerunnerLegacy;
			currentShareLegacyContent.gameObject.SetActive(value: true);
			currentShareLegacyCell.Setup(shareForerunnerLegacy);
			currentShareLegacyStayTimeText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_forerunner_legacy_remain_exist_time_arg0.FormatWith(TextDisplayHelper.GetRoughTimeSpanText(shareForerunnerLegacy.expireTime - DateTime.UtcNow));
		}
		else
		{
			currentShareLegacyContent.gameObject.SetActive(value: false);
		}
		gotCurrencyMatterCell.Setup(new MatterInfo(ScriptableObjectSingleton<FortressDataManager>.instance.weeklyTokenMatter, UIBase.applicationState.forerunnerManager.PendingShareRewardMatterCount));
		shareRewardClaimButton.interactable = UIBase.applicationState.forerunnerManager.HasPendingShareReward && !claimingShareReward;
		shareRewardRedDot.SetActive(UIBase.applicationState.forerunnerManager.HasPendingShareReward && !claimingShareReward);
		gotLegacyHistoryPool.DespawnAll();
		foreach (ForerunnerLegacy item in UIBase.applicationState.forerunnerManager.gotForerunnerLegacyHistory)
		{
			gotLegacyHistoryPool.Spawn(item);
		}
		leaveLegacyHistoryPool.DespawnAll();
		foreach (ForerunnerLegacy item2 in UIBase.applicationState.forerunnerManager.leaveForerunnerLegacyHistory)
		{
			leaveLegacyHistoryPool.Spawn(item2);
		}
	}

	private void _OnShareRewardClaimButtonClicked()
	{
		if (claimingShareReward || !UIBase.applicationState.forerunnerManager.HasPendingShareReward || !GlobalConfigManager.instance.enableNetwork)
		{
			return;
		}
		claimingShareReward = true;
		SetDirty();
		GeneralGameAsyncOperationResultOnly<GetLegacyShareRewardResult> op = new GeneralGameAsyncOperationResultOnly<GetLegacyShareRewardResult>(UIBase.applicationState.ChillyRoomSDK.Forerunner.GetLegacyShareRewardAsync);
		op.OnComplete(delegate
		{
			if (op.OpResult?.Reward != null)
			{
				GlobalRewardPackage globalRewardPackage = op.OpResult.TryGiveRewards(UIBase.applicationState);
				UIBase.applicationState.forerunnerManager.ClearPendingShareReward();
				if (globalRewardPackage != null)
				{
					UIBase.fortressState.uiManager.globalRewardPackageDisplay.Setup(globalRewardPackage);
				}
			}
		}).OnOver(delegate
		{
			UIBase.applicationState.forerunnerManager.RefreshForerunnerProfileData(delegate
			{
				claimingShareReward = false;
				SetDirty();
				UIBase.fortressState.uiManager.mainUIPanel.SetDirty();
			});
		}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_forerunner_data);
	}

	private void _OnTagSwitched()
	{
		if (switchPanel.targetShowTag == currentLegacyAreaTag)
		{
			currentLegacyArea.SetActive(value: true);
			gotLegacyArea.SetActive(value: false);
			leaveLegacyArea.SetActive(value: false);
			if (base.gameObject.activeInHierarchy)
			{
				ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "档案馆-先行者遗物-保管");
			}
		}
		else if (switchPanel.targetShowTag == gotLegacyAreaTag)
		{
			currentLegacyArea.SetActive(value: false);
			gotLegacyArea.SetActive(value: true);
			leaveLegacyArea.SetActive(value: false);
			if (base.gameObject.activeInHierarchy)
			{
				ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "档案馆-先行者遗物-收到");
			}
		}
		else if (switchPanel.targetShowTag == leaveLegacyAreaTag)
		{
			currentLegacyArea.SetActive(value: false);
			gotLegacyArea.SetActive(value: false);
			leaveLegacyArea.SetActive(value: true);
			if (base.gameObject.activeInHierarchy)
			{
				ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "档案馆-先行者遗物-留下");
			}
		}
	}

	private void _OnStopLocalLegacyButtonClicked()
	{
		UIBase.applicationState.forerunnerManager.CleanLocalLegacy();
		SetDirty();
	}

	private void _OnStopShareLegacyButtonClicked()
	{
		UIBase.applicationState.forerunnerManager.CleanShareLegacy();
		SetDirty();
	}

	private void _OnBackButtonClicked()
	{
		SetActiveAndDirtyWithFadeAnim(active: false);
	}
}
