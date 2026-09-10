using System;
using System.Collections;
using DG.Tweening;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class PowerDisplay : CellUI<PowerInfo>
{
	private const string animStr_ToYang = "YinToYang";

	private const string animStr_ToYin = "YangToYin";

	private const string animStr_Yang = "YangIdle";

	private const string animStr_Yin = "YinIdle";

	[SerializeField]
	private SkeletonGraphic skeletonGraphic;

	[SerializeField]
	private Image outline;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[NonSerialized]
	public bool hasAnim;

	[NonSerialized]
	private int index;

	[NonSerialized]
	private DG.Tweening.Sequence sequence;

	[NonSerialized]
	private int targetMaxSlot;

	[NonSerialized]
	private int targetSlotIndex;

	[NonSerialized]
	private bool temp;

	[NonSerialized]
	private TrigramsDisplay trigramsDisplay;

	protected override void OnSetup(PowerInfo dailyContract)
	{
		trigramsDisplay = CellUI<PowerInfo>.gameState.UIManager.trigramsDisplay;
		targetMaxSlot = -1;
		targetSlotIndex = -1;
		index = -1;
		hasAnim = false;
		canvasGroup.alpha = 0f;
		outline.gameObject.SetActive(value: false);
		temp = false;
	}

	protected override void OnUnset()
	{
		sequence?.Kill();
		sequence = null;
		hasAnim = false;
	}

	protected override void UpdateDisplay()
	{
		if (CellUI<PowerInfo>.gameState.battleState.isInBattle && RefreshIndex() && index >= 0 && targetSlotIndex >= 0 && targetMaxSlot >= targetSlotIndex)
		{
			outline.gameObject.SetActive(temp);
			CellUI<PowerInfo>.gameState.StartCoroutine(PlayTween_Shift);
		}
	}

	public void UpdateDisplayImmediately(TrigramsManager.YinYangType type)
	{
		RefreshIndex();
		this.RectTransform().anchoredPosition = new Vector2(GetTargetPosition(), this.RectTransform().anchoredPosition.y);
		PlayAnim((type == TrigramsManager.YinYangType.Yang) ? "YangIdle" : "YinIdle");
		canvasGroup.alpha = (temp ? 0.8f : 1f);
		outline.gameObject.SetActive(temp);
		CellUI<PowerInfo>.gameState.StartCoroutine(PlayTween_Shift);
	}

	public IEnumerator PlayTween_Add(TrigramsManager.YinYangType type)
	{
		RefreshIndex();
		RectTransform rectTransform = this.RectTransform();
		rectTransform.anchoredPosition = new Vector2(rectTransform.parent.RectTransform().GetWidth() * ((type == TrigramsManager.YinYangType.Yang) ? (-0.5f) : 0.5f), rectTransform.anchoredPosition.y);
		PlayAnim((type == TrigramsManager.YinYangType.Yang) ? "YangIdle" : "YinIdle");
		CellUI<PowerInfo>.gameState.StartCoroutine(PlayTween_Shift);
		yield break;
	}

	public IEnumerator PlayTween_Remove(TrigramsManager.YinYangType type)
	{
		RefreshIndex();
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = CellUI<PowerInfo>.gameState.actionManager.GetBattleScale();
		float num = base.transform.parent.RectTransform().GetWidth() * ((type == TrigramsManager.YinYangType.Yang) ? (-0.5f) : 0.5f);
		sequence.Append(DOTweenModuleUI.DOAnchorPosX(this.RectTransform(), num, 0.5f, false));
		sequence.Join(DOTweenModuleUI.DOFade(canvasGroup, 0f, 0.5f));
		DG.Tweening.Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator PlayTween_Convert(TrigramsManager.YinYangType type)
	{
		RefreshIndex();
		PlayAnim((type == TrigramsManager.YinYangType.Yang) ? "YangToYin" : "YinToYang");
		CellUI<PowerInfo>.gameState.StartCoroutine(PlayTween_Shift);
		yield break;
	}

	private IEnumerator PlayTween_Shift()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = CellUI<PowerInfo>.gameState.actionManager.GetBattleScale();
		outline.gameObject.SetActive(temp);
		float targetPosition = GetTargetPosition();
		sequence.Append(DOTweenModuleUI.DOAnchorPosX(this.RectTransform(), targetPosition, 0.5f, false));
		sequence.Join(DOTweenModuleUI.DOFade(canvasGroup, temp ? 0.8f : 1f, 0.5f));
		DG.Tweening.Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	private TrackEntry PlayAnim(string animName)
	{
		return skeletonGraphic.PlayAnimation(animName, loop: false);
	}

	private bool RefreshIndex()
	{
		TrigramsManager trigramsManager = CellUI<PowerInfo>.gameState.trigramsManager;
		int num = trigramsManager.maxPower + 1;
		int displayIndex = trigramsDisplay.GetDisplayIndex(this);
		int num2 = displayIndex;
		if (num2 >= (int)trigramsManager.yangPower)
		{
			num2++;
		}
		bool flag = displayIndex < trigramsManager.turnMaxYangPower || displayIndex > trigramsManager.maxPower - trigramsManager.turnMaxYinPower - 1;
		if (targetMaxSlot == num && targetSlotIndex == num2 && temp == flag && index == displayIndex)
		{
			return false;
		}
		targetSlotIndex = num2;
		targetMaxSlot = num;
		index = displayIndex;
		temp = flag;
		return true;
	}

	private float GetTargetPosition()
	{
		int num = 60;
		float width = base.transform.parent.RectTransform().GetWidth();
		if (width < (float)((targetMaxSlot + 1) * num))
		{
			float num2 = width;
			float num3 = num2 / (float)(targetMaxSlot + 1);
			return (float)(targetSlotIndex + 1) * num3 - num2 * 0.5f;
		}
		int num4 = (targetMaxSlot + 1) * num;
		int num5 = num;
		return (float)((targetSlotIndex + 1) * num5) - (float)num4 * 0.5f;
	}
}
