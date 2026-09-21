using System;
using System.Collections.Generic;

public class RootBlessUpgradeEvent : WorldEvent, IAfterBossUpgradeSource
{
	[NonSerialized]
	private DungeonGenerationManager.RootBlessUpgradeConfig config;

	[NonSerialized]
	private List<AfterBossUpgrade> allAfterBossUpgrade;

	[NonSerialized]
	private readonly List<AfterBossUpgrade> upgraded = new List<AfterBossUpgrade>();

	[NonSerialized]
	private bool upgradeFinished;

	[NonSerialized]
	private CurrencyUpgradeRefreshModel currencyRefresh;

	[NonSerialized]
	private AdUpgradeRefreshModel adRefresh;

	public override WorldEventDisplayType displayType => WorldEventDisplayType.AfterBossUpgrade;

	public List<AfterBossUpgrade> AllAfterBossUpgrade => allAfterBossUpgrade;

	public UpgradeRefreshModel CurrencyRefresh => currencyRefresh;

	public UpgradeRefreshModel AdRefresh => adRefresh;

	public bool UpgradeFinished => upgradeFinished;

	private bool IsRefreshUnlocked
	{
		get
		{
			if (config != null)
			{
				if (!(config.refreshUnlockClavis != null) || !gameState.applicationState.clavisManager.IsActivated(config.refreshUnlockClavis))
				{
					return gameState.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableRootBonfireBlessRefresh);
				}
				return true;
			}
			return false;
		}
	}

	public void SetupConfig(DungeonGenerationManager.RootBlessUpgradeConfig rootBlessUpgradeConfig)
	{
		config = rootBlessUpgradeConfig;
	}

	public override WorldEventPanel GetDisplayWorldEventPanel(GameStateInfo gameState)
	{
		return gameState.UIManager.worldEventPanel_AfterBossUpgrade;
	}

	public override void FinishedWorldEvent()
	{
		FinishedWorldEvent(exitRoom: false);
	}

	protected override void Init()
	{
		base.Init();
		currentDisplayInfo.title = ScriptableObjectSingleton<TextDataPreset>.instance.term_root_bless_upgrade_title;
		currentDisplayInfo.description = ScriptableObjectSingleton<TextDataPreset>.instance.term_root_bless_upgrade_description;
		currentDisplayInfo.showLeaveButton = true;
		allAfterBossUpgrade = GenerateRootBlessUpgrades(updateRecord: false);
		upgraded.Clear();
		upgradeFinished = false;
		currencyRefresh = new CurrencyUpgradeRefreshModel(gameState, () => IsRefreshUnlocked && !upgradeFinished, () => config?.refreshPrices, () => config?.refreshMatter, delegate
		{
			allAfterBossUpgrade = GenerateRootBlessUpgrades(updateRecord: false);
		}, "刷新始源祝福");
		adRefresh = new AdUpgradeRefreshModel(() => !upgradeFinished && gameState.applicationState.playerInfoManager.vip, () => config?.adRefreshMaxCount ?? 1, delegate
		{
			allAfterBossUpgrade = GenerateRootBlessUpgrades(updateRecord: false);
		});
	}

	protected override void UpdateDisplayInfo(WorldEventDisplayInfo displayInfo)
	{
		displayInfo.title = ScriptableObjectSingleton<TextDataPreset>.instance.term_root_bless_upgrade_title;
		displayInfo.description = (upgradeFinished ? ScriptableObjectSingleton<TextDataPreset>.instance.term_root_bless_upgrade_finished_description : ScriptableObjectSingleton<TextDataPreset>.instance.term_root_bless_upgrade_description);
		displayInfo.showLeaveButton = true;
		displayInfo.ClearChoice();
	}

	private List<AfterBossUpgrade> GenerateRootBlessUpgrades(bool updateRecord)
	{
		List<int> willPowerCosts = new List<int> { 1, 2, 3 };
		return gameState.generationManager.GenerateRootBonfireBlessOptions(willPowerCosts, updateRecord).ConvertAll((Converter<Influence, AfterBossUpgrade>)((Influence b) => new BossBlessUpgrade(b)));
	}

	public void OnUpgradeTriggered(AfterBossUpgrade upgrade)
	{
		if (upgrade != null)
		{
			upgraded.Add(upgrade);
			upgradeFinished = true;
			SetPanelDirty();
			if (upgrade is BossBlessUpgrade bossBlessUpgrade)
			{
				gameState.recordManager.generatedInfluence.AddWithoutDuplicate(bossBlessUpgrade.inf.def);
			}
		}
	}
}
