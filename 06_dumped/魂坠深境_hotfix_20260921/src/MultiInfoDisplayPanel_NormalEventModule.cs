using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MultiInfoDisplayPanel_NormalEventModule : MultiInfoDisplayPanel.DisplayModule
{
	[SerializeField]
	private RectTransform topConstraintPivot;

	[NonSerialized]
	private Sequence sequence;

	public override void Setup()
	{
		sequence?.Kill();
		base.display.displaysRoot.enabled = true;
		Vector3 position = parent.GetUIInParents<NormalEventPanel>().selected.transform.position + new Vector3(250f, 0f, 0f);
		if (position.y > topConstraintPivot.position.y)
		{
			position.y = topConstraintPivot.position.y;
		}
		base.display.displaysRoot.transform.position = position;
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.display.displaysRoot.RectTransform());
		base.display.displaysRoot.enabled = false;
		UIDisplayModule.gameState.StartCoroutine(OnShow);
	}

	public override void Unset()
	{
		UIDisplayModule.gameState.StartCoroutine(OnHide);
	}

	private IEnumerator OnShow()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		float x = base.display.itemInfoDisplayParent.position.x;
		float x2 = base.display.influenceInfoDisplayParent.position.x;
		float x3 = base.display.runeDisplayParent.position.x;
		float x4 = base.display.skillInfoDisplayParent.position.x;
		Vector3 position = base.display.itemInfoDisplayParent.position;
		position.x = 0f;
		base.display.itemInfoDisplayParent.position = position;
		position = base.display.influenceInfoDisplayParent.position;
		position.x = 0f;
		base.display.influenceInfoDisplayParent.position = position;
		position = base.display.runeDisplayParent.position;
		position.x = 0f;
		base.display.runeDisplayParent.position = position;
		position = base.display.skillInfoDisplayParent.position;
		position.x = 0f;
		base.display.skillInfoDisplayParent.position = position;
		sequence.Append(base.display.itemInfoDisplayParent.DOMoveX(x, 0.3f));
		sequence.Join(base.display.influenceInfoDisplayParent.DOMoveX(x2, 0.3f));
		sequence.Join(base.display.runeDisplayParent.DOMoveX(x3, 0.3f));
		sequence.Join(base.display.skillInfoDisplayParent.DOMoveX(x4, 0.3f));
		sequence.Join(DOTweenModuleUI.DOFade(base.display.itemInfoDisplayParent.GetComponent<CanvasGroup>(), 1f, 0.3f));
		sequence.Join(DOTweenModuleUI.DOFade(base.display.influenceInfoDisplayParent.GetComponent<CanvasGroup>(), 1f, 0.3f));
		sequence.Join(DOTweenModuleUI.DOFade(base.display.runeDisplayParent.GetComponent<CanvasGroup>(), 1f, 0.3f));
		sequence.Join(DOTweenModuleUI.DOFade(base.display.skillInfoDisplayParent.GetComponent<CanvasGroup>(), 1f, 0.3f));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	private IEnumerator OnHide()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.Append(base.display.itemInfoDisplayParent.DOMoveX(0f, 0.3f));
		sequence.Join(base.display.influenceInfoDisplayParent.DOMoveX(0f, 0.3f));
		sequence.Join(base.display.runeDisplayParent.DOMoveX(0f, 0.3f));
		sequence.Join(base.display.skillInfoDisplayParent.DOMoveX(0f, 0.3f));
		sequence.Join(DOTweenModuleUI.DOFade(base.display.itemInfoDisplayParent.GetComponent<CanvasGroup>(), 0f, 0.3f));
		sequence.Join(DOTweenModuleUI.DOFade(base.display.influenceInfoDisplayParent.GetComponent<CanvasGroup>(), 0f, 0.3f));
		sequence.Join(DOTweenModuleUI.DOFade(base.display.runeDisplayParent.GetComponent<CanvasGroup>(), 0f, 0.3f));
		sequence.Join(DOTweenModuleUI.DOFade(base.display.skillInfoDisplayParent.GetComponent<CanvasGroup>(), 0f, 0.3f));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			base.Unset();
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}
}
