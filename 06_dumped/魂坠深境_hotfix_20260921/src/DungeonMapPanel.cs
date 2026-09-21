using System;
using System.Collections.Generic;
using Common.UI;
using DG.Tweening;
using UnityEngine;

public class DungeonMapPanel : UIBase
{
	private static readonly Vector2 MapWillPowerProgressBarPosition = new Vector2(0f, -72f);

	private static readonly Vector2 NestActivityFeedbackPosition = new Vector2(0f, -96f);

	private const float MapWillPowerProgressBarScale = 0.84f;

	private const float NestActivityFeedbackHideDelay = 0.45f;

	[SerializeField]
	public DungeonMapDisplay dungeonMapDisplay;

	[SerializeField]
	private ProgressBarDisplay progressBarDisplay;

	[SerializeField]
	private WillPowerProgressBarDisplay willPowerProgressBar;

	[SerializeField]
	private WillPowerProgressBarDisplay willPowerProgressBarPrefab;

	[SerializeField]
	private List<CanvasGroup> mapPanels;

	[NonSerialized]
	private Sequence mapShowSequence;

	[NonSerialized]
	private WillPowerProgressBarDisplay nestActivityFeedbackBar;

	private bool ShouldShowWillPowerProgressBar
	{
		get
		{
			GameStateInfo gameStateInfo = UIBase.gameState;
			if (gameStateInfo == null)
			{
				return true;
			}
			return !gameStateInfo.IsIntroGame;
		}
	}

	protected override void Init()
	{
		base.gameObject.SetActive(value: false);
		InitUI(progressBarDisplay);
		CreateWillPowerProgressBarIfNeeded();
		InitUI(willPowerProgressBar);
		RefreshWillPowerProgressBarVisible();
		InitUI(dungeonMapDisplay);
		foreach (CanvasGroup mapPanel in mapPanels)
		{
			((Component)(object)mapPanel).gameObject.SetActive(value: false);
		}
	}

	public virtual void Setup(DungeonMap map)
	{
		progressBarDisplay.Setup();
		RefreshWillPowerProgressBarVisible();
		dungeonMapDisplay.Setup(map);
	}

	public virtual void Unset()
	{
		progressBarDisplay.Unset();
		dungeonMapDisplay.Unset();
	}

	public void PlayNestActivityReduced(int oldActivity, int newActivity, Item insecticide = null, int insecticideCost = 0)
	{
		WillPowerProgressBarDisplay willPowerProgressBarDisplay = ((willPowerProgressBarPrefab != null) ? willPowerProgressBarPrefab : willPowerProgressBar);
		if (oldActivity > newActivity && !(willPowerProgressBarDisplay == null) && !(UIBase.gameState?.UIManager?.uiVFXRoot == null))
		{
			DisposeNestActivityFeedbackBar();
			RectTransform uiVFXRoot = UIBase.gameState.UIManager.uiVFXRoot;
			nestActivityFeedbackBar = UnityEngine.Object.Instantiate(willPowerProgressBarDisplay, uiVFXRoot);
			nestActivityFeedbackBar.name = "NestActivityFeedback";
			RectTransform obj = nestActivityFeedbackBar.rectTransform;
			obj.position = GetCanvasPosition(new Vector2(0.5f, 1f), NestActivityFeedbackPosition);
			obj.localScale = Vector3.one;
			nestActivityFeedbackBar.ConfigureTemporaryNestActivityFeedback();
			nestActivityFeedbackBar.gameObject.SetActive(value: true);
			InitUI(nestActivityFeedbackBar);
			nestActivityFeedbackBar.ForceUpdate();
			nestActivityFeedbackBar.PlayNestActivityReduced(oldActivity, newActivity, delegate
			{
				DOVirtual.DelayedCall(0.45f, DisposeNestActivityFeedbackBar);
			});
		}
	}

