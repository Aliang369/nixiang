using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class MonthCheckCell : CellUI<MonthCheckInState>
{
	[SerializeField]
	private GlobalItemDisplay rewardDisplay;

	[SerializeField]
	private TextMeshProUGUI dateCountText;

	[SerializeField]
	private GameObject completeMask;

	[SerializeField]
	private GameObject lastMonthMask;

	[NonSerialized]
	private Sequence sequence;

	private MonthCheckInState state => base.setupData;

	protected override void Init()
	{
		InitUI(rewardDisplay);
	}

	protected override void OnSetup(MonthCheckInState state)
	{
		GlobalRewardSetup reward = state.reward;
		rewardDisplay.gameObject.SetActive(reward != null);
		if (reward != null)
		{
			rewardDisplay.Setup(reward);
		}
		dateCountText.text = ((state.displayIndex > 0) ? state.displayIndex.ToString() : ((state.index > 0) ? state.index.ToString() : ""));
	}

	protected override void UpdateDisplay()
	{
		if (completeMask != null)
		{
			completeMask.gameObject.SetActive(state.isCollected);
		}
		lastMonthMask.gameObject.SetActive(!state.isCurrentMonth);
	}

	public IEnumerator PlayGiveAnim(GlobalRewardInfo reward)
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.Append(base.transform.DOScale(1.5f, 0.4f));
		sequence.Append(base.transform.DOScale(1f, 0.1f));
		sequence.Play();
		yield return sequence.WaitForKill();
		CellUI<MonthCheckInState>.applicationState.fortressState.uiManager.globalRewardPackageDisplay.Setup(new GlobalRewardPackage(reward));
		SetDirty();
	}
}
