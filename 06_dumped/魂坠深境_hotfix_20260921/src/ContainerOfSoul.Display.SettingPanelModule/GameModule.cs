using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ContainerOfSoul.Display.SettingPanelModule;

public class GameModule : SettingPanel.DisplayModule
{
	[SerializeField]
	private GameObject challengeRecordPrefab;

	[SerializeField]
	private Transform challengePivot;

	[SerializeField]
	private TextMeshProUGUI contractDescription;

	[SerializeField]
	private TextMeshProUGUI difficultyDescription;

	[SerializeField]
	private TextMeshProUGUI progressDescription;

	[SerializeField]
	private GameButton continueButton;

	[SerializeField]
	public GameButton watchAdToRestartBattleButton;

	[SerializeField]
	public GameButton giveUpButton;

	[SerializeField]
	private GameButton openDifficultyDetailButton;

	[SerializeField]
	private GameButton closeDifficultyDetailButton;

	[SerializeField]
	private GameObject DifficultyDetailPanel;

	[SerializeField]
	private TextMeshProUGUI DifficultyDetailText;

	[SerializeField]
	private TextMeshProUGUI ChallengeName;

	[SerializeField]
	private TextMeshProUGUI ChallengeText;

	private bool restartingBattle;

	public override void Init()
	{
		base.Init();
		UIDisplayModule.gameState.applicationState.eventManager.achievementAchieveEvent.AddListener(_AchievementArchived);
		continueButton.onClick.AddListener(_ContinueGameButtonPressed);
		watchAdToRestartBattleButton.onClick.AddListener(_OnWatchAdToRestartBattleButtonClicked);
		giveUpButton.onClick.AddListener(_EndGameButtonPressed);
		openDifficultyDetailButton.onClick.AddListener(_OnOpenDifficultyDetailButtonClicked);
		closeDifficultyDetailButton.onClick.AddListener(_OnCloseDifficultyDetailButtonClicked);
	}

	public override void Dispose()
	{
		UIDisplayModule.gameState.applicationState.eventManager.achievementAchieveEvent.RemoveListener(_AchievementArchived);
	}

	public override void UpdateDisplay()
	{
		ChallengeName.text = UIDisplayModule.gameState.contractState.selectedContract.ContractName;
		ChallengeText.text = UIDisplayModule.gameState.contractState.selectedContract.GetContractDescription(UIDisplayModule.applicationState, out var _);
		DifficultyDetailText.text = UIDisplayModule.gameState.contractState.selectedContract.GetDifficultyInfo(UIDisplayModule.applicationState).GetFullDetailDescription(UIDisplayModule.applicationState);
		if (UIDisplayModule.applicationState.guideManager.IsAfterIntroFirstGame)
		{
			if (UIDisplayModule.applicationState.guideManager.IsAfterIntroProtect)
			{
				giveUpButton.interactable = false;
			}
			else
			{
				giveUpButton.interactable = true;
			}
		}
		watchAdToRestartBattleButton.gameObject.SetActive(value: false);
		if (UIDisplayModule.applicationState.guideManager.introFinished && UIDisplayModule.gameState.battleState.isInBattle)
		{
			watchAdToRestartBattleButton.gameObject.SetActive(value: true);
		}
	}

	private void Update()
	{
		contractDescription.text = "<size=150%>" + UIDisplayModule.gameState.contractState.selectedContract.ContractName + "</size>" + Environment.NewLine + UIDisplayModule.gameState.playerState.CharacterDef.attachClass.characterName + "-" + UIDisplayModule.gameState.playerState.character.def.characterName;
		progressDescription.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_game_record_progress_arg0.FormatWith(UIDisplayModule.gameState.dungeonMapManager.currentGlobalProgress) + Environment.NewLine + ScriptableObjectSingleton<TextDataPreset>.instance.term_game_record_time_arg0.FormatWith(new TimeSpan(UIDisplayModule.gameState.gameTimer.GetTime()).ToString("hh\\:mm\\:ss")) + Environment.NewLine;
		bool flag = UIDisplayModule.gameState.contractState.selectedContract?.contractDef?.contractRule is SpiderContractRuleDef;
		difficultyDescription.gameObject.SetActive(!flag);
		if (!flag)
		{
			difficultyDescription.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_difficulty_arg0.FormatWith(UIDisplayModule.gameState.applicationState.difficultyManager.GetDifficultyInfo(UIDisplayModule.gameState.contractState.selectedContract.difficultyLevel).difficultyName);
		}
	}

	private void AddChallengeItem(Achievement achievement)
	{
		GameObject obj = UnityEngine.Object.Instantiate(challengeRecordPrefab, challengePivot);
		obj.gameObject.SetActive(value: true);
		obj.GetComponentInChildren<TextMeshProUGUI>().text = achievement.achievementDef.achievementName + "-" + achievement.GetDescription(UIDisplayModule.applicationState, out var _);
	}

	private void _ContinueGameButtonPressed()
	{
		base.display.SetActiveAndDirtyWithFadeAnim(active: false);
	}

	private void _OnWatchAdToRestartBattleButtonClicked()
	{
		if (!restartingBattle)
		{
			restartingBattle = true;
			watchAdToRestartBattleButton.interactable = false;
			EventSystem.current?.SetSelectedGameObject(null);
			UIDisplayModule.applicationState.inited = false;
			UIDisplayModule.gameState.inited = false;
			UIDisplayModule.gameState.battleState.StopBattleState();
			UIDisplayModule.gameState.actionManager.Reset();
			UIDisplayModule.gameState.UIManager.StopAllCoroutines();
			Bootstrap.instance.Load(BootStrapLoadData.ToAfterLoginLoad(Bootstrap.instance.CurrentAccountID, dropCurrentGame: false));
		}
	}

	private void _EndGameButtonPressed()
	{
		UIDisplayModule.applicationState.savingManager.SetProfileDirtyToSave();
		base.gameObject.SetActive(value: false);
		UIDisplayModule.gameState.EndGame(success: false, finished: false);
		Bootstrap.instance.audioManager.StopSustained("BgmTag");
	}

	private void _AchievementArchived(Achievement achievement)
	{
		UIDisplayModule.applicationState.savingManager.SetProfileDirtyToSave();
		AddChallengeItem(achievement);
	}

	private void _OnOpenDifficultyDetailButtonClicked()
	{
		DifficultyDetailPanel.SetActive(value: true);
		base.display.SetDirty();
	}

	private void _OnCloseDifficultyDetailButtonClicked()
	{
		DifficultyDetailPanel.SetActive(value: false);
		base.display.SetDirty();
	}
}
