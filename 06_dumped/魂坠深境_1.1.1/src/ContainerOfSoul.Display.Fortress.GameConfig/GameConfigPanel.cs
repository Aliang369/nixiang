using System;
using System.Collections;
using Common.UI;
using ContainerOfSoul.Display.Fortress.ContractSelectDisplay;
using Spine.Unity;
using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.GameConfig;

public class GameConfigPanel : UIBase, IShowAsync, IHideAsync
{
	private const string ballIdleAnimName = "#dle#";

	private const string ballTouchAnimName = "#touch#";

	[SerializeField]
	public GameButton backButton;

	[SerializeField]
	private ConfigCharacterDisplay configCharacterDisplay;

	[SerializeField]
	private DifficultySelectionPanel difficultySelectionPanel;

	[SerializeField]
	private GameButton startGameButton;

	[SerializeField]
	private SkeletonGraphic ballAnimGraphic;

	[SerializeField]
	private RectTransform contractArea;

	[SerializeField]
	private PresetSkillSlotArea skillPresetArea;

	[SerializeField]
	private TextMeshProUGUI contractNameText;

	[SerializeField]
	private TextMeshProUGUI contractDescriptionText;

	[SerializeField]
	private GameButton toEditSkillButton;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private float startWaitBallAnimTime = 0.5f;

	[SerializeField]
	private CharacterStrengthenArea characterStrengthenArea;

	[SerializeField]
	private GameButton showHintPanelButton;

	[NonSerialized]
	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	[NonSerialized]
	private bool starting;

	[NonSerialized]
	public NewGameConfigData config;

	[NonSerialized]
	private FortressBaseGameUIPanel baseGameUIPanelParent;

	[SerializeField]
	private DailyChallengeTextPanel dailyChallengeGameplayPanel;

	[SerializeField]
	private DailyChallengeTextPanel dailyChallengeScorePanel;

	private SkillConfigPanel skillConfigPanel => UIBase.fortressState.uiManager.skillConfigPanel;

