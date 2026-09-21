using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AfterBossWorldEvent : WorldEvent, IAfterBossUpgradeSource
{
	public class AfterIntroFirstBossVisitConfig
	{
		[LocalizeText("事件文本")]
		public string overrideEventName;

		[TextArea]
		[LocalizeText("事件文本")]
		public string overrideEventDescription;

		public WorldEventChoiceText continueChoiceText;

		public WorldEventChoiceText stopChoiceText;
	}

	[SerializeField]
	[LocalizeText("事件文本")]
	public string confirmText;

	[SerializeField]
	[LocalizeText("事件文本")]
	public string confirmDescriptionText;

	[SerializeField]
	[LocalizeText("事件文本")]
	public string title;

	[SerializeField]
	[LocalizeText("事件文本")]
	[TextArea]
	public string afterBossEventDescription;

	[SerializeField]
	[LocalizeText("事件文本")]
	[TextArea]
	public string rootBlessOfferingChosenDescription;

	public AfterIntroFirstBossVisitConfig firstBossVisitConfig;

	[NonSerialized]
	public List<AfterBossUpgrade> allAfterBossUpgrade;

	[NonSerialized]
	public List<AfterBossUpgrade> upgraded;

	[NonSerialized]
	public bool upgradeFinished;

	[NonSerialized]
	private bool firstBossVisitContinue;

	[NonSerialized]
	private CurrencyUpgradeRefreshModel currencyRefresh;

	[NonSerialized]
	private AdUpgradeRefreshModel adRefresh;

	[NonSerialized]
	private WorldEventChoice firstBossVisitContinueChoice;

	[NonSerialized]
	private WorldEventChoice firstBossVisitStopChoice;

	[NonSerialized]
	private WorldEventChoice selectBossUpgradeChoice;

	[NonSerialized]
	private WorldEventChoice rootBlessOfferingChoice;

	[NonSerialized]
	private bool rootBlessOfferingChosen;

	[NonSerialized]
	private List<AfterBossUpgrade> recordedExposureOptions;

	public override WorldEventDisplayType displayType => WorldEventDisplayType.AfterBossUpgrade;

	public List<AfterBossUpgrade> AllAfterBossUpgrade => allAfterBossUpgrade;

	public UpgradeRefreshModel CurrencyRefresh => currencyRefresh;

	public UpgradeRefreshModel AdRefresh => adRefresh;

	private DungeonGenerationManager.RootBlessUpgradeConfig RootBlessConfig => ScriptableObjectSingleton<DungeonGenerationManager>.instance?.rootBlessUpgradeConfig;

	private string RootBlessOfferingChosenDescriptionText
	{
		get
		{
			if (!rootBlessOfferingChosenDescription.IsNullOrEmpty())
			{
				return rootBlessOfferingChosenDescription;
			}
			return afterBossEventDescription;
		}
	}

	public bool IsRefreshUnlocked
	{
		get
		{
			DungeonGenerationManager.RootBlessUpgradeConfig rootBlessConfig = RootBlessConfig;
			if (rootBlessConfig != null)
			{
				if (!(rootBlessConfig.refreshUnlockClavis != null) || !gameState.applicationState.clavisManager.IsActivated(rootBlessConfig.refreshUnlockClavis))
				{
					return gameState.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableRootBonfireBlessRefresh);
				}
				return true;
			}
			return false;
		}
	}

	public override WorldEventPanel GetDisplayWorldEventPanel(GameStateInfo gameState)
	{
		return gameState.UIManager.worldEventPanel_AfterBossUpgrade;
	}

	protected override void Init()
	{
		base.Init();
		upgraded = new List<AfterBossUpgrade>();
		allAfterBossUpgrade = GenerateBossUpgrades(gameState);
		recordedExposureOptions = null;
		upgradeFinished = false;
		firstBossVisitContinue = false;
		rootBlessOfferingChosen = false;
		currencyRefresh = new CurrencyUpgradeRefreshModel(gameState, () => IsRefreshUnlocked && !upgradeFinished, () => RootBlessConfig?.refreshPrices, () => RootBlessConfig?.refreshMatter, delegate
		{
			allAfterBossUpgrade = GenerateBossUpgrades(gameState, updateRecord: false, isRefresh: true);
		}, "刷新Boss祝福");
		adRefresh = new AdUpgradeRefreshModel(() => !upgradeFinished, () => RootBlessConfig?.adRefreshMaxCount ?? 1, delegate
		{
			allAfterBossUpgrade = GenerateBossUpgrades(gameState, updateRecord: false, isRefresh: true);
		});
		foreach (BossBlessUpgrade item in allAfterBossUpgrade.OfType<BossBlessUpgrade>())
		{
			ThinkingAnalyticsExtends.TrackExposeBonusBless(gameState, item.inf.def, "Boss祝福", item.CheckTriggerAble(gameState));
		}
		firstBossVisitContinueChoice = new WorldEventChoice
		{
			buttonText = firstBossVisitConfig.continueChoiceText.choiceName,
			descriptionText = firstBossVisitConfig.continueChoiceText.choiceDescription,
			executeAction = delegate
			{
				firstBossVisitContinue = true;
				RefreshAfterChoose(firstBossVisitConfig.continueChoiceText.chosenDescription);
			}
		};
		firstBossVisitStopChoice = new WorldEventChoice
		{
			buttonText = firstBossVisitConfig.stopChoiceText.choiceName,
			descriptionText = firstBossVisitConfig.stopChoiceText.choiceDescription,
			executeAction = delegate
			{
				gameState.actionManager.EnqueueAction(new EndGameAction(EndGameAction.EndGameType.Win, LastGameEndReason.ReturnToLobby));
			}
		};
		selectBossUpgradeChoice = new WorldEventChoice
		{
			buttonText = confirmText,
			descriptionText = confirmDescriptionText,
			executeAction = delegate
			{
				gameState.UIManager.worldEventPanel_AfterBossUpgrade.ToBossUpgrade();
			}
		};
		rootBlessOfferingChoice = new WorldEventChoice
		{
			executeAbleChecker = ShouldShowRootBlessOfferingChoice,
			executeAction = ExecuteRootBlessOfferingChoice
		};
	}

	protected override void UpdateDisplayInfo(WorldEventDisplayInfo displayInfo)
	{
		if (gameState.applicationState.guideManager.IsAfterIntroProtect)
		{
			displayInfo.title = firstBossVisitConfig.overrideEventName;
			if (firstBossVisitContinue)
			{
				displayInfo.description = (rootBlessOfferingChosen ? RootBlessOfferingChosenDescriptionText : firstBossVisitConfig.continueChoiceText.chosenDescription);
				displayInfo.ClearChoice();
				if (!upgradeFinished)
				{
					displayInfo.AddChoice(selectBossUpgradeChoice);
				}
				AddRootBlessOfferingChoiceIfNeeded(displayInfo);
				displayInfo.showLeaveButton = true;
			}
			else
			{
				displayInfo.description = firstBossVisitConfig.overrideEventDescription;
				displayInfo.ClearChoice();
				displayInfo.AddChoice(firstBossVisitStopChoice);
				displayInfo.AddChoice(firstBossVisitContinueChoice);
				displayInfo.showLeaveButton = false;
			}
		}
		else
		{
			displayInfo.title = title;
			displayInfo.description = (rootBlessOfferingChosen ? RootBlessOfferingChosenDescriptionText : afterBossEventDescription);
			displayInfo.ClearChoice();
			if (!upgradeFinished)
			{
				displayInfo.AddChoice(selectBossUpgradeChoice);
			}
			AddRootBlessOfferingChoiceIfNeeded(displayInfo);
			displayInfo.showLeaveButton = true;
		}
	}

	public List<AfterBossUpgrade> GenerateBossUpgrades(GameStateInfo gameState, bool updateRecord = true, bool isRefresh = false)
	{
		List<AfterBossUpgrade> list = new List<AfterBossUpgrade>();
		List<Influence> list2 = gameState.generationManager.GenerateRandomBossBless(updateRecord, isRefresh);
		list.AddRange(list2.ConvertAll((Influence b) => new BossBlessUpgrade(b)));
		return list;
	}

	public void RecordDisplayedBossBlessOptions()
	{
		if (recordedExposureOptions == allAfterBossUpgrade)
		{
			return;
		}
		recordedExposureOptions = allAfterBossUpgrade;
		if (allAfterBossUpgrade == null)
		{
			return;
		}
		foreach (BossBlessUpgrade item in allAfterBossUpgrade.OfType<BossBlessUpgrade>())
		{
			gameState.recordManager.RecordBossBlessExposure(item.inf?.def);
		}
	}

	private void AddRootBlessOfferingChoiceIfNeeded(WorldEventDisplayInfo displayInfo)
	{
		DungeonGenerationManager.RootBlessUpgradeConfig rootBlessUpgradeConfig = ScriptableObjectSingleton<DungeonGenerationManager>.instance?.rootBlessUpgradeConfig;
		if (!IsRefreshUnlocked && !(rootBlessUpgradeConfig?.refreshUnlockClavis == null))
		{
			rootBlessOfferingChoice.buttonText = rootBlessUpgradeConfig.refreshUnlockClavis.GetMajorText(gameState.applicationState);
			rootBlessOfferingChoice.descriptionText = rootBlessUpgradeConfig.refreshUnlockClavis.GetMinorText(gameState.applicationState);
			displayInfo.AddChoice(rootBlessOfferingChoice);
		}
	}

	private bool ShouldShowRootBlessOfferingChoice()
	{
		DungeonGenerationManager.RootBlessUpgradeConfig rootBlessUpgradeConfig = ScriptableObjectSingleton<DungeonGenerationManager>.instance?.rootBlessUpgradeConfig;
		if (rootBlessUpgradeConfig != null && rootBlessUpgradeConfig.CheckRefreshClavisCanOffer(gameState.applicationState))
		{
			return HasEnoughRootBlessOfferingMatter(rootBlessUpgradeConfig);
		}
		return false;
	}

	private bool HasEnoughRootBlessOfferingMatter(DungeonGenerationManager.RootBlessUpgradeConfig config)
	{
		if (config.offeringMatter == null || config.offeringMatterCount <= 0)
		{
			return true;
		}
		return gameState.applicationState.matterManager.HasEnoughMatter(config.offeringMatter, config.offeringMatterCount);
	}

	private void ExecuteRootBlessOfferingChoice()
	{
		DungeonGenerationManager.RootBlessUpgradeConfig rootBlessUpgradeConfig = ScriptableObjectSingleton<DungeonGenerationManager>.instance?.rootBlessUpgradeConfig;
		if (rootBlessUpgradeConfig == null || !rootBlessUpgradeConfig.CheckRefreshClavisCanOffer(gameState.applicationState))
		{
			return;
		}
		if (!HasEnoughRootBlessOfferingMatter(rootBlessUpgradeConfig))
		{
			gameState.UIManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_not_enough_matter);
			return;
		}
		if (rootBlessUpgradeConfig.offeringMatter != null && rootBlessUpgradeConfig.offeringMatterCount > 0)
		{
			gameState.applicationState.matterManager.ConsumeMatter(rootBlessUpgradeConfig.offeringMatter, rootBlessUpgradeConfig.offeringMatterCount, "始源祝福刷新献礼");
		}
		gameState.applicationState.clavisManager.ActivatedClavis(rootBlessUpgradeConfig.refreshUnlockClavis, silent: false, delegate
		{
			rootBlessOfferingChosen = true;
			SetPanelDirty();
		});
	}

	public void OnUpgradeTriggered(AfterBossUpgrade upgrade)
	{
		if (upgrade != null)
		{
			upgraded.Add(upgrade);
			upgradeFinished = true;
			if (upgrade is BossBlessUpgrade bossBlessUpgrade)
			{
				gameState.recordManager.RecordBossBlessPick(bossBlessUpgrade.inf?.def);
				gameState.recordManager.generatedInfluence.AddWithoutDuplicate(bossBlessUpgrade.inf.def);
			}
		}
	}
}
