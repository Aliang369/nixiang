using System;
using System.Collections.Generic;
using Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClavisHint : UIBase, IFrameUpdate
{
	public enum HintType
	{
		Unlock,
		Activate
	}

	public TextMeshProUGUI nameText;

	public Image icon;

	public TextMeshProUGUI descriptionText;

	public TextMeshProUGUI titleDescriptionText;

	[NonSerialized]
	private Sequence sequence;

	[NonSerialized]
	private (ClavisDef clavis, HintType type)? currentInfo;

	[NonSerialized]
	private Queue<(ClavisDef, HintType)> clavisHintQueue;

	[NonSerialized]
	private bool hiding;

	public bool WithClavisToHint
	{
		get
		{
			if (clavisHintQueue.Count == 0)
			{
				return currentInfo.HasValue;
			}
			return true;
		}
	}

	protected override void Init()
	{
		clavisHintQueue = new Queue<(ClavisDef, HintType)>();
		currentInfo = null;
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyAreaClicked);
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyAreaClicked);
	}

	public void Setup((ClavisDef, HintType) t)
	{
		currentInfo = t;
		SetDirty();
	}

	public void Unset()
	{
	}

	protected override void UpdateDisplay()
	{
		if (currentInfo.HasValue)
		{
			nameText.text = currentInfo.Value.clavis.NameWithColor;
			icon.sprite = currentInfo.Value.clavis.icon;
			switch (currentInfo.Value.type)
			{
			case HintType.Unlock:
				descriptionText.text = currentInfo.Value.clavis.storyDescription;
				titleDescriptionText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_unlock_clavis_for_activate;
				break;
			case HintType.Activate:
				descriptionText.text = currentInfo.Value.clavis.activatedHint;
				titleDescriptionText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_activate_clavis_effect;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public void EnqueueShowClavisHint(ClavisDef cl, HintType type)
	{
		clavisHintQueue.Enqueue((cl, type));
		TryShowNextHint();
	}

	public void FrameUpdate()
	{
		TryShowNextHint();
	}

	private void TryShowNextHint()
	{
		if (clavisHintQueue.Count > 0 && !currentInfo.HasValue)
		{
			(ClavisDef, HintType) tuple = clavisHintQueue.Dequeue();
			ShowHint(tuple.Item1, tuple.Item2);
		}
	}

	private void ShowHint(ClavisDef cl, HintType type)
	{
		Setup((cl, type));
		sequence?.Complete();
		base.gameObject.SetActive(value: true);
		sequence = DOTween.Sequence();
		sequence.AppendCallback(SetDirty).Append(DOTweenModuleUI.DOFade(GetComponent<CanvasGroup>(), 0f, 0f)).Append(DOTweenModuleUI.DOFade(GetComponent<CanvasGroup>(), 1f, 0.5f))
			.AppendCallback(delegate
			{
				sequence = null;
			})
			.Play();
		if (UIBase.gameState != null)
		{
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.ClavisHint);
		}
	}

	private void HideHint()
	{
		sequence?.Complete();
		sequence = DOTween.Sequence();
		sequence.Append(DOTweenModuleUI.DOFade(GetComponent<CanvasGroup>(), 1f, 0f)).Append(DOTweenModuleUI.DOFade(GetComponent<CanvasGroup>(), 0f, 0.5f)).AppendCallback(delegate
		{
			base.gameObject.SetActive(value: false);
			sequence = null;
			currentInfo = null;
			if (UIBase.gameState != null)
			{
				UIBase.gameState.UIManager.SetDirty();
			}
			if (UIBase.fortressState != null)
			{
				UIBase.fortressState.uiManager.SetDirty();
			}
			TryShowNextHint();
		})
			.Play();
		if (UIBase.gameState != null)
		{
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.ClavisHint);
		}
	}

	private void _OnEmptyAreaClicked()
	{
		if (sequence == null)
		{
			HideHint();
		}
	}
}
