using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

[CreateAssetMenu(fileName = "勒索", menuName = "WorldEvent/Soul Stealer Extortion", order = 0)]
public class SoulStealerExtortionEvent : WorldEvent
{
	[SerializeField]
	private WorldEventChoiceText dealChoiceText;

	[SerializeField]
	private WorldEventChoiceText bargainChoiceText;

	[SerializeField]
	private WorldEventChoiceText fightChoiceText;

	[SerializeField]
	private WorldEventChoiceText emptyHandedChoiceText;

	[SerializeField]
	private EnemyAssetRef soulStealerEnemy;

	[SerializeField]
	private ItemDefinition soulStealerMask;

	[SerializeField]
	private int crystalReward = 6;

	[SerializeField]
	private int initialBargainPrice = 3;

	[SerializeField]
	private int bargainPriceIncrement = 2;

	[SerializeField]
	private int enemyLevelAdjustment = 2;

	[NonSerialized]
	private Item targetItem;

	[NonSerialized]
	private HashSet<ItemDefinition> viewedItemDefinitions;

	[NonSerialized]
	private int bargainPrice;

	protected override void Init()
	{
		base.Init();
		bargainPrice = initialBargainPrice;
		viewedItemDefinitions = new HashSet<ItemDefinition>();
		List<Item> deckItems = GetDeckItems();
		if (deckItems.Count == 0)
		{
			currentDisplayInfo.AddChoice(CreateEmptyHandedChoice());
			return;
		}
		targetItem = SelectPreferredTarget(deckItems);
		viewedItemDefinitions.Add(targetItem.GetDef());
		AddTradeChoices(currentDisplayInfo);
	}

	private List<Item> GetDeckItems()
	{
		return (from item in gameState.playerState.DeckContainer.GetAllItems()
			where item != null
			select item).ToList();
	}

	private Item SelectPreferredTarget(List<Item> candidates)
	{
		if (candidates == null || candidates.Count == 0)
		{
			return null;
		}
		List<Item> list = candidates;
		List<Item> list2 = list.FindAll((Item item) => !item.HasTag(ScriptableObjectSingleton<GameDataManager>.instance.curseTag));
		if (list2.Count > 0)
		{
			list = list2;
		}
		List<Item> list3 = list.FindAll((Item item) => !viewedItemDefinitions.Contains(item.GetDef()));
		if (list3.Count > 0)
		{
			list = list3;
		}
		return list.SelectRandom(gameState.randomManager.NextDouble);
	}

	private List<Item> GetAlternativeTargets()
	{
		return GetDeckItems().FindAll((Item item) => item != targetItem);
	}

	private void AddTradeChoices(WorldEventDisplayInfo displayInfo)
	{
		foreach (WorldEventChoice item in CreateTradeChoices())
		{
			displayInfo.AddChoice(item);
		}
	}

	private List<WorldEventChoice> CreateTradeChoices()
	{
		List<WorldEventChoice> list = new List<WorldEventChoice>();
		string arg = TextDataPreset.FormatItemName(gameState, targetItem, out var _);
		string arg2 = crystalReward.ToString().ColorText(ScriptableObjectSingleton<VFXDataPreset>.instance.crystalColor);
		WorldEventChoice worldEventChoice = new WorldEventChoice
		{
			buttonText = dealChoiceText.choiceName,
			descriptionText = string.Format(dealChoiceText.choiceDescription, arg, arg2),
			executeAction = CompleteDeal
		};
		worldEventChoice.AddDisplayItem(targetItem);
		list.Add(worldEventChoice);
		if (GetAlternativeTargets().Count > 0)
		{
			string arg3 = bargainPrice.ToString().ColorText(ScriptableObjectSingleton<VFXDataPreset>.instance.coinColor);
			string arg4 = bargainPriceIncrement.ToString().ColorText(ScriptableObjectSingleton<VFXDataPreset>.instance.coinColor);
			list.Add(new WorldEventChoice
			{
				buttonText = bargainChoiceText.choiceName,
				descriptionText = string.Format(bargainChoiceText.choiceDescription, arg3, arg4),
				executeAbleChecker = () => gameState.playerState.HasEnoughCoin(bargainPrice),
				executeAction = Bargain,
				stayAfterChoose = true
			});
		}
		list.Add(new WorldEventChoice
		{
			buttonText = fightChoiceText.choiceName,
			descriptionText = fightChoiceText.choiceDescription,
			executeAction = Fight
		});
		return list;
	}

	private WorldEventChoice CreateEmptyHandedChoice()
	{
		return new WorldEventChoice
		{
			buttonText = emptyHandedChoiceText.choiceName,
			descriptionText = emptyHandedChoiceText.choiceDescription,
			executeAction = LeaveEmptyHanded
		};
	}

	private void CompleteDeal()
	{
		EnqueueAction(new ExileItemAction(targetItem, this, null, ExileItemAction.ExileType.Permanent));
		EnqueueAction(new ChangeCrystalAction(crystalReward));
		RefreshAfterChoose(dealChoiceText.chosenDescription);
	}

	private void Bargain()
	{
		Item item = SelectPreferredTarget(GetAlternativeTargets());
		if (item != null)
		{
			EnqueueAction(new ChangeCoinAction(-bargainPrice));
			bargainPrice += bargainPriceIncrement;
			targetItem = item;
			viewedItemDefinitions.Add(targetItem.GetDef());
			RefreshAfterChoose(bargainChoiceText.chosenDescription, CreateTradeChoices(), showLeaveButton: false);
		}
	}

	private void Fight()
	{
		EncounterSetupInfo setupInfo = gameState.currentEncounter.setupInfo;
		setupInfo.fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.enterBattleEventTemplate;
		setupInfo.fixedEnemyInfoData = soulStealerEnemy.GetAssetPreData(gameState.applicationState.assetCatalog);
		setupInfo.fixedEnemySetup = null;
		setupInfo.enemySpawnType = EnemyEncounterType.Elite;
		setupInfo.enemyLevel += enemyLevelAdjustment;
		setupInfo.roomBonus = new BattleBonus();
		FinishBattleEncounterAction.AddEliteFightBonus(gameState, setupInfo.roomBonus, includeOptionalRewards: false);
		setupInfo.roomBonus.bonuses.Add(new SingleItemBonus(new Item(soulStealerMask)));
		gameState.playerBftDisplay.skipNextTurn = true;
		FinishedWorldEvent(exitRoom: false);
		EnqueueAction(new TransitReloadRoomAction(setupInfo));
	}

	private void LeaveEmptyHanded()
	{
		EnqueueGetMultiElementAction(new MultiInfoDisplayPanel.SetupInfo
		{
			items = new List<Item>
			{
				new Item(soulStealerMask)
			}
		});
		RefreshAfterChoose(emptyHandedChoiceText.chosenDescription);
	}
}
