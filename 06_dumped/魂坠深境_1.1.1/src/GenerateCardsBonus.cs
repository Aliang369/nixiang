using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public class GenerateCardsBonus : SelectGetItemBonus, PickBonusSelectionInfoPanel.IShowTextHint
{
	[SaveField(null, null)]
	public CardRewardType type;

	public override bool isChoose => true;

	public override Sprite PreviewIcon => ScriptableObjectSingleton<DungeonGenerationManager>.instance.itemBonusIcon;

	public override string previewText
	{
		get
		{
			CardRewardType cardRewardType = type;
			if (cardRewardType != CardRewardType.BossBattle && cardRewardType != CardRewardType.GuaranteedRare)
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_get_item;
			}
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_get_rare_item;
		}
	}

	string PickBonusSelectionInfoPanel.IShowTextHint.GetHint()
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_selectGetItemBonus_GenerateCardsBonus;
	}

	public GenerateCardsBonus(GameStateInfo gameState, CardRewardType type)
	{
		this.type = type;
		GenerateRelicItem(gameState);
	}

	private GenerateCardsBonus()
	{
	}

	private void GenerateRelicItem(GameStateInfo gameState)
	{
		int count = 3 + gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.relicBonusCountDelta);
		CardRewardType cardRewardType = type;
		QualityLevel qualityLevel = ((cardRewardType == CardRewardType.BossBattle || cardRewardType == CardRewardType.GuaranteedRare) ? QualityLevel.Rare : ((!gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.OnlyCommonCardBonusAfterBattle)) ? QualityLevel.None : QualityLevel.Common));
		cardRewardType = type;
		bool flag = cardRewardType == CardRewardType.NormalBattle || cardRewardType == CardRewardType.EliteBattle || cardRewardType == CardRewardType.BossBattle || cardRewardType == CardRewardType.AuxiliaryNormal;
		allItems = ((flag && qualityLevel != QualityLevel.Common) ? gameState.generationManager.GenerateCardRewardItems(count, gameState.difficultyState.CurrentStage, type) : gameState.generationManager.GenerateCharacterPoolItems(count, gameState.difficultyState.CurrentStage, qualityLevel));
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.ExtraRareCardBonus))
		{
			List<ItemDefinition> list = gameState.playerState.CharacterDef.attachClass.GetPoolItems(gameState.applicationState).FindAll((ItemDefinition it) => it.qualityLevel == QualityLevel.Rare);
			if (!list.IsNullOrEmpty())
			{
				ItemDefinition def = list.SelectRandom(gameState.randomManager.NextDouble);
				allItems.Add(new Item(def));
			}
		}
		if (!allItems.IsNullOrEmpty() && gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.GetBonusCardWithRune))
		{
			foreach (Item item in allItems)
			{
				RuneDef runeDef = gameState.generationManager.GetAllGenerateAbleRunes().FindAll((RuneDef r) => r.CheckCanAddRuneTo(item) == true).SelectRandom(gameState.randomManager.NextDouble);
				ChangeRuneAction action = new ChangeRuneAction(item, runeDef, runeDef.attachGenerationInfo.stack, null);
				gameState.actionManager.EnqueueAction(action);
			}
		}
		foreach (Item allItem in allItems)
		{
			ThinkingAnalyticsExtends.TrackExposeBonusCard(gameState, allItem, "战斗奖励", selectable: true);
		}
	}

	protected override void TriggerImpl(GameStateInfo gameState)
	{
		gameState.actionManager.EnqueueAction(new GatherMultiElementsAction(new MultiInfoDisplayPanel.SetupInfo
		{
			items = selectItems
		}));
		ConfirmGotBonus();
		ThinkingAnalyticsExtends.TrackBonusCard(gameState, selectItems.FirstOrDefault(), "战斗奖励");
	}
}
