using System;
using System.Collections.Generic;
using GameRandom;
using UnityEngine;

public class OldRraffleMachine : WorldEvent
{
	[SerializeField]
	[LocalizeText("事件文本")]
	private string warriorDescription;

	[SerializeField]
	[LocalizeText("事件文本")]
	private string getBonusDescription = "你抽到了奖励";

	[SerializeField]
	private WorldEventChoiceText choice1Text;

	[SerializeField]
	private WorldEventChoiceText choice2Text;

	[SerializeField]
	private RuneDef runeDef;

	[SerializeField]
	private int runeStack = 8;

	[SerializeField]
	private int price;

	[SerializeField]
	private List<BonusGenerationInfo> bgis;

	[NonSerialized]
	private WorldEventChoice choice1;

	[NonSerialized]
	private WorldEventChoice choice2;

	[NonSerialized]
	private WorldEventChoice choice3;

	[NonSerialized]
	private bool drawed;

	[NonSerialized]
	private WorldEventChoice leaveChoice;

	protected override void Init()
	{
		base.Init();
		if (gameState.playerState.CharacterClass == CharacterClassType.Warrior)
		{
			currentDisplayInfo.description = warriorDescription;
		}
		choice1 = new WorldEventChoice();
		choice1.descriptionText = choice1Text.choiceDescription;
		choice1.buttonText = choice1Text.choiceName;
		choice1.executeAbleChecker = () => gameState.playerState.HasEnoughCoin(price);
		choice1.executeAction = ExecuteAction1;
		currentDisplayInfo.AddChoice(choice1);
		choice2 = new WorldEventChoice();
		choice2.descriptionText = choice2Text.choiceDescription;
		choice2.buttonText = choice2Text.choiceName;
		choice2.AddDisplayRune(new Rune(runeDef, runeStack));
		choice2.executeAction = ExecuteAction2;
		currentDisplayInfo.AddChoice(choice2);
	}

	private void ExecuteAction1()
	{
		EnqueueAction(new ChangeCoinAction(-price));
		TriggerBonusAction action = new TriggerBonusAction(new BattleBonus(bgis.SelectRandom(gameState.randomManager.NextDouble).GenerateBonusObject(gameState))
		{
			title = getBonusDescription
		}, EndEncounterAfterBonus: false);
		gameState.actionManager.EnqueueAction(action);
		if (!drawed)
		{
			currentDisplayInfo.RemoveChoice(choice2);
			RefreshAfterChoose(choice1Text.chosenDescription);
			drawed = true;
		}
	}

	private void ExecuteAction2()
	{
		RefreshAfterChoose(choice2Text.chosenDescription);
		MintageRunePanel_Select.SetupInfo setupInfo = new MintageRunePanel_Select.SetupInfo(choice2.displayRunes);
		setupInfo.canBack = (Func<bool>)Delegate.Combine(setupInfo.canBack, (Func<bool>)(() => gameState.UIManager.mintageRunePanel_Select.GetRunes().Count == 0));
		gameState.UIManager.mintageRunePanel_Select.Setup(setupInfo);
	}
}
