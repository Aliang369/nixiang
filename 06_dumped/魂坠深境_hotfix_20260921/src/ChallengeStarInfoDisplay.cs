using System;
using System.Collections;
using Common.UI;
using TMPro;
using UnityEngine;

public class ChallengeStarInfoDisplay : CellUI<ChallengeStar>, IShowAsync, IHideAsync
{
	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI outdateHintText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private TextMeshProUGUI confirmButtonText;

	[SerializeField]
	private TextMeshProUGUI unfinishedDifficultyBonusText;

	[SerializeField]
	private TextMeshProUGUI unlockInfo;

	[SerializeField]
	private GameObject finishBonusGotHint;

	[SerializeField]
	private RectTransform finishBonusRewardPivot;

	[SerializeField]
	private GlobalItemDisplay finishBonusRewardPrefab;

	[SerializeField]
	private GameButton confirmStartButton;

	[SerializeField]
	private GameButton infoButton;

	[SerializeField]
	private Animator animator;

	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	private CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> finishBonusPool;

	private ChallengeStar star => base.setupData;

	private StarMapPanel starMapPanel => CellUI<ChallengeStar>.fortressState.uiManager.starMapPanel;

	private ConstellationDetailPanel constellationDetailPanel => GetUIInParents<ConstellationDetailPanel>();

	protected override void Init()
	{
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<ChallengeStar>.applicationState, descriptionText);
		finishBonusPool = GetPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup>(finishBonusRewardPrefab, finishBonusRewardPivot);
		confirmStartButton.onClick.AddListener(_OnConfirmButtonClicked);
		infoButton.onClick.AddListener(delegate
		{
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(CellUI<ChallengeStar>.applicationState, "观星室-难度奖励查看", star.ContractDef.Name);
			constellationDetailPanel.constellationRewardDetailPanel.Setup(star.ContractDef);
		});
	}

	protected override void OnSetup(ChallengeStar _)
	{
		base.gameObject.SetActive(value: true);
		ContractDef contract = star.ContractDef;
		bool active = CellUI<ChallengeStar>.applicationState.contractManager.IsContractFinished(contract);
		ContractState contractState = CellUI<ChallengeStar>.applicationState.contractManager.contractStates.Find((ContractState c) => c.contractDef == contract);
		DifficultyLevel finishedMaxDifficultyThisCycle = star.setupData.finishedMaxDifficultyThisCycle;
		int num = Mathf.Max(0, (int)(finishedMaxDifficultyThisCycle - 1 + 1));
		int num2 = (int)(ScriptableObjectSingleton<GameStatisticData>.instance.topDifficulty - 1 + 1);
		titleText.text = contract.Name;
		descriptionText.text = contract.GetContractDescription(CellUI<ChallengeStar>.applicationState, out var generator);
		textLinkObjectDisplayer.SetGenerator(generator);
		if (contractState == null || !contractState.NextCycleResetTime.HasValue)
		{
			outdateHintText.gameObject.SetActive(value: false);
		}
		else
		{
			outdateHintText.gameObject.SetActive(value: true);
			outdateHintText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_expire_after_arg0_time.FormatWith(TextDisplayHelper.GetRoughTimeSpanText(contractState.NextCycleResetTime.Value.UtcDateTime - DateTime.UtcNow));
		}
		finishBonusGotHint.gameObject.SetActive(active);
		finishBonusPool.DespawnAll();
		foreach (GlobalRewardSetup item in contract.finishRewards.OrEmptyIfNull())
		{
			finishBonusPool.Spawn(item).Setup(item);
		}
		unfinishedDifficultyBonusText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_contract_got_finish_bonus_count_arg0_per_all_bonus_count_arg1.FormatWith(num, num2);
		unlockInfo.gameObject.SetActive(value: false);
		if (star.Unlocked)
		{
			confirmStartButton.interactable = true;
			confirmButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_start;
			return;
		}
		unlockInfo.gameObject.SetActive(value: true);
		if (DateTime.UtcNow < star.startDate.UtcDateTime)
		{
			unlockInfo.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_unlock_after_date.FormatWith(TextDisplayHelper.GetDateText(star.startDate.LocalDateTime));
		}
		else if (DateTime.UtcNow > star.endDate.UtcDateTime)
		{
			unlockInfo.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_challenge_end;
		}
		else
		{
			unlockInfo.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_unlock_after.FormatWith(star.GetPrevious()?.ContractDef?.Name);
		}
		confirmStartButton.interactable = false;
		confirmButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_not_unlocked;
	}

	IEnumerator IShowAsync.ShowAnim()
	{
		base.gameObject.SetActive(value: true);
		((Behaviour)(object)animator).enabled = true;
		animator.Play(AnimKeywords.show);
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled || !base.gameObject.activeInHierarchy);
	}

	IEnumerator IHideAsync.HideAnim()
	{
		((Behaviour)(object)animator).enabled = true;
		animator.Play(AnimKeywords.hide);
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled || !base.gameObject.activeInHierarchy);
		base.gameObject.SetActive(value: false);
	}

	private void _OnConfirmButtonClicked()
	{
		if (starMapPanel != null)
		{
			NewGameConfigData currentNewGameConfig = CellUI<ChallengeStar>.applicationState.newGameConfigManager.currentNewGameConfig;
			currentNewGameConfig.contract = star.ContractDef.GenerateContract(CellUI<ChallengeStar>.applicationState.assetCatalog, GetMaxSelectableChallengeDifficulty(currentNewGameConfig));
			starMapPanel.gameConfigPanel.Setup(currentNewGameConfig);
			manager.ShowUI(starMapPanel.gameConfigPanel);
		}
		GetComponentInParent<ConstellationDetailPanel>().gameObject.SetActive(value: false);
	}

	private DifficultyLevel GetMaxSelectableChallengeDifficulty(NewGameConfigData config)
	{
		CharacterClassDef characterClassDef = config.currentCharacter?.attachClass;
		DifficultyLevel difficultyLevel = ((characterClassDef != null) ? CellUI<ChallengeStar>.applicationState.idolManager.GetMainlineDifficulty(characterClassDef.mClass) : CellUI<ChallengeStar>.applicationState.difficultyManager.MaxUnlockDifficulty);
		if (difficultyLevel < DifficultyLevel.Normal)
		{
			difficultyLevel = DifficultyLevel.Normal;
		}
		DifficultyLevel difficultyLevel2 = DifficultyLevel.Normal;
		foreach (DifficultyInfo validAsset in CellUI<ChallengeStar>.applicationState.assetCatalog.difficultyCatalog.ValidAssets)
		{
			if (validAsset.difficultyLevel >= DifficultyLevel.Normal && validAsset.difficultyLevel <= difficultyLevel && star.ContractDef.CheckDifficultyLevelAble(validAsset) && validAsset.difficultyLevel > difficultyLevel2)
			{
				difficultyLevel2 = validAsset.difficultyLevel;
			}
		}
		return difficultyLevel2;
	}
}
