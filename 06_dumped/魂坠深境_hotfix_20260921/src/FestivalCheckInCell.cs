using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FestivalCheckInCell : CellUI<FestivalCheckInDayState>
{
	[SerializeField]
	private GlobalItemDisplay rewardDisplay;

	[SerializeField]
	private TextMeshProUGUI dateCountText;

	[SerializeField]
	private TextMeshProUGUI rewardNameText;

	[SerializeField]
	private GameObject completeMask;

	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private Sprite normalBackground;

	[SerializeField]
	private Sprite featuredBackground;

	[SerializeField]
	private List<int> featuredDayNumbers = new List<int> { 3, 7 };

	[NonSerialized]
	private Sequence sequence;

	public FestivalCheckInDayState info => base.setupData;

	protected override void Init()
	{
		InitUI(rewardDisplay);
		ResolveRewardNameText();
	}

	private TextMeshProUGUI ResolveRewardNameText()
	{
		if (rewardNameText != null)
		{
			return rewardNameText;
		}
		TextMeshProUGUI[] componentsInChildren = GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
		foreach (TextMeshProUGUI textMeshProUGUI in componentsInChildren)
		{
			if (textMeshProUGUI.name == "RewardNameText")
			{
				return textMeshProUGUI;
			}
		}
		return null;
	}

	protected override void OnSetup(FestivalCheckInDayState state)
	{
		rewardDisplay.Setup(state.Reward);
		dateCountText.text = (state.index + 1).ToString("00");
		RefreshRewardName();
		if (backgroundImage != null)
		{
			bool flag = featuredDayNumbers != null && featuredDayNumbers.Contains(state.index + 1);
			backgroundImage.sprite = (flag ? featuredBackground : normalBackground);
			Image component = completeMask.GetComponent<Image>();
			if (component != null)
			{
				component.sprite = backgroundImage.sprite;
				component.type = backgroundImage.type;
				component.preserveAspect = backgroundImage.preserveAspect;
			}
		}
	}

	private void RefreshRewardName()
	{
		if (base.setupData == null)
		{
			return;
		}
		TextMeshProUGUI textMeshProUGUI = ResolveRewardNameText();
		if (textMeshProUGUI != null)
		{
			GlobalRewardSetup reward = base.setupData.Reward;
			string text = reward.GlobalItemCellName;
			if (text.IsNullOrEmpty())
			{
				text = reward.GlobalItemTitle;
			}
			if (text.IsNullOrEmpty())
			{
				text = reward.GlobalItemRefSkill?.Name;
			}
			if (text.IsNullOrEmpty())
			{
				text = reward.GetRewardBrief();
			}
			textMeshProUGUI.text = text.ColorText(ScriptableObjectSingleton<GameDataManager>.instance.GetQualityBaseColor(reward.GlobalItemQuality));
		}
	}

	protected override void UpdateDisplay()
	{
		completeMask.gameObject.SetActive(info.IsGot);
	}

	public IEnumerator PlayGiveAnim(FestivalCheckInResult festivalCheckInResult)
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.Append(base.transform.DOScale(1.5f, 0.4f));
		sequence.Append(base.transform.DOScale(1f, 0.1f));
		sequence.AppendInterval(0.5f);
		sequence.Play();
		yield return sequence.WaitForKill();
		CellUI<FestivalCheckInDayState>.applicationState.fortressState.uiManager.globalRewardPackageDisplay.Setup(new GlobalRewardPackage(festivalCheckInResult.reward));
		SetDirty();
	}

	private void _OnGetButtonClicked()
	{
		if (!info.IsGot)
		{
			info.GiveReward(CellUI<FestivalCheckInDayState>.applicationState);
			SetDirty();
		}
	}
}