	protected override void Init()
	{
		InitUI(configCharacterDisplay);
		InitUI(difficultySelectionPanel);
		InitUI(skillPresetArea);
		startGameButton.onClick.AddListener(_OnStartGameButtonClicked);
		backButton.onClick.AddListener(_OnBackButtonClicked);
		toEditSkillButton.onClick.AddListener(_OnToEditSkillButtonClicked);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyAreaClicked);
		UIBase.applicationState.eventManager.SetNewGameConfigEvent.AddListener(_OnNewGameConfigUpdate);
		showHintPanelButton.onClick.AddListener(_OnShowHintPanelButtonClicked);
		baseGameUIPanelParent = GetUIInParents<FortressBaseGameUIPanel>();
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(UIBase.applicationState, contractDescriptionText, contractDescriptionText.rectTransform);
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.SetNewGameConfigEvent.RemoveListener(_OnNewGameConfigUpdate);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyAreaClicked);
	}

	public void Setup(NewGameConfigData config)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		this.config = config;
		UIBase.applicationState.newGameConfigManager.SanitizeLimitedDeploymentSelection(config);
		ballAnimGraphic.PlayAnimation("#dle#", loop: true);
		UIBase.applicationState.eventManager.enterFortressGameConfigPanel.TriggerEvent(config);
		characterStrengthenArea.Setup(config.GetDisplayCharacterState(UIBase.applicationState));
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_altar_activate);
		difficultySelectionPanel.Setup();
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		UIBase.applicationState.newGameConfigManager.SanitizeLimitedDeploymentSelection(config);
		DifficultyLevel? fixedDifficulty = config.contract.contractDef.FixedDifficulty;
		if (fixedDifficulty.HasValue)
		{
			config.contract.difficultyLevel = fixedDifficulty.Value;
		}
		ClampDifficultyToCurrentCharacter();
		PlayerCharacterState displayCharacterState = config.GetDisplayCharacterState(UIBase.applicationState);
		characterStrengthenArea.Setup(displayCharacterState);
		skillPresetArea.Setup(displayCharacterState.skillPreset);
		toEditSkillButton.gameObject.SetActive(!UIBase.applicationState.guideManager.IsPostFirstGameTalentFocusMode);
		contractArea.gameObject.SetActive(config.contract.contractDef != ScriptableObjectSingleton<FortressDataManager>.instance.defaultContractDef);
		contractNameText.text = config.contract.ContractName;
		contractDescriptionText.text = config.contract.GetContractDescription(UIBase.applicationState, out var generator);
		textLinkObjectDisplayer.SetGenerator(generator);
		DailyChallengeRunContext dailyChallengeContext = config.dailyChallengeContext;
		DailyChallengeScoreContractPresentation dailyChallengeScoreContractPresentation = ((dailyChallengeContext != null && dailyChallengeContext.isDailyChallenge) ? config.dailyChallengeContext.scoreCompetitionContract : null);
		bool flag = dailyChallengeScoreContractPresentation != null;
		ApplyDailyChallengeMode(flag);
		dailyChallengeGameplayPanel?.Setup(config.contract.ContractName, contractDescriptionText.text, flag);
		dailyChallengeScorePanel?.Setup(dailyChallengeScoreContractPresentation?.displayName ?? string.Empty, DailyChallengeDisplayText.GetScoreContractBodyText(dailyChallengeScoreContractPresentation), flag);
		difficultySelectionPanel.gameObject.SetActive(UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableDifficultySelection) && !fixedDifficulty.HasValue && config.contract.contractDef.contractType != ContractDef.ContractType.PVPOffender);
	}

	private void ApplyDailyChallengeMode(bool isDailyChallenge)
	{
		contractNameText.gameObject.SetActive(!isDailyChallenge);
		contractDescriptionText.gameObject.SetActive(!isDailyChallenge);
		dailyChallengeGameplayPanel?.gameObject.SetActive(isDailyChallenge);
		dailyChallengeScorePanel?.gameObject.SetActive(isDailyChallenge);
	}

	private void ClampDifficultyToCurrentCharacter()
	{
		if (config?.contract?.contractDef == null || config.contract.contractDef.IsDifficultyIsolated)
		{
			return;
		}
		CharacterClassDef characterClassDef = config.currentCharacter?.attachClass;
		if (characterClassDef == null)
		{
			return;
		}
		ContractDef.ContractType contractType = config.contract.contractDef.contractType;
		DifficultyLevel difficultyLevel;
		if (config.IsDailyChallenge)
		{
			difficultyLevel = UIBase.applicationState.idolManager.GetMaxSelectableMainlineDifficulty(characterClassDef.mClass);
		}
		else
		{
			switch (contractType)
			{
			default:
				return;
			case ContractDef.ContractType.BaseGame:
				difficultyLevel = UIBase.applicationState.idolManager.GetMaxSelectableMainlineDifficulty(characterClassDef.mClass);
				break;
			case ContractDef.ContractType.Challenge:
			case ContractDef.ContractType.HardChallenge:
			case ContractDef.ContractType.WeekShuffle:
			{
				DifficultyLevel mainlineDifficulty = UIBase.applicationState.idolManager.GetMainlineDifficulty(characterClassDef.mClass);
				difficultyLevel = ((mainlineDifficulty < DifficultyLevel.Normal) ? DifficultyLevel.Normal : mainlineDifficulty);
				break;
			}
			}
		}
		if (config.contract.difficultyLevel > difficultyLevel)
		{
			config.contract.difficultyLevel = difficultyLevel;
		}
	}

	IEnumerator IShowAsync.ShowUIAsync()
	{
		Bootstrap.instance.audioManager.PlaySoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_ambience_abyssal_stone);
		base.gameObject.SetActive(value: true);
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.show);
		yield return null;
		difficultySelectionPanel.OnShow();
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled);
	}

	IEnumerator IHideAsync.HideUIAsync()
	{
		Bootstrap.instance.audioManager.StopSoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_ambience_abyssal_stone);
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.hide);
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled);
		base.gameObject.SetActive(value: false);
		if (baseGameUIPanelParent != null)
		{
			baseGameUIPanelParent.gameObject.SetActive(value: false);
		}
	}

	private void StartGame()
	{
		if (!starting)
		{
			starting = true;
			StartCoroutine(StartGameCor());
		}
	}

	public void SetCurrentDifficulty(DifficultyLevel difficultyLevel)
	{
		difficultySelectionPanel.SetCurrentDifficulty(difficultyLevel);
	}

	private IEnumerator StartGameCor()
	{
		difficultySelectionPanel.ApplyCurrentDifficultyToConfig();
		UIBase.applicationState.newGameConfigManager.RememberDailyChallengeDifficulty(config);
		NewGameConfigData runConfig = UIBase.applicationState.newGameConfigManager.DetachDailyChallengeRunConfig(config);
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_altar_enter);
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.startGame);
		ballAnimGraphic.PlayAnimation("#touch#", loop: false);
		if (UIBase.applicationState.settingManager.uiTimeScaleTypeSetting != TimeScaleManager.TimeScaleType.X1)
		{
			yield return new WaitForSeconds(startWaitBallAnimTime);
		}
		UIBase.applicationState.newGameConfigManager.StartGame(runConfig);
		Bootstrap.instance.audioManager.StopAllSustained();
	}

	private void _OnStartGameButtonClicked()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		StartGame();
		Bootstrap.instance.audioManager.StopSoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_ambience_abyssal_stone);
	}

	private void _OnBackButtonClicked()
	{
		UIBase.applicationState.newGameConfigManager.RestorePersistentConfigAfterDailyChallenge(config);
		manager.HideUI(this);
		UIBase.fortressState.uiManager.SetDirty();
	}

	private void _OnNewGameConfigUpdate(NewGameConfigData config)
	{
		SetDirty();
	}

	private void _OnEmptyAreaClicked()
	{
		UIHelper.HideAllInfo(base.transform);
		SetDirty();
	}

	private void _OnToEditSkillButtonClicked()
	{
		if (!UIBase.applicationState.guideManager.IsPostFirstGameTalentFocusMode)
		{
			skillConfigPanel.Setup(config.GetSkillConfigCharacterState(UIBase.applicationState));
			skillConfigPanel.ShowUI();
		}
	}

	private void _OnShowHintPanelButtonClicked()
	{
		UIBase.fortressState.uiManager.fortressGuidePanel.gameConfigGuideHint.Show();
	}
}
