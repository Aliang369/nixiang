using System;
using System.Collections.Generic;
using GameRandom;

public class RootBonfireHealSpringEvent : HealSpringEvent, IAfterBossUpgradeSource
{
	public WorldEventChoiceText rootBlessChoiceText;

	public WorldEventChoiceText offeringChoiceText;

	public MatterDef offeringMatter;

	public int offeringMatterCount = 1;

	public MatterDef refreshMatter;

	public List<int> refreshPrices = new List<int> { 30, 90, 150 };

	public ClavisDef refreshUnlockClavis;

	public int adRefreshMaxCount = 1;

	[NonSerialized]
	public List<AfterBossUpgrade> allAfterBossUpgrade;

	[NonSerialized]
	public List<AfterBossUpgrade> upgraded;

	[NonSerialized]
	public bool upgradeFinished;

	[NonSerialized]
	private CurrencyUpgradeRefreshModel currencyRefresh;

	[NonSerialized]
	private AdUpgradeRefreshModel adRefresh;

	[NonSerialized]
	private WorldEventChoice rootBlessChoice;

	[NonSerialized]
	private WorldEventChoice offeringChoice;

	protected override bool ShouldSetupHealChoice => false;

	protected override bool ShouldSetupDefaultUtilityChoices => false;

	public override WorldEventDisplayType displayType => WorldEventDisplayType.AfterBossUpgrade;

	public List<AfterBossUpgrade> AllAfterBossUpgrade => allAfterBossUpgrade;

	public UpgradeRefreshModel CurrencyRefresh => currencyRefresh;

	public UpgradeRefreshModel AdRefresh => adRefresh;

	private bool IsRefreshUnlocked
	{
		get
		{
			if (!(refreshUnlockClavis != null) || !gameState.applicationState.clavisManager.IsActivated(refreshUnlockClavis))
			{
				return gameState.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableRootBonfireBlessRefresh);
			}
			return true;
		}
	}

	public override WorldEventPanel GetDisplayWorldEventPanel(GameStateInfo gameState)
	{
		return gameState.UIManager.worldEventPanel_AfterBossUpgrade;
	}

	protected override void Init()
	{
		allAfterBossUpgrade = GenerateRootBonfireUpgrades(updateRecord: false);
		upgraded = new List<AfterBossUpgrade>();
		upgradeFinished = false;
		currencyRefresh = new CurrencyUpgradeRefreshModel(gameState, () => IsRefreshUnlocked && !upgradeFinished, () => refreshPrices, () => refreshMatter, delegate
		{
			allAfterBossUpgrade = GenerateRootBonfireUpgrades(updateRecord: false);
		}, "刷新始源祝福");
		adRefresh = new AdUpgradeRefreshModel(() => !upgradeFinished, () => adRefreshMaxCount, delegate
		{
			allAfterBossUpgrade = GenerateRootBonfireUpgrades(updateRecord: false);
		});
		base.Init();
	}

	protected override void SetupBeforeHealSpringUtilityChoices()
	{
		rootBlessChoice = new WorldEventChoice
		{
			buttonText = rootBlessChoiceText.choiceName,
			descriptionText = rootBlessChoiceText.choiceDescription,
			executeAction = delegate
			{
				gameState.UIManager.worldEventPanel_AfterBossUpgrade.ToBossUpgrade();
			}
		};
		currentDisplayInfo.AddChoice(rootBlessChoice);
		if (!gameState.IsIntroGame)
		{
			SetupActivateSkillChoice();
		}
	}

	protected override void SetupAfterHealSpringUtilityChoices()
	{
		if (!gameState.IsIntroGame)
		{
			SetupActivatePotentialTalentChoice();
		}
		SetupOfferingChoice();
	}

	private void SetupOfferingChoice()
	{
		if (!IsRefreshUnlocked && !(refreshUnlockClavis == null))
		{
			offeringChoice = new WorldEventChoice
			{
				buttonText = refreshUnlockClavis.GetMajorText(gameState.applicationState),
				descriptionText = refreshUnlockClavis.GetMinorText(gameState.applicationState),
				executeAbleChecker = () => gameState.applicationState.clavisManager.IsUnlocked(refreshUnlockClavis) && HasEnoughOfferingMatter(),
				executeAction = ExecuteOfferingChoice
			};
			currentDisplayInfo.AddChoice(offeringChoice);
		}
	}

	private bool HasEnoughOfferingMatter()
	{
		if (offeringMatter == null || offeringMatterCount <= 0)
		{
			return true;
		}
		return gameState.applicationState.matterManager.HasEnoughMatter(offeringMatter, offeringMatterCount);
	}

	private void ExecuteOfferingChoice()
	{
		if (refreshUnlockClavis == null || !gameState.applicationState.clavisManager.IsUnlocked(refreshUnlockClavis))
		{
			return;
		}
		if (!HasEnoughOfferingMatter())
		{
			gameState.UIManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_not_enough_matter);
			return;
		}
		if (offeringMatter != null && offeringMatterCount > 0)
		{
			gameState.applicationState.matterManager.ConsumeMatter(offeringMatter, offeringMatterCount, "特殊篝火献礼");
		}
		gameState.applicationState.clavisManager.ActivatedClavis(refreshUnlockClavis, silent: false, delegate
		{
			currentDisplayInfo.RemoveChoice(offeringChoice);
			currentDisplayInfo.description = offeringChoiceText.chosenDescription;
			SetPanelDirty();
		});
	}

	private List<AfterBossUpgrade> GenerateRootBonfireUpgrades(bool updateRecord)
	{
		List<int> willPowerCosts = new List<int> { 1, 2, 3 }.Shuffle(gameState.randomManager.NextDouble);
		return gameState.generationManager.GenerateRootBonfireBlessOptions(willPowerCosts, updateRecord).ConvertAll((Converter<Influence, AfterBossUpgrade>)((Influence b) => new BossBlessUpgrade(b)));
	}

	public void OnUpgradeTriggered(AfterBossUpgrade upgrade)
	{
		if (upgrade != null)
		{
			upgraded.Add(upgrade);
			upgradeFinished = true;
			if (rootBlessChoice != null)
			{
				currentDisplayInfo.RemoveChoice(rootBlessChoice);
			}
			if (rootBlessChoiceText != null)
			{
				currentDisplayInfo.description = rootBlessChoiceText.chosenDescription;
			}
			SetPanelDirty();
			if (upgrade is BossBlessUpgrade bossBlessUpgrade)
			{
				gameState.recordManager.generatedInfluence.AddWithoutDuplicate(bossBlessUpgrade.inf.def);
			}
		}
	}
}
