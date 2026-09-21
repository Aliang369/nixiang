using System;
using System.Collections.Generic;
using GameRandom;
using UnityEngine;

public class BonusLocationEvent : WorldEvent
{
	public enum BonusPanelType
	{
		None,
		Shell,
		Miner,
		Roulette,
		ChooseSequence
	}

	[SerializeField]
	[LocalizeText("事件文本")]
	public string introConfirmText;

	[SerializeField]
	[LocalizeText("事件文本")]
	public string introConfirmDescriptionText;

	[SerializeField]
	[LocalizeText("事件文本")]
	public string introDescription;

	[SerializeField]
	[TextArea]
	[LocalizeText("事件文本")]
	public string leaveDescription;

	[SerializeField]
	[TextArea]
	[LocalizeText("事件文本")]
	public string leaveWithoutBonusDescription;

	[SerializeField]
	[TextArea]
	[LocalizeText("事件文本")]
	public string refuseAndFightDescription;

	[SerializeField]
	[LocalizeText("事件文本")]
	public string fightConfirmText;

	[SerializeField]
	[LocalizeText("事件文本")]
	public string fightConfirmDescriptionText;

	public int majorCoin;

	public int majorCrystal;

	public int majorAddMaxHealth;

	public int majorCoin_StageAdd;

	public int majorCrystal_StageAdd;

	public int majorAddMaxHealth_StageAdd;

	public int minorCoin;

	public int minorCrystal;

	public int minorHealHealth;

	public int minorCoin_StageAdd;

	public int minorCrystal_StageAdd;

	public int minorHealHealth_StageAdd;

	public float runeBonusPossibility;

	public float runeBonusPossibility_StageAdd;

	public int upgradeCardStageThreshold;

	[NonSerialized]
	public bool gameSwitched;

	public bool IsBigBonusLocation
	{
		get
		{
			PathLocation eventCurrentLocation = base.EventCurrentLocation;
			if (eventCurrentLocation == null)
			{
				return false;
			}
			return eventCurrentLocation.variantType == PathLocation.LocationVariantType.BigBonusLocation;
		}
	}

	public BonusPanelType currentPanelType { get; private set; }

	public override WorldEventDisplayType displayType => WorldEventDisplayType.BonusLocation;

	public override WorldEventPanel GetDisplayWorldEventPanel(GameStateInfo gameState)
	{
		return gameState.UIManager.bonusLocationPanel;
	}

