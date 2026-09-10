using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ContractInfoDisplay : UIBase
{
	public TextMeshProUGUI nameText;

	public TextMeshProUGUI descriptionText;

	public RectTransform descriptionArea;

	public GameButton showDescriptionButton;

	public RectTransform descriptionAreaTopPivot;

	public RectTransform descriptionAreaBottomPivot;

	[NonSerialized]
	private bool showingDescription;

	[NonSerialized]
	private bool isDailyChallengeMode;

	[NonSerialized]
	private Tween switchDescriptionTween;

	[NonSerialized]
	protected TextLinkObjectDisplayer displayer;

	[SerializeField]
	private RectTransform dailyChallengeContractDropdown;

	[SerializeField]
	private DailyChallengeTextPanel dailyChallengeGameplayPanel;

	[SerializeField]
	private DailyChallengeTextPanel dailyChallengeScorePanel;

	protected override void Init()
	{
		displayer = new CommonTextLinkObjectDisplayer(UIBase.applicationState, descriptionText, base.rectTransform);
		descriptionArea.localPosition = descriptionAreaTopPivot.localPosition;
		showingDescription = false;
		showDescriptionButton.onClick.AddListener(_OnButtonClicked);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyAreaClicked);
		SetDirty();
	}

	protected override void Dispose()
	{
		switchDescriptionTween?.Kill();
		showDescriptionButton.onClick.RemoveListener(_OnButtonClicked);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyAreaClicked);
	}

	protected override void UpdateDisplay()
	{
		if (UIBase.gameState.contractState.IsChallengeContractGame)
		{
			base.gameObject.SetActive(value: true);
			nameText.text = UIBase.gameState.contractState.selectedContract.ContractName;
			descriptionText.text = UIBase.gameState.contractState.selectedContract.GetContractDescription(UIBase.applicationState, out var generator);
			if (UIBase.gameState.contractState.selectedContract.contractDef.contractRule is SpiderContractRuleDef spiderContractRuleDef)
			{
				descriptionText.text = "活性变化：" + FormatSignedValue(spiderContractRuleDef.GetNestActivityChange(UIBase.gameState)) + Environment.NewLine + "等级变化：" + FormatSignedValue(spiderContractRuleDef.GetNestEnemyLevelChange(UIBase.gameState));
			}
			displayer.SetGenerator(generator);
			NewGameConfigData newGameConfigData = UIBase.gameState.newGameConfigData;
			DailyChallengeScoreContractPresentation dailyChallengeScoreContractPresentation = ((newGameConfigData != null && newGameConfigData.dailyChallengeContext?.isDailyChallenge == true) ? UIBase.gameState.newGameConfigData.dailyChallengeContext.scoreCompetitionContract : null);
			bool flag = dailyChallengeScoreContractPresentation != null;
			if (flag)
			{
				nameText.text = UIBase.gameState.contractState.selectedContract.ContractName + " × " + dailyChallengeScoreContractPresentation.displayName;
			}
			ApplyDailyChallengeMode(flag);
			dailyChallengeGameplayPanel?.Setup(UIBase.gameState.contractState.selectedContract.ContractName, descriptionText.text, flag);
			dailyChallengeScorePanel?.Setup(dailyChallengeScoreContractPresentation?.displayName ?? string.Empty, DailyChallengeDisplayText.GetScoreContractBodyText(dailyChallengeScoreContractPresentation), flag);
		}
		else
		{
			base.gameObject.SetActive(value: false);
			ApplyDailyChallengeMode(isDailyChallenge: false);
			dailyChallengeGameplayPanel?.Setup("", "", active: false);
			dailyChallengeScorePanel?.Setup("", "", active: false);
		}
	}

	private void ApplyDailyChallengeMode(bool isDailyChallenge)
	{
		isDailyChallengeMode = isDailyChallenge;
		descriptionText.gameObject.SetActive(!isDailyChallenge);
		UpdateDailyChallengeDropdownVisibility();
	}

	private static string FormatSignedValue(int value)
	{
		if (value < 0)
		{
			return value.ToString();
		}
		return $"+{value}";
	}

	private void UpdateDailyChallengeDropdownVisibility()
	{
		dailyChallengeContractDropdown?.gameObject.SetActive(isDailyChallengeMode && showingDescription);
	}

	private void _OnEmptyAreaClicked()
	{
		ToHideDescription();
	}

	private void _OnButtonClicked()
	{
		if (showingDescription)
		{
			ToHideDescription();
		}
		else
		{
			ToShowDescription();
		}
	}

	private void ToHideDescription()
	{
		Tween tween = switchDescriptionTween;
		if (tween != null && tween.active && switchDescriptionTween.IsPlaying())
		{
			switchDescriptionTween.Complete();
		}
		showingDescription = false;
		switchDescriptionTween = descriptionArea.DOLocalMoveY(descriptionAreaTopPivot.localPosition.y, 0.5f).OnComplete(UpdateDailyChallengeDropdownVisibility).Play();
	}

	private void ToShowDescription()
	{
		Tween tween = switchDescriptionTween;
		if (tween != null && tween.active && switchDescriptionTween.IsPlaying())
		{
			switchDescriptionTween.Complete();
		}
		showingDescription = true;
		UpdateDailyChallengeDropdownVisibility();
		switchDescriptionTween = descriptionArea.DOLocalMoveY(descriptionAreaBottomPivot.localPosition.y, 0.5f).Play();
	}
}
