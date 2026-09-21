using System;
using System.Collections.Generic;
using GameRandom;
using UnityEngine;

public class DamagedSeal : WorldEvent
{
	[SerializeField]
	private WorldEventChoiceText choice1Text;

	[SerializeField]
	private WorldEventChoiceText choice1Text2;

	[SerializeField]
	private WorldEventChoiceText choice2Text;

	[SerializeField]
	private List<EnemyAssetRef> enemyRefs;

	[SerializeField]
	private int enemySetupIndex = -1;

	[SerializeField]
	private ItemDefinition bonusItem;

	[SerializeField]
	private int subHealth;

	[SerializeField]
	private int getCrystal;

	[NonSerialized]
	private WorldEventChoice choice1;

	[NonSerialized]
	private WorldEventChoice choice1_2;

	[NonSerialized]
	private WorldEventChoice choice2;

	private WorldEventDisplayType currentDisplayType;

	[NonSerialized]
	private WorldEventChoice leaveChoice;

	public override WorldEventDisplayType displayType => currentDisplayType;

	protected override void Init()
	{
		base.Init();
		currentDisplayType = WorldEventDisplayType.Default;
		choice1 = new WorldEventChoice();
		choice1.descriptionText = choice1Text.choiceDescription;
		choice1.buttonText = choice1Text.choiceName;
		choice1.executeAction = ExecuteAction1;
		choice1.AddDisplayItem(new Item(bonusItem));
		currentDisplayInfo.AddChoice(choice1);
		choice1_2 = new WorldEventChoice();
		choice1_2.descriptionText = choice1Text2.choiceDescription;
		choice1_2.buttonText = choice1Text2.choiceName;
		choice1_2.executeAction = ExecuteAction1_2;
		choice2 = new WorldEventChoice();
		choice2.descriptionText = choice2Text.choiceDescription;
		choice2.buttonText = choice2Text.choiceName;
		choice2.executeAction = Avoid;
		currentDisplayInfo.AddChoice(choice2);
	}

	private void ExecuteAction1()
	{
		currentDisplayInfo.ClearChoice();
		currentDisplayInfo.description = choice1Text.chosenDescription;
		EnqueueAction(new ActionAction(delegate
		{
			currentDisplayInfo.AddChoice(choice1_2);
			gameState.UIManager.currentEventPanel.SetDirty();
		}));
	}

	public void ExecuteAction1_2()
	{
		EncounterSetupInfo setupInfo = gameState.currentEncounter.setupInfo;
		setupInfo.fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.enterBattleEventTemplate;
		setupInfo.roomBonus = new BattleBonus();
		setupInfo.roomBonus.bonuses.Add(new SingleItemBonus(new Item(bonusItem)));
		setupInfo.fixedEnemyInfoData = enemyRefs.SelectRandom(gameState.randomManager.NextDouble).GetAssetPreData(gameState.applicationState.assetCatalog);
		if (enemySetupIndex != -1 && setupInfo.fixedEnemyInfoData.setups.Count > enemySetupIndex)
		{
			setupInfo.fixedEnemySetupIndex = enemySetupIndex;
			setupInfo.fixedEnemySetup = setupInfo.fixedEnemyInfoData.setups[enemySetupIndex];
		}
		FinishedWorldEvent(exitRoom: false);
		EnqueueAction(new TransitReloadRoomAction(setupInfo));
	}

	private void Avoid()
	{
		EnqueueAction(new ChangeBftMaxHealthAction(gameState.playerState.bft, -subHealth, baseAdd: true));
		EnqueueAction(new ChangeCrystalAction(getCrystal));
		RefreshAfterChoose(choice2Text.chosenDescription);
	}
}
