using System;
using Common.UI;
using UnityEngine;

public class AfterBossUpgradePanel : UIBase
{
	[SerializeField]
	private AfterBossUpgradeInfoDisplay infoDisplayPrefab;

	[SerializeField]
	private RectTransform infoDisplayParent;

	[SerializeField]
	private WillPowerProgressBarDisplay willPowerProgressBar;

	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private AfterBossUpgradeRefreshButton propsRefreshButton;

	[SerializeField]
	private AfterBossUpgradeRefreshButton adRefreshButton;

	[NonSerialized]
	private CellUIPrefabPool<AfterBossUpgradeInfoDisplay, AfterBossUpgrade> afterBossUpgradeDisplayPool;

	[NonSerialized]
	private IAfterBossUpgradeSource upgradeSource;

	protected override bool shouldUpdate => parent.gameObject.activeInHierarchy;

	private AfterBossUpgradeEventPanel panel => UIBase.gameState.UIManager.worldEventPanel_AfterBossUpgrade;

	private IUpgradeChoicePresentation presentation => upgradeSource as IUpgradeChoicePresentation;

	private bool UsesDirectLeaveLayout
	{
		get
		{
			IAfterBossUpgradeSource afterBossUpgradeSource = upgradeSource;
			return afterBossUpgradeSource is RootBlessUpgradeEvent || afterBossUpgradeSource is AfterBossWorldEvent;
		}
	}

	public bool UsesWillPower => presentation?.UsesWillPower ?? true;

	public bool ShowWillPowerCost => presentation?.ShowWillPowerCost ?? true;

	private bool ShowWillPowerProgress => presentation?.ShowWillPowerProgress ?? true;

	private GameObject WillPowerInfoRoot => willPowerProgressBar.transform.parent.gameObject;

	private bool CanUseAdRefresh
	{
		get
		{
			IUpgradeChoicePresentation upgradeChoicePresentation = presentation;
			if (upgradeChoicePresentation == null || upgradeChoicePresentation.AdRefreshRequiresRootBlessUnlock)
			{
				return IsRefreshUnlocked;
			}
			return true;
		}
	}

	public bool IsRefreshUnlocked
	{
		get
		{
			DungeonGenerationManager.RootBlessUpgradeConfig rootBlessUpgradeConfig = ScriptableObjectSingleton<DungeonGenerationManager>.instance.rootBlessUpgradeConfig;
			if (rootBlessUpgradeConfig != null)
			{
				if (!(rootBlessUpgradeConfig.refreshUnlockClavis != null) || !UIBase.gameState.applicationState.clavisManager.IsActivated(rootBlessUpgradeConfig.refreshUnlockClavis))
				{
					return UIBase.gameState.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableRootBonfireBlessRefresh);
				}
				return true;
			}
			return false;
		}
	}

	protected override void Init()
	{
		infoDisplayPrefab.gameObject.SetActive(value: false);
		afterBossUpgradeDisplayPool = GetPrefabPool<AfterBossUpgradeInfoDisplay, AfterBossUpgrade>(infoDisplayPrefab, infoDisplayParent);
		InitUI(willPowerProgressBar);
		backButton.onClick.AddListener(_OnBackButtonClicked);
		if (propsRefreshButton != null)
		{
			propsRefreshButton.Init(_OnPropsRefreshButtonClicked);
			propsRefreshButton.SetShown(shown: false);
		}
		if (adRefreshButton != null)
		{
			adRefreshButton.Init(_OnAdRefreshButtonClicked);
			adRefreshButton.SetShown(shown: false);
		}
	}

	public void Setup(IAfterBossUpgradeSource upgradeSource)
	{
		this.upgradeSource = upgradeSource;
		if (upgradeSource?.AdRefresh != null && CanUseAdRefresh)
		{
			Bootstrap.instance.adsManager.EnterAdScene("refresh_bless");
		}
		SetDirty();
	}

	public void Unset()
	{
		afterBossUpgradeDisplayPool.DespawnAll();
	}

	protected override void UpdateDisplay()
	{
		if (upgradeSource != null)
		{
			afterBossUpgradeDisplayPool.Sync(upgradeSource.AllAfterBossUpgrade);
			panel.RecordVisibleBossBlessOptions();
			backButton.gameObject.SetActive(presentation?.ShowBackButton ?? (!UsesDirectLeaveLayout));
			WillPowerInfoRoot.SetActive(ShowWillPowerProgress);
			if (ShowWillPowerProgress)
			{
				willPowerProgressBar.SetDirty();
			}
			RefreshPropsRefreshButton();
			RefreshAdRefreshButton();
		}
	}

	private void _OnBackButtonClicked()
	{
		panel._OnBackButtonClicked();
	}

	private void RefreshPropsRefreshButton()
	{
		if (!(propsRefreshButton == null))
		{
			UpgradeRefreshModel upgradeRefreshModel = upgradeSource?.CurrencyRefresh;
			bool flag = upgradeRefreshModel != null && upgradeRefreshModel.Available && upgradeRefreshModel.HasCost;
			propsRefreshButton.SetShown(flag);
			if (flag)
			{
				int matterCount = UIBase.applicationState.matterManager.GetMatterCount(upgradeRefreshModel.Matter);
				propsRefreshButton.BindCurrency(upgradeRefreshModel.UsedCount, upgradeRefreshModel.MaxCount, upgradeRefreshModel.CanRefresh, upgradeRefreshModel.CurrentPrice, matterCount, upgradeRefreshModel.Matter.icon);
			}
		}
	}

	private void _OnPropsRefreshButtonClicked()
	{
		UpgradeRefreshModel upgradeRefreshModel = upgradeSource?.CurrencyRefresh;
		if (upgradeRefreshModel != null)
		{
			if (!upgradeRefreshModel.CanRefresh)
			{
				UIBase.gameState.UIManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_not_enough_matter);
				SetDirty();
			}
			else
			{
				upgradeRefreshModel.Refresh();
				SetDirty();
			}
		}
	}

	private void RefreshAdRefreshButton()
	{
		UpgradeRefreshModel upgradeRefreshModel = upgradeSource?.AdRefresh;
		bool flag = upgradeRefreshModel != null && upgradeRefreshModel.Available && CanUseAdRefresh;
		adRefreshButton.SetShown(flag);
		if (flag)
		{
			bool flag2 = Bootstrap.instance.adsManager.CanClaimReward("refresh_bless");
			if (!flag2)
			{
				Bootstrap.instance.adsManager.LoadAd("refresh_bless");
			}
			adRefreshButton.BindAd(upgradeRefreshModel.UsedCount, upgradeRefreshModel.MaxCount, upgradeRefreshModel.CanRefresh & flag2);
		}
	}

	private void _OnAdRefreshButtonClicked()
	{
		UpgradeRefreshModel refresh = upgradeSource?.AdRefresh;
		if (refresh != null && refresh.CanRefresh)
		{
			Bootstrap.instance.adsManager.ShowAd("refresh_bless", delegate
			{
				refresh.Refresh();
				SetDirty();
			}, delegate
			{
				UIBase.gameState.UIManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_ad_not_finished_to_get_reward);
			}, delegate
			{
			});
		}
	}
}
