using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

[CreateAssetMenu(fileName = "SpiderPotionChoiceEvent", menuName = "World Event/Spider/Potion Choice")]
public sealed class SpiderPotionChoiceEvent : WorldEvent, IAfterBossUpgradeSource, IUpgradeChoicePresentation
{
	public enum ChoiceMode
	{
		InitialPotion,
		PotionUpgrade
	}

	[Serializable]
	public sealed class UpgradeOptionConfig
	{
		public SpiderContractRuleDef.PotionUpgrade upgrade;

		public string title;

		[TextArea]
		public string description;

		public Sprite icon;
	}

	private sealed class PotionSelection : AfterBossUpgrade
	{
		private readonly SpiderContractRuleDef rule;

		private readonly SpiderContractRuleDef.PotionType potionType;

		private ItemDefinition Definition => rule.GetPotionDefinition(potionType);

		public override int WillPowerCost => 0;

		public override Sprite Icon => Definition?.Icon;

		public override string Title => Definition?.itemName ?? string.Empty;

		public PotionSelection(SpiderContractRuleDef rule, SpiderContractRuleDef.PotionType potionType)
		{
			this.rule = rule;
			this.potionType = potionType;
		}

		public override string GetDescription(GameStateInfo gameState, out TextDataGenerator generator)
		{
			generator = TextDataGenerator.Create(gameState, new Item(Definition));
			string itemDefinitionDisplayReferenceText = generator.GetItemDefinitionDisplayReferenceText(Definition);
			return "获得" + itemDefinitionDisplayReferenceText + "。";
		}

		public override void Trigger(GameStateInfo gameState, Action successFallback = null)
		{
			if (rule.SelectPotion(gameState, potionType))
			{
				successFallback?.Invoke();
			}
		}

		public override QualityLevel GetQuality()
		{
			return Definition?.qualityLevel ?? QualityLevel.Common;
		}

		public override bool CheckTriggerAble(GameStateInfo gameState)
		{
			return !rule.HasPotion(gameState);
		}
	}

	private sealed class PotionUpgradeSelection : AfterBossUpgrade
	{
		private readonly SpiderContractRuleDef rule;

		private readonly UpgradeOptionConfig config;

		public override int WillPowerCost => 0;

		public override Sprite Icon
		{
			get
			{
				Sprite icon = config.icon;
				if ((object)icon == null)
				{
					ItemDefinition insecticideOne = rule.insecticideOne;
					if ((object)insecticideOne == null)
					{
						return null;
					}
					icon = insecticideOne.Icon;
				}
				return icon;
			}
		}

		public override string Title => config.title;

		public PotionUpgradeSelection(SpiderContractRuleDef rule, UpgradeOptionConfig config)
		{
			this.rule = rule;
			this.config = config;
		}

		public override string GetDescription(GameStateInfo gameState, out TextDataGenerator generator)
		{
			Item potion = rule.GetPotion(gameState);
			generator = ((potion == null) ? null : TextDataGenerator.Create(gameState, potion));
			return generator?.Parse(config.description) ?? config.description;
		}

		public override void Trigger(GameStateInfo gameState, Action successFallback = null)
		{
			if (rule.ApplyPotionUpgrade(gameState, config.upgrade))
			{
				successFallback?.Invoke();
			}
		}

		public override QualityLevel GetQuality()
		{
			return QualityLevel.Common;
		}

		public override bool CheckTriggerAble(GameStateInfo gameState)
		{
			return rule.CanApplyPotionUpgrade(gameState, config.upgrade);
		}
	}

	public ChoiceMode mode;

	public List<UpgradeOptionConfig> upgradeOptions = new List<UpgradeOptionConfig>();

	[NonSerialized]
	private List<AfterBossUpgrade> allAfterBossUpgrade;

	[NonSerialized]
	private AdUpgradeRefreshModel adRefresh;

	[NonSerialized]
	private bool choiceFinished;

	public override WorldEventDisplayType displayType => WorldEventDisplayType.AfterBossUpgrade;

	public List<AfterBossUpgrade> AllAfterBossUpgrade => allAfterBossUpgrade;

	public UpgradeRefreshModel CurrencyRefresh => null;

	public UpgradeRefreshModel AdRefresh => adRefresh;

	public bool UsesWillPower => false;

	public bool ShowWillPowerProgress => false;

	public bool ShowWillPowerCost => false;

	public bool ShowBackButton => false;

	public bool AdRefreshRequiresRootBlessUnlock => false;

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
		choiceFinished = false;
		allAfterBossUpgrade = GenerateChoices();
		adRefresh = ((mode == ChoiceMode.PotionUpgrade) ? new AdUpgradeRefreshModel(() => !choiceFinished && gameState.applicationState.playerInfoManager.vip, () => ScriptableObjectSingleton<DungeonGenerationManager>.instance.rootBlessUpgradeConfig?.adRefreshMaxCount ?? 1, delegate
		{
			allAfterBossUpgrade = GenerateChoices();
		}) : null);
	}

	protected override void UpdateDisplayInfo(WorldEventDisplayInfo displayInfo)
	{
		displayInfo.title = EventName;
		displayInfo.description = EventDescription;
		displayInfo.showLeaveButton = false;
		displayInfo.ClearChoice();
	}

	public void OnUpgradeTriggered(AfterBossUpgrade upgrade)
	{
		if (upgrade != null && !choiceFinished)
		{
			choiceFinished = true;
			FinishedWorldEvent(exitRoom: false);
		}
	}

	private List<AfterBossUpgrade> GenerateChoices()
	{
		SpiderContractRuleDef rule = GetRule();
		if (rule == null)
		{
			return new List<AfterBossUpgrade>();
		}
		if (mode == ChoiceMode.InitialPotion)
		{
			return new SpiderContractRuleDef.PotionType[3]
			{
				SpiderContractRuleDef.PotionType.InsecticideOne,
				SpiderContractRuleDef.PotionType.InsecticideTwo,
				SpiderContractRuleDef.PotionType.InsecticideThree
			}.Where((SpiderContractRuleDef.PotionType potionType) => rule.GetPotionDefinition(potionType) != null).Select((Func<SpiderContractRuleDef.PotionType, AfterBossUpgrade>)((SpiderContractRuleDef.PotionType potionType) => new PotionSelection(rule, potionType))).ToList();
		}
		return ((IEnumerable<UpgradeOptionConfig>)(upgradeOptions ?? new List<UpgradeOptionConfig>()).Where((UpgradeOptionConfig option) => option != null && rule.CanApplyPotionUpgrade(gameState, option.upgrade)).ToList().SelectRandomCountUnique(3, gameState.randomManager.NextDouble)).Select((Func<UpgradeOptionConfig, AfterBossUpgrade>)((UpgradeOptionConfig option) => new PotionUpgradeSelection(rule, option))).ToList();
	}

	private SpiderContractRuleDef GetRule()
	{
		return gameState?.contractState?.selectedContract?.contractDef?.contractRule as SpiderContractRuleDef;
	}
}
