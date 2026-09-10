using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameStatisticDisplayPanel : UIBase
{
	public TextMeshProUGUI gameCount;

	public TextMeshProUGUI winGameCount;

	public TextMeshProUGUI highestDifficulty;

	public TextMeshProUGUI currentWinStreak;

	public TextMeshProUGUI maxWinStreak;

	public TextMeshProUGUI defeatEnemyCount;

	public TextMeshProUGUI encounterEventCount;

	public TextMeshProUGUI damageCount;

	public TextMeshProUGUI castItemCount;

	public TextMeshProUGUI coinGetCount;

	public TextMeshProUGUI crystalGetCount;

	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI gameCountLabel;

	[SerializeField]
	private TextMeshProUGUI winGameCountLabel;

	[SerializeField]
	private TextMeshProUGUI highestDifficultyLabel;

	[SerializeField]
	private TextMeshProUGUI currentWinStreakLabel;

	[SerializeField]
	private TextMeshProUGUI maxWinStreakLabel;

	[SerializeField]
	private TextMeshProUGUI defeatEnemyCountLabel;

	[SerializeField]
	private TextMeshProUGUI encounterEventCountLabel;

	[SerializeField]
	private TextMeshProUGUI damageCountLabel;

	[SerializeField]
	private TextMeshProUGUI castItemCountLabel;

	[SerializeField]
	private TextMeshProUGUI coinGetCountLabel;

	[SerializeField]
	private TextMeshProUGUI crystalGetCountLabel;

	[SerializeField]
	private IconObjectFilterPanel characterClassFilterPanel;

	[SerializeField]
	private PlayerInfoPanel playerInfoPanel;

	protected override void Init()
	{
		if (playerInfoPanel != null)
		{
			InitUI(playerInfoPanel);
		}
		InitCharacterClassFilterPanel();
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		GameStatisticsManager gameStatisticsManager = UIBase.fortressState.applicationState.gameStatisticsManager;
		CharacterClassDef characterClassDef = characterClassFilterPanel?.selected?.setupData.target as CharacterClassDef;
		if (characterClassDef == null)
		{
			UpdateSummaryTexts(gameStatisticsManager.gameCount, gameStatisticsManager.winGameCount, GetDifficultyName(gameStatisticsManager.highestFinishedDifficulty), gameStatisticsManager.currentWinStreak, gameStatisticsManager.maxWinStreak, gameStatisticsManager.defeatEnemyCount, gameStatisticsManager.encounterEventCount, gameStatisticsManager.damageCount, gameStatisticsManager.castItemCount, gameStatisticsManager.coinGetCount, gameStatisticsManager.crystalGetCount);
			return;
		}
		GameStatisticsManager.PlayerClassStatisticsInfo playerClassStatisticsInfo = gameStatisticsManager.GetClassStatistics(characterClassDef) ?? new GameStatisticsManager.PlayerClassStatisticsInfo();
		UpdateSummaryTexts(playerClassStatisticsInfo.gameCount, playerClassStatisticsInfo.winGameCount, GetDifficultyName(playerClassStatisticsInfo.highestFinishedDifficulty), playerClassStatisticsInfo.currentWinStreak, playerClassStatisticsInfo.maxWinStreak, playerClassStatisticsInfo.defeatEnemyCount, playerClassStatisticsInfo.encounterEventCount, playerClassStatisticsInfo.damageCount, playerClassStatisticsInfo.castItemCount, playerClassStatisticsInfo.coinGetCount, playerClassStatisticsInfo.crystalGetCount);
	}

	private void InitCharacterClassFilterPanel()
	{
		if (!(characterClassFilterPanel == null))
		{
			InitUI(characterClassFilterPanel);
			List<IconObjectFilterPanel.IconFilterData> list = new List<IconObjectFilterPanel.IconFilterData>
			{
				new IconObjectFilterPanel.IconFilterData(null, ScriptableObjectSingleton<GameDataManager>.instance.generalClassIcon, ScriptableObjectSingleton<TextDataPreset>.instance.term_statistics_global)
			};
			list.AddRange(from data in UIBase.fortressState.applicationState.characterManager.GetAllCharacterClassIconFilterDatas(includeGeneral: false)
				where data.icon != null && data.target is CharacterClassDef characterClassDef && !characterClassDef.developingAssetFlag
				select data);
			characterClassFilterPanel.Sync(list);
			characterClassFilterPanel.canUnselect = false;
			characterClassFilterPanel.selected = characterClassFilterPanel.GetFilter(null);
			characterClassFilterPanel.onChangeSelected = SetDirty;
		}
	}

	private void UpdateSummaryTexts(int gameCountValue, int winGameCountValue, string highestDifficultyValue, int currentWinStreakValue, int maxWinStreakValue, int defeatEnemyCountValue, int encounterEventCountValue, long damageCountValue, long castItemCountValue, long coinGetCountValue, long crystalGetCountValue)
	{
		SetLabels();
		SetStatisticText(gameCount, gameCountValue);
		SetStatisticText(winGameCount, winGameCountValue);
		SetStatisticText(highestDifficulty, highestDifficultyValue);
		SetStatisticText(currentWinStreak, currentWinStreakValue);
		SetStatisticText(maxWinStreak, maxWinStreakValue);
		SetStatisticText(defeatEnemyCount, defeatEnemyCountValue);
		SetStatisticText(encounterEventCount, encounterEventCountValue);
		SetStatisticText(damageCount, damageCountValue);
		SetStatisticText(castItemCount, castItemCountValue);
		SetStatisticText(coinGetCount, coinGetCountValue);
		SetStatisticText(crystalGetCount, crystalGetCountValue);
	}

	private void SetLabels()
	{
		SetText(gameCountLabel, ScriptableObjectSingleton<TextDataPreset>.instance.term_statistics_game_count);
		SetText(winGameCountLabel, ScriptableObjectSingleton<TextDataPreset>.instance.term_statistics_win_count);
		SetText(highestDifficultyLabel, ScriptableObjectSingleton<TextDataPreset>.instance.term_statistics_highest_finished_difficulty);
		SetText(currentWinStreakLabel, ScriptableObjectSingleton<TextDataPreset>.instance.term_statistics_current_win_streak);
		SetText(maxWinStreakLabel, ScriptableObjectSingleton<TextDataPreset>.instance.term_statistics_max_win_streak);
		SetText(defeatEnemyCountLabel, ScriptableObjectSingleton<TextDataPreset>.instance.term_statistics_enemy_defeat_count);
		SetText(encounterEventCountLabel, ScriptableObjectSingleton<TextDataPreset>.instance.term_statistics_event_encounter_count);
		SetText(damageCountLabel, ScriptableObjectSingleton<TextDataPreset>.instance.term_statistics_damage_count);
		SetText(castItemCountLabel, ScriptableObjectSingleton<TextDataPreset>.instance.term_statistics_cast_item_count);
		SetText(coinGetCountLabel, ScriptableObjectSingleton<TextDataPreset>.instance.term_statistics_gain_coin_count);
		SetText(crystalGetCountLabel, ScriptableObjectSingleton<TextDataPreset>.instance.term_statistics_gain_crystal_count);
	}

	private void SetStatisticText(TextMeshProUGUI text, long value)
	{
		SetStatisticText(text, value.ToString());
	}

	private void SetStatisticText(TextMeshProUGUI text, string value)
	{
		SetText(text, value);
	}

	private void SetText(TextMeshProUGUI text, string value)
	{
		if (text != null)
		{
			text.text = value;
		}
	}

	private string GetDifficultyName(DifficultyLevel difficulty)
	{
		if (difficulty < DifficultyLevel.Normal)
		{
			return "-";
		}
		return DifficultyInfo.GetDifficultyInfo(difficulty, UIBase.fortressState.applicationState.assetCatalog)?.difficultyName ?? difficulty.ToString();
	}
}
