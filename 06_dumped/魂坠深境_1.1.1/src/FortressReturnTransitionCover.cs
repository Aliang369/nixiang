using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public sealed class FortressReturnTransitionCover : MonoBehaviour
{
	private CanvasGroup canvasGroup;

	private Transform loadingPanelTransform;

	private Tween fadeTween;

	private Action fadeCompletion;

	private bool suppressFadeKillCleanup;

	private Action timeoutCallback;

	private float timeoutAt;

	private bool timeoutArmed;

	public bool IsShowing => base.gameObject.activeSelf;

	public static FortressReturnTransitionCover Create(LoadingPanel loadingPanel)
	{
		if (loadingPanel == null || loadingPanel.transform.parent == null)
		{
			return null;
		}
		GameObject gameObject = new GameObject("FortressReturnTransitionCover", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
		gameObject.layer = loadingPanel.gameObject.layer;
		gameObject.transform.SetParent(loadingPanel.transform.parent, worldPositionStays: false);
		RectTransform component = gameObject.GetComponent<RectTransform>();
		component.anchorMin = Vector2.zero;
		component.anchorMax = Vector2.one;
		component.offsetMin = Vector2.zero;
		component.offsetMax = Vector2.zero;
		Image component2 = gameObject.GetComponent<Image>();
		component2.color = Color.black;
		component2.raycastTarget = true;
		FortressReturnTransitionCover fortressReturnTransitionCover = gameObject.AddComponent<FortressReturnTransitionCover>();
		fortressReturnTransitionCover.canvasGroup = gameObject.GetComponent<CanvasGroup>();
		fortressReturnTransitionCover.loadingPanelTransform = loadingPanel.transform;
		fortressReturnTransitionCover.PlaceBelowLoadingPanel();
		fortressReturnTransitionCover.HideImmediate();
		return fortressReturnTransitionCover;
	}

	public void Show(float timeoutSeconds, Action onTimeout)
	{
		KillFadeWithoutCompletion();
		fadeCompletion = null;
		timeoutCallback = onTimeout;
		timeoutArmed = timeoutSeconds > 0f;
		timeoutAt = Time.realtimeSinceStartup + Mathf.Max(0f, timeoutSeconds);
		PlaceBelowLoadingPanel();
		base.gameObject.SetActive(value: true);
		canvasGroup.alpha = 1f;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
	}

	public void HideWithFade(float duration, Action onHidden = null)
	{
		KillFadeWithoutCompletion();
		fadeCompletion = null;
		if (!base.gameObject.activeSelf || duration <= 0f)
		{
			HideImmediate();
			onHidden?.Invoke();
			return;
		}
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
		fadeCompletion = onHidden;
		bool finalized = false;
		Tween tween = null;
		tween = DOTweenModuleUI.DOFade(canvasGroup, 0f, duration).SetUpdate(isIndependentUpdate: true);
		fadeTween = tween;
		tween.OnComplete(FinishFade).OnKill(delegate
		{
			if (!suppressFadeKillCleanup)
			{
				FinishFade();
			}
		});
		void FinishFade()
		{
			if (!finalized)
			{
				finalized = true;
				if (fadeTween == tween)
				{
					fadeTween = null;
				}
				FinishHide(invokeCompletion: true);
			}
		}
	}

	public void HideImmediate()
	{
		KillFadeWithoutCompletion();
		FinishHide(invokeCompletion: false);
	}

	private void FinishHide(bool invokeCompletion)
	{
		Action obj = (invokeCompletion ? fadeCompletion : null);
		fadeCompletion = null;
		ClearTimeout();
		if ((UnityEngine.Object)(object)canvasGroup != null)
		{
			canvasGroup.alpha = 1f;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
		}
		if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
		}
		obj?.Invoke();
	}

	private void KillFadeWithoutCompletion()
	{
		Tween tween = fadeTween;
		fadeTween = null;
		if (tween == null)
		{
			return;
		}
		suppressFadeKillCleanup = true;
		try
		{
			tween.Kill();
		}
		finally
		{
			suppressFadeKillCleanup = false;
		}
	}

	private void Update()
	{
		if (!timeoutArmed || Time.realtimeSinceStartup < timeoutAt)
		{
			return;
		}
		Action action = timeoutCallback;
		ClearTimeout();
		try
		{
			action?.Invoke();
		}
		finally
		{
			if (base.gameObject.activeSelf)
			{
				KillFadeWithoutCompletion();
				FinishHide(invokeCompletion: true);
			}
		}
	}

	private void PlaceBelowLoadingPanel()
	{
		if (!(loadingPanelTransform == null) && !(base.transform.parent != loadingPanelTransform.parent))
		{
			int siblingIndex = loadingPanelTransform.GetSiblingIndex();
			int b = ((base.transform.GetSiblingIndex() < siblingIndex) ? (siblingIndex - 1) : siblingIndex);
			base.transform.SetSiblingIndex(Mathf.Max(0, b));
		}
	}

	private void ClearTimeout()
	{
		timeoutArmed = false;
		timeoutCallback = null;
		timeoutAt = 0f;
	}

	private void OnDestroy()
	{
		KillFadeWithoutCompletion();
		fadeCompletion = null;
		ClearTimeout();
	}
}