	protected override void Init()
	{
		base.Init();
		currentPanelType = GetAllBonusLocationPanels().SelectRandom(gameState.randomManager.NextDouble);
		currentDisplayInfo.AddChoice(new WorldEventChoice
		{
			buttonText = introConfirmText,
			descriptionText = introConfirmDescriptionText,
			executeAbleChecker = () => !gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.MintageRuneCantMintage),
			executeAction = delegate
			{
				currentDisplayInfo.ClearChoice();
				gameState.UIManager.bonusLocationPanel.OpenSelect();
			}
		});
	}

	public void SwitchToRandomBonusPanel()
	{
		gameSwitched = true;
		currentPanelType = GetAllBonusLocationPanels().FindAll((BonusPanelType p) => p != currentPanelType).SelectRandom(gameState.randomManager.NextDouble);
		SwitchToBonusPanel(currentPanelType);
	}

	public void SwitchToBonusPanel(BonusPanelType type)
	{
		gameState.UIManager.bonusLocationPanel.HideCurrentGamePanel();
		currentPanelType = type;
		gameState.UIManager.bonusLocationPanel.ShowCurrentGamePanel();
	}

	private List<BonusPanelType> GetAllBonusLocationPanels()
	{
		return new List<BonusPanelType>
		{
			BonusPanelType.Roulette,
			BonusPanelType.ChooseSequence,
			BonusPanelType.Shell,
			BonusPanelType.Miner
		};
	}

	public List<BonusLocationBonus> GenerateRandomBonus()
	{
		List<BonusLocationBonus> list = new List<BonusLocationBonus>();
		List<BattleBonusObject> list2 = new List<BattleBonusObject>();
		int count = majorCoin + majorCoin_StageAdd * gameState.difficultyState.CurrentStage;
		int count2 = majorCrystal + majorCrystal_StageAdd * gameState.difficultyState.CurrentStage;
		int healthCount = majorAddMaxHealth + majorAddMaxHealth_StageAdd * gameState.difficultyState.CurrentStage;
		int count3 = minorCoin + minorCoin_StageAdd * gameState.difficultyState.CurrentStage;
		int count4 = minorCrystal + minorCrystal_StageAdd * gameState.difficultyState.CurrentStage;
		int healCount = minorHealHealth + minorHealHealth_StageAdd * gameState.difficultyState.CurrentStage;
		list2.Add(new CoinBonus(count));
		list2.Add(new CrystalBonus(count2));
		list2.Add(new AddMaxHealthBonus(healthCount));
		list2.Add(new AddInfluenceBonus(gameState.generationManager.GenerateRandomCommonBless(1).ConvertAll((InfluenceDef ifd) => new Influence(ifd)), gameState));
		float num = runeBonusPossibility + runeBonusPossibility_StageAdd * (float)gameState.difficultyState.CurrentStage;
		List<BattleBonusObject> list3 = new List<BattleBonusObject>();
		if (gameState.randomManager.NextFloat(0f, 1f) < num)
		{
			list3.AddRange(gameState.generationManager.GenerateMintageRunes(2).ConvertAll((Rune r) => new RuneBonus(r)));
		}
		if (list3.Count < 2)
		{
			list3.AddRange(gameState.generationManager.GenerateCardRewardItems(2 - list3.Count, gameState.difficultyState.CurrentStage, CardRewardType.Event).ConvertAll(delegate(Item it)
			{
				if (gameState.difficultyState.CurrentStage >= upgradeCardStageThreshold && !it.upgraded)
				{
					it.Upgrade(gameState);
				}
				return new SingleItemBonus(it, SingleItemBonus.GetItemBonusType.BonusGame);
			}));
		}
		List<List<BattleBonusObject>> list4 = new List<List<BattleBonusObject>>();
		list4.Add(new List<BattleBonusObject>
		{
			new CoinBonus(count3),
			new CoinBonus(count3),
			new CoinBonus(count3)
		});
		list4.Add(new List<BattleBonusObject>
		{
			new CrystalBonus(count4),
			new CrystalBonus(count4),
			new CrystalBonus(count4)
		});
		list4.Add(new List<BattleBonusObject>
		{
			new HealBonus(healCount),
			new HealBonus(healCount),
			new HealBonus(healCount)
		});
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.EnableBonusLocationGameExtraRareBonus) || IsBigBonusLocation)
		{
			list.AddRange(list2.SelectRandomCount(3, gameState.randomManager.NextDouble).ConvertAll((BattleBonusObject b) => new BonusLocationBonus(b, rare: true)));
			list.AddRange(list3.ConvertAll((BattleBonusObject it) => new BonusLocationBonus(it, rare: false)));
			list.AddRange(list4.SelectRandom(gameState.randomManager.NextDouble).SelectRandomCount(1, gameState.randomManager.NextDouble).ConvertAll((BattleBonusObject b) => new BonusLocationBonus(b, rare: false)));
		}
		else
		{
			list.Add(new BonusLocationBonus(list2.SelectRandom(gameState.randomManager.NextDouble), rare: true));
			list.AddRange(list3.ConvertAll((BattleBonusObject it) => new BonusLocationBonus(it, rare: false)));
			list.AddRange(list4.SelectRandom(gameState.randomManager.NextDouble).ConvertAll((BattleBonusObject b) => new BonusLocationBonus(b, rare: false)));
			list = list.Shuffle(gameState.randomManager.NextDouble);
		}
		return list;
	}
}
