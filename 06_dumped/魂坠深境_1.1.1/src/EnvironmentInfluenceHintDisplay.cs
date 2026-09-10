using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentInfluenceHintDisplay : UIBase
{
	public TextMeshProUGUI nameText;

	public TextMeshProUGUI descriptionText;

	public Image iconDisplay;

	public float tweenTime;

	public float stayTime;

	[NonSerialized]
	private Sequence sequence;

	private float SpeedScale => UIBase.gameState.actionManager.GetBattleScale();

	public IEnumerator ShowInfluenceCor(EnvironmentInfluence influence)
	{
		sequence?.Kill();
		nameText.text = influence.def.environmentName;
		descriptionText.text = influence.def.showDescription;
		iconDisplay.sprite = influence.def.icon;
		iconDisplay.color = Color.white;
		sequence = DOTween.Sequence();
		base.gameObject.SetActive(value: true);
		base.transform.localScale = new Vector3(1f, 0f, 1f);
		sequence.Append(base.transform.DOScale(new Vector3(1f, 0f, 1f), 0f));
		sequence.Append(base.transform.DOScale(new Vector3(1f, 1f, 1f), tweenTime / SpeedScale));
		sequence.AppendInterval(stayTime / SpeedScale);
		sequence.Append(base.transform.DOScale(new Vector3(1f, 0f, 1f), tweenTime / SpeedScale));
		sequence.AppendCallback(delegate
		{
			sequence = null;
			base.gameObject.SetActive(value: false);
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator HideInfluenceCor(EnvironmentInfluence influence)
	{
		sequence?.Kill();
		nameText.text = influence.def.environmentName;
		descriptionText.text = influence.def.hideDescription;
		iconDisplay.sprite = influence.def.icon;
		iconDisplay.color = Color.white;
		sequence = DOTween.Sequence();
		base.gameObject.SetActive(value: true);
		base.transform.localScale = new Vector3(1f, 0f, 1f);
		sequence.Append(base.transform.DOScale(new Vector3(1f, 0f, 1f), 0f));
		sequence.Append(base.transform.DOScale(new Vector3(1f, 1f, 1f), tweenTime / SpeedScale));
		sequence.AppendInterval(stayTime / SpeedScale);
		sequence.Append(base.transform.DOScale(new Vector3(1f, 0f, 1f), tweenTime / SpeedScale));
		sequence.AppendCallback(delegate
		{
			sequence = null;
			base.gameObject.SetActive(value: false);
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}
}