	public void PlayNestActivityIncreased(int oldActivity, int newActivity)
	{
		WillPowerProgressBarDisplay willPowerProgressBarDisplay = ((willPowerProgressBarPrefab != null) ? willPowerProgressBarPrefab : willPowerProgressBar);
		if (oldActivity < newActivity && !(willPowerProgressBarDisplay == null) && !(UIBase.gameState?.UIManager?.uiVFXRoot == null))
		{
			DisposeNestActivityFeedbackBar();
			RectTransform uiVFXRoot = UIBase.gameState.UIManager.uiVFXRoot;
			nestActivityFeedbackBar = UnityEngine.Object.Instantiate(willPowerProgressBarDisplay, uiVFXRoot);
			nestActivityFeedbackBar.name = "NestActivityFeedback";
			RectTransform obj = nestActivityFeedbackBar.rectTransform;
			obj.position = GetCanvasPosition(new Vector2(0.5f, 1f), NestActivityFeedbackPosition);
			obj.localScale = Vector3.one;
			nestActivityFeedbackBar.ConfigureTemporaryNestActivityFeedback();
			nestActivityFeedbackBar.gameObject.SetActive(value: true);
			InitUI(nestActivityFeedbackBar);
			nestActivityFeedbackBar.ForceUpdate();
			nestActivityFeedbackBar.PlayNestActivityIncreased(oldActivity, newActivity, delegate
			{
				DOVirtual.DelayedCall(0.45f, DisposeNestActivityFeedbackBar);
			});
		}
	}

	public void ShowPanel()
	{
		TriggerEvent(UIEvent.OnPanelShow);
		if (UIBase.gameState.dungeonMapManager.inMapMode)
		{
			return;
		}
		UIBase.gameState.dungeonMapManager.inMapMode = true;
		RefreshWillPowerProgressBarVisible();
		if (ShouldShowWillPowerProgressBar)
		{
			willPowerProgressBar?.SetDirty();
		}
		SetDirty();
		foreach (CanvasGroup mapPanel in mapPanels)
		{
			((Component)(object)mapPanel).gameObject.SetActive(value: true);
		}
	}

	public void HidePanel()
	{
		if (!UIBase.gameState.dungeonMapManager.inMapMode)
		{
			return;
		}
		SetDirty();
		UIBase.gameState.dungeonMapManager.inMapMode = false;
		UIBase.applicationState.eventManager.UIHideDungeonMapEvent.TriggerEvent();
		SetDirty();
		foreach (CanvasGroup mapPanel in mapPanels)
		{
			((Component)(object)mapPanel).gameObject.SetActive(value: false);
		}
	}

	private void CreateWillPowerProgressBarIfNeeded()
	{
		if (!(willPowerProgressBar != null) && !(willPowerProgressBarPrefab == null) && !(progressBarDisplay == null))
		{
			RectTransform rectTransform = progressBarDisplay.transform as RectTransform;
			if (!(rectTransform == null) && !(rectTransform.parent == null))
			{
				willPowerProgressBar = UnityEngine.Object.Instantiate(willPowerProgressBarPrefab, rectTransform.parent);
				willPowerProgressBar.name = "WillPowerProgressBar_Runtime";
				RectTransform obj = willPowerProgressBar.rectTransform;
				obj.anchorMin = new Vector2(0.5f, 1f);
				obj.anchorMax = new Vector2(0.5f, 1f);
				obj.pivot = new Vector2(0.5f, 0.5f);
				obj.anchoredPosition = MapWillPowerProgressBarPosition;
				obj.localScale = Vector3.one * 0.84f;
				obj.SetSiblingIndex(rectTransform.GetSiblingIndex() + 1);
			}
		}
	}

	private void RefreshWillPowerProgressBarVisible()
	{
		if (willPowerProgressBar != null)
		{
			willPowerProgressBar.gameObject.SetActive(ShouldShowWillPowerProgressBar);
		}
	}

	private Vector3 GetCanvasPosition(Vector2 normalizedPosition, Vector2 offset = default(Vector2))
	{
		RectTransform rectTransform = ((Component)(object)UIBase.gameState?.UIManager?.mainCanvas)?.transform as RectTransform;
		if (rectTransform == null)
		{
			return Vector3.zero;
		}
		Rect rect = rectTransform.rect;
		Vector3 position = new Vector3(Mathf.Lerp(rect.xMin, rect.xMax, normalizedPosition.x) + offset.x, Mathf.Lerp(rect.yMin, rect.yMax, normalizedPosition.y) + offset.y);
		return rectTransform.TransformPoint(position);
	}

	private void DisposeNestActivityFeedbackBar()
	{
		if (!(nestActivityFeedbackBar == null))
		{
			DisposeUI(nestActivityFeedbackBar);
			UnityEngine.Object.Destroy(nestActivityFeedbackBar.gameObject);
			nestActivityFeedbackBar = null;
		}
	}
}
