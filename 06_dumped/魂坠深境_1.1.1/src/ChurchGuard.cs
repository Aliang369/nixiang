using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

[CreateAssetMenu(fileName = "ChurchGuard", menuName = "WorldEvent/ChurchGuard", order = 0)]
public class ChurchGuard : WorldEvent
{
	[SerializeField]
	private WorldEventChoiceText choice1Text;

	[SerializeField]
	private WorldEventChoiceText choice2Text;

	[SerializeField]
	private WorldEventChoiceText choice3Text;

	[SerializeField]
	private EnemyAssetRef enemyRef;

	[SerializeField]
	private int enemySetupIndex = -1;

	[SerializeField]
	private SkillDef bonusSkill;

	[SerializeField]
	private InfluenceDef influenceDef;

	[SerializeField]
	private int bribePrice = 6;

	[NonSerialized]
	private WorldEventChoice choice1;

	[NonSerialized]
	private WorldEventChoice choice2;

	[NonSerialized]
	private WorldEventChoice choice3;

	private WorldEventDisplayType currentDisplayType;

	[SerializeField]
	private List<DialogueTrigger> dialogueTriggers;

	[NonSerialized]
	private Influence influence;

	[NonSerialized]
	private WorldEventChoice leaveChoice;

	public override WorldEventDisplayType displayType => currentDisplayType;

	protected override void Init()
	{
		base.Init();
		currentDisplayType = WorldEventDisplayType.Default;
		influence = new Influence(influenceDef);
		choice1 = new WorldEventChoice();
		choice1.descriptionText = string.Format(choice1Text.choiceDescription, bribePrice, influence.def.InfluenceName);
		choice1.buttonText = choice1Text.choiceName;
		choice1.executeAbleChecker = () => gameState.playerState.HasEnoughCoin(bribePrice);
		choice1.AddDisplayInfluence(influence);
		choice1.executeAction = ExecuteAction1;
		currentDisplayInfo.AddChoice(choice1);
		choice2 = new WorldEventChoice();
		GameSkill gameSkill = new GameSkill(new ProfileSkill(bonusSkill, 0));
		choice2.descriptionText = choice2Text.choiceDescription.FormatWith(gameSkill.NameWithQualityColor);
		choice2.buttonText = choice2Text.choiceName;
		choice2.AddDisplaySkill(gameSkill);
		choice2.executeAction = ExecuteAction2;
		currentDisplayInfo.AddChoice(choice2);
		choice3 = new WorldEventChoice();
		choice3.descriptionText = choice3Text.choiceDescription;
		choice3.buttonText = choice3Text.choiceName;
		choice3.executeAction = ExecuteAction3;
		currentDisplayInfo.AddChoice(choice3);
	}

	private void ExecuteAction1()
	{
		EnqueueAction(new ChangeCoinAction(-bribePrice));
		EnqueueGetMultiElementAction(new MultiInfoDisplayPanel.SetupInfo
		{
			influences = new List<Influence> { influence }
		});
		RefreshAfterChoose(choice1Text.chosenDescription);
	}

	private void ExecuteAction2()
	{
		FinishedWorldEvent(exitRoom: false);
		EncounterSetupInfo setupInfo = gameState.currentEncounter.setupInfo;
		setupInfo.fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.enterBattleEventTemplate;
		setupInfo.roomBonus = new BattleBonus();
		setupInfo.roomBonus.bonuses.Add(new GetSkillBonus(new GameSkill(new ProfileSkill(bonusSkill, 0))));
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

	private void ExecuteAction3()
	{
		List<Item> allItems = gameState.playerState.DeckContainer.GetAllItems();
		if (allItems.Count >= 3)
		{
			List<Item> selected = new List<Item>();
			List<Item> selectPool = allItems.SelectRandomCountUnique(3, gameState.randomManager.NextDouble);
			SelectItemInfo info = new SelectItemInfo
			{
				pool = selectPool,
				panelHintGetter = () => choice3.descriptionText,
				output = selected,
				canConfirm = () => selected.Count == 1,
				canSelect = (Item _) => true,
				canCancel = () => false,
				shouldCancel = () => selectPool.Count < 1,
				singleSelect = true,
				onConfirm = delegate
				{
					Item item = selected.FirstOrDefault();
					EnqueueAction(new NotifyItemModificationAction(item));
					EnqueueAction(new RemoveItemAction(item, null));
					RefreshAfterChoose(choice3Text.chosenDescription);
				}
			};
			gameState.actionManager.EnqueueAction(new SelectItemAction(info, PlayerSelectItemsEffect.SelectPanelType.HorizontalSelect));
		}
		RefreshAfterChoose(choice3Text.chosenDescription);
	}
}
