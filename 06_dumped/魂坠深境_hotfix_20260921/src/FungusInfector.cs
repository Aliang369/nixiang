using System;
using System.Collections.Generic;
using UnityEngine;

public class FungusInfector : WorldEvent
{
	[SerializeField]
	private WorldEventChoiceText choice1Text;

	[SerializeField]
	private WorldEventChoiceText choice1_2Text;

	[SerializeField]
	private WorldEventChoiceText choice2Text;

	[SerializeField]
	[LocalizeText("事件文本")]
	private string choice2DancerChosenDescription;

	[SerializeField]
	private EnemyAssetRef enemyRef;

	[SerializeField]
	private int enemySetupIndex = -1;

	[SerializeField]
	private RuneDef bonusRuneDef;

	[SerializeField]
	private int HealthDelta;

	[NonSerialized]
	private Rune bonusRune;

	[NonSerialized]
	private WorldEventChoice choice1;

	[NonSerialized]
	private WorldEventChoice choice1_2;

	[NonSerialized]
	private WorldEventChoice choice2;

	private WorldEventDisplayType currentDisplayType;

	[SerializeField]
	private List<DialogueTrigger> dialogueTriggers;

	[NonSerialized]
	private WorldEventChoice leaveChoice;

	public override WorldEventDisplayType displayType => currentDisplayType;

	protected override void Init()
	{
		base.Init();
		currentDisplayType = WorldEventDisplayType.Default;
		bonusRune = new Rune(bonusRuneDef);
		choice1 = new WorldEventChoice();
		choice1.descriptionText = choice1Text.choiceDescription.FormatWith(bonusRune.GetNameWithValue());
		choice1.AddDisplayRune(bonusRune);
		choice1.buttonText = choice1Text.choiceName;
		choice1.executeAction = ExecuteAction1;
		currentDisplayInfo.AddChoice(choice1);
		choice1_2 = new WorldEventChoice();
		choice1_2.descriptionText = choice1_2Text.choiceDescription;
		choice1_2.buttonText = choice1_2Text.choiceName;
		choice1_2.executeAction = ExecuteAction1_2;
		choice2 = new WorldEventChoice();
		choice2.descriptionText = choice2Text.choiceDescription;
		choice2.buttonText = choice2Text.choiceName;
		choice2.executeAction = ExecuteAction2;
		currentDisplayInfo.AddChoice(choice2);
	}

	private void ExecuteAction1()
	{
		currentDisplayInfo.ClearChoice();
		currentDisplayInfo.AddChoice(choice1_2);
		currentDisplayInfo.description = choice1Text.chosenDescription;
		SetPanelDirty();
	}

	private void ExecuteAction1_2()
	{
		EncounterSetupInfo setupInfo = gameState.currentEncounter.setupInfo;
		setupInfo.fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.enterBattleEventTemplate;
		setupInfo.roomBonus = new BattleBonus();
		setupInfo.roomBonus.bonuses.Add(new RuneBonus(bonusRune));
		setupInfo.fixedEnemyInfoData = enemyRef.GetAssetPreData(gameState.applicationState.assetCatalog);
		if (enemySetupIndex != -1 && setupInfo.fixedEnemyInfoData.setups.Count > enemySetupIndex)
		{
			setupInfo.fixedEnemySetupIndex = enemySetupIndex;
			setupInfo.fixedEnemySetup = setupInfo.fixedEnemyInfoData.setups[enemySetupIndex];
		}
		FinishedWorldEvent(exitRoom: false);
		EnqueueAction(new TransitReloadRoomAction(setupInfo));
		EnqueueAction(new ActionAction(delegate
		{
			gameState.currentEncounter.enemyAI.component.dialogueTriggers.AddRange(dialogueTriggers);
		}));
	}

	private void ExecuteAction2()
	{
		EnqueueAction(new ChangeBftHealthAction(gameState.playerState.bft, HealthDelta, false));
		if (gameState.playerState.CharacterClass == CharacterClassType.Dancer)
		{
			RefreshAfterChoose(choice2DancerChosenDescription);
		}
		else
		{
			RefreshAfterChoose(choice2Text.chosenDescription);
		}
	}
}
