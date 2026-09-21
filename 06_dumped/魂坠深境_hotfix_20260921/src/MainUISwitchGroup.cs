using System;
using System.Collections.Generic;
using ContainerOfSoul.Display.Fortress;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class MainUISwitchGroup : UIBase
{
	private struct ButtonTextStyle
	{
		public Color color;

		public bool enableVertexGradient;

		public VertexGradient colorGradient;
	}

	[SerializeField]
	public FortressMainUIPanel.MainPanelLayerSetup[] mainPanelLayerSetups;

	[SerializeField]
	private float switchTweenTime = 0.8f;

	[SerializeField]
	private float sourceFadeOutTime = 0.2f;

	[SerializeField]
	private float targetFadeInTime = 0.2f;

	[SerializeField]
	private Ease switchTweenEase = Ease.OutCubic;

	[SerializeField]
	private float switchSlideDistance = 300f;

	[NonSerialized]
	public FortressMainUIPanel.MainPanelLayerSetup currentLayerSetups;

	[NonSerialized]
	private Sequence switchTweenSequence;

	[NonSerialized]
	private readonly Dictionary<TextMeshProUGUI, ButtonTextStyle> buttonTextStyles = new Dictionary<TextMeshProUGUI, ButtonTextStyle>();

	public bool IsSwitching { get; private set; }

	protected override void Init()
	{
		FortressMainUIPanel.MainPanelLayerSetup[] array = mainPanelLayerSetups;
		foreach (FortressMainUIPanel.MainPanelLayerSetup panelLayerSetup in array)
		{
			panelLayerSetup.button.group = this;
			CacheButtonTextStyle(panelLayerSetup);
			ResetLayerPanel(panelLayerSetup);
			SetLayerPanelInteraction(panelLayerSetup, enabled: false);
			panelLayerSetup.layerPanel.gameObject.SetActive(value: false);
			if (panelLayerSetup.layer == FortressMainUIPanel.MainPanelLayer.MainRoom)
			{
				currentLayerSetups = panelLayerSetup;
				panelLayerSetup.layerPanel.gameObject.SetActive(value: true);
				SetLayerPanelInteraction(panelLayerSetup, enabled: true);
			}
			panelLayerSetup.button.onClick.AddListener(delegate
			{
				TrySwitchTo(panelLayerSetup, animated: true, showLockedHint: true);
			});
		}
		RefreshButtonTextColors();
	}

	protected override void UpdateDisplay()
	{
		FortressMainUIPanel.MainPanelLayerSetup[] array = mainPanelLayerSetups;
		foreach (FortressMainUIPanel.MainPanelLayerSetup mainPanelLayerSetup in array)
		{
			if (mainPanelLayerSetup.lockImage != null)
			{
				mainPanelLayerSetup.lockImage.gameObject.SetActive(mainPanelLayerSetup.isLocked);
			}
		}
		RefreshButtonTextColors();
	}

	protected override void Dispose()
	{
		StopSwitchAnimation();
		SetSwitchingState(isSwitching: false);
	}

	public bool TrySwitchToLayer(FortressMainUIPanel.MainPanelLayer targetLayer, bool animated, bool showLockedHint = false)
	{
		FortressMainUIPanel.MainPanelLayerSetup targetSetup = Array.Find(mainPanelLayerSetups, (FortressMainUIPanel.MainPanelLayerSetup setup) => setup != null && setup.layer == targetLayer);
		return TrySwitchTo(targetSetup, animated, showLockedHint);
	}

	private bool TrySwitchTo(FortressMainUIPanel.MainPanelLayerSetup targetSetup, bool animated, bool showLockedHint)
	{
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		if (targetSetup == null || IsSwitchAnimationPlaying())
		{
			return false;
		}
		if (targetSetup.isLocked)
		{
			if (showLockedHint)
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(targetSetup.lockedHint);
			}
			currentLayerSetups.button.Select();
			RefreshButtonTextColors();
			SetDirty();
			return false;
		}
		if (currentLayerSetups == targetSetup)
		{
			return true;
		}
		UIBase.fortressState.uiManager.universalCommonInfoPanel.HideAll();
		FortressMainUIPanel.MainPanelLayerSetup sourceSetup = currentLayerSetups;
		StopSwitchAnimation();
		SetSwitchingState(isSwitching: true);
		ResetAllLayerPanels();
		currentLayerSetups = targetSetup;
		currentLayerSetups.button.GetComponent<ImageAlphaTransition>().OnButtonClick();
		UIBase.fortressState.fortressRoomManager.SetRoom(targetSetup.roomRef);
		FortressMainUIPanel.MainPanelLayerSetup[] array = mainPanelLayerSetups;
		foreach (FortressMainUIPanel.MainPanelLayerSetup mainPanelLayerSetup in array)
		{
			if (mainPanelLayerSetup != targetSetup)
			{
				mainPanelLayerSetup.button.StateTransition(ButtonSelectionState.Normal, instant: false);
			}
		}
		RefreshButtonTextColors();
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_switch_layer);
		manager.SetDirty();
		TrackSwitchTo(targetSetup);
		if (animated)
		{
			PlaySwitchAnimation(sourceSetup, targetSetup);
		}
		else
		{
			CompleteSwitch(targetSetup);
		}
		return true;
	}

	private void PlaySwitchAnimation(FortressMainUIPanel.MainPanelLayerSetup sourceSetup, FortressMainUIPanel.MainPanelLayerSetup targetSetup)
	{
		RectTransform layerPanelRect = GetLayerPanelRect(sourceSetup);
		RectTransform layerPanelRect2 = GetLayerPanelRect(targetSetup);
		if (layerPanelRect == null || layerPanelRect2 == null || switchTweenTime <= 0f)
		{
			CompleteSwitch(targetSetup);
			return;
		}
		int incomingDirection = GetIncomingDirection(sourceSetup, targetSetup);
		sourceSetup.layerPanel.gameObject.SetActive(value: true);
		targetSetup.layerPanel.gameObject.SetActive(value: true);
		SetLayerPanelInteraction(sourceSetup, enabled: false);
		SetLayerPanelInteraction(targetSetup, enabled: false);
		layerPanelRect.anchoredPosition = Vector2.zero;
		layerPanelRect2.anchoredPosition = Vector2.zero;
		CanvasGroup layerPanelCanvasGroup = GetLayerPanelCanvasGroup(sourceSetup);
		CanvasGroup layerPanelCanvasGroup2 = GetLayerPanelCanvasGroup(targetSetup);
		layerPanelCanvasGroup.alpha = 1f;
		layerPanelCanvasGroup2.alpha = 0f;
		float num = Mathf.Max(0f, switchSlideDistance);
		Vector2 anchoredPosition = Vector2.up * ((float)incomingDirection * num);
		Vector2 vector = Vector2.up * ((float)(-incomingDirection) * num);
		layerPanelRect2.anchoredPosition = anchoredPosition;
		Sequence sequence = DOTween.Sequence();
		float num2 = Mathf.Clamp(sourceFadeOutTime, 0f, switchTweenTime);
		float num3 = Mathf.Clamp(targetFadeInTime, 0f, switchTweenTime);
		switchTweenSequence = sequence;
		sequence.Append(DOTweenModuleUI.DOAnchorPos(layerPanelRect, vector, switchTweenTime, false).SetEase(switchTweenEase));
		sequence.Join(DOTweenModuleUI.DOAnchorPos(layerPanelRect2, Vector2.zero, switchTweenTime, false).SetEase(switchTweenEase));
		sequence.Join(DOTweenModuleUI.DOFade(layerPanelCanvasGroup, 0f, num2).SetEase(switchTweenEase));
		sequence.Join(DOTweenModuleUI.DOFade(layerPanelCanvasGroup2, 1f, num3).SetEase(switchTweenEase));
		Sequence sequence2 = sequence;
		sequence2.onComplete = (TweenCallback)Delegate.Combine(sequence2.onComplete, (TweenCallback)delegate
		{
			CompleteSwitch(targetSetup);
			if (switchTweenSequence == sequence)
			{
				switchTweenSequence = null;
			}
		});
		Sequence sequence3 = sequence;
		sequence3.onKill = (TweenCallback)Delegate.Combine(sequence3.onKill, (TweenCallback)delegate
		{
			if (switchTweenSequence == sequence)
			{
				switchTweenSequence = null;
			}
		});
		sequence.Play();
	}

	private bool IsSwitchAnimationPlaying()
	{
		if (switchTweenSequence != null && switchTweenSequence.IsActive())
		{
			return !switchTweenSequence.IsComplete();
		}
		return false;
	}

	private void CompleteSwitch(FortressMainUIPanel.MainPanelLayerSetup targetSetup)
	{
		FortressMainUIPanel.MainPanelLayerSetup[] array = mainPanelLayerSetups;
		foreach (FortressMainUIPanel.MainPanelLayerSetup mainPanelLayerSetup in array)
		{
			ResetLayerPanel(mainPanelLayerSetup);
			bool active = mainPanelLayerSetup == targetSetup;
			mainPanelLayerSetup.layerPanel.gameObject.SetActive(active);
			SetLayerPanelInteraction(mainPanelLayerSetup, active);
		}
		SetSwitchingState(isSwitching: false);
		UIBase.applicationState?.eventManager?.fortressMainPanelLayerChangedEvent.TriggerEvent(targetSetup.layer);
	}

	private int GetIncomingDirection(FortressMainUIPanel.MainPanelLayerSetup sourceSetup, FortressMainUIPanel.MainPanelLayerSetup targetSetup)
	{
		float buttonScreenY = GetButtonScreenY(sourceSetup);
		float buttonScreenY2 = GetButtonScreenY(targetSetup);
		if (!Mathf.Approximately(buttonScreenY, buttonScreenY2))
		{
			if (!(buttonScreenY2 > buttonScreenY))
			{
				return -1;
			}
			return 1;
		}
		if (sourceSetup == null)
		{
			return 1;
		}
		if (targetSetup.layer >= sourceSetup.layer)
		{
			return -1;
		}
		return 1;
	}

	private float GetButtonScreenY(FortressMainUIPanel.MainPanelLayerSetup setup)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (setup?.button == null)
		{
			return 0f;
		}
		Canvas componentInParent = setup.button.GetComponentInParent<Canvas>();
		return RectTransformUtility.WorldToScreenPoint(((UnityEngine.Object)(object)componentInParent != null && (int)componentInParent.renderMode != 0) ? componentInParent.worldCamera : null, setup.button.transform.position).y;
	}

	private RectTransform GetLayerPanelRect(FortressMainUIPanel.MainPanelLayerSetup setup)
	{
		if (!(setup?.layerPanel != null))
		{
			return null;
		}
		return setup.layerPanel.transform as RectTransform;
	}

	private CanvasGroup GetLayerPanelCanvasGroup(FortressMainUIPanel.MainPanelLayerSetup setup)
	{
		CanvasGroup val = setup.layerPanel.GetComponent<CanvasGroup>();
		if ((UnityEngine.Object)(object)val == null)
		{
			val = setup.layerPanel.AddComponent<CanvasGroup>();
		}
		return val;
	}

	private void ResetAllLayerPanels()
	{
		FortressMainUIPanel.MainPanelLayerSetup[] array = mainPanelLayerSetups;
		foreach (FortressMainUIPanel.MainPanelLayerSetup mainPanelLayerSetup in array)
		{
			ResetLayerPanel(mainPanelLayerSetup);
			SetLayerPanelInteraction(mainPanelLayerSetup, enabled: false);
			mainPanelLayerSetup.layerPanel.gameObject.SetActive(value: false);
		}
	}

	private void SetLayerPanelInteraction(FortressMainUIPanel.MainPanelLayerSetup setup, bool enabled)
	{
		if (!(setup?.layerPanel == null))
		{
			CanvasGroup layerPanelCanvasGroup = GetLayerPanelCanvasGroup(setup);
			layerPanelCanvasGroup.interactable = true;
			layerPanelCanvasGroup.blocksRaycasts = enabled;
		}
	}

	private void SetSwitchingState(bool isSwitching)
	{
		if (IsSwitching == isSwitching)
		{
			return;
		}
		IsSwitching = isSwitching;
		List<FortressNPC> list = UIBase.fortressState?.uiManager?.mainUIPanel?.fortressNpcs;
		if (list == null)
		{
			return;
		}
		foreach (FortressNPC item in list)
		{
			if (item != null)
			{
				item.SetRoomSwitchDialogueSuppressed(isSwitching);
			}
		}
	}

	private void ResetLayerPanel(FortressMainUIPanel.MainPanelLayerSetup setup)
	{
		RectTransform layerPanelRect = GetLayerPanelRect(setup);
		if (layerPanelRect != null)
		{
			layerPanelRect.anchoredPosition = Vector2.zero;
		}
		if (setup?.layerPanel != null)
		{
			GetLayerPanelCanvasGroup(setup).alpha = 1f;
		}
	}

	private TextMeshProUGUI GetButtonText(FortressMainUIPanel.MainPanelLayerSetup setup)
	{
		if (setup == null)
		{
			return null;
		}
		if (setup.buttonText == null && setup.button != null)
		{
			setup.buttonText = setup.button.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
		}
		return setup.buttonText;
	}

	private void CacheButtonTextStyle(FortressMainUIPanel.MainPanelLayerSetup setup)
	{
		TextMeshProUGUI buttonText = GetButtonText(setup);
		if (!(buttonText == null) && !buttonTextStyles.ContainsKey(buttonText))
		{
			buttonTextStyles.Add(buttonText, new ButtonTextStyle
			{
				color = buttonText.color,
				enableVertexGradient = buttonText.enableVertexGradient,
				colorGradient = buttonText.colorGradient
			});
		}
	}

	private void RefreshButtonTextColors()
	{
		FortressMainUIPanel.MainPanelLayerSetup[] array = mainPanelLayerSetups;
		foreach (FortressMainUIPanel.MainPanelLayerSetup mainPanelLayerSetup in array)
		{
			TextMeshProUGUI buttonText = GetButtonText(mainPanelLayerSetup);
			if (!(buttonText == null))
			{
				CacheButtonTextStyle(mainPanelLayerSetup);
				ButtonTextStyle value;
				if (mainPanelLayerSetup == currentLayerSetups)
				{
					buttonText.enableVertexGradient = false;
					buttonText.color = Color.white;
				}
				else if (buttonTextStyles.TryGetValue(buttonText, out value))
				{
					buttonText.color = value.color;
					buttonText.enableVertexGradient = value.enableVertexGradient;
					buttonText.colorGradient = value.colorGradient;
				}
			}
		}
	}

	private void StopSwitchAnimation()
	{
		if (switchTweenSequence != null && switchTweenSequence.IsActive())
		{
			switchTweenSequence.Kill();
		}
		switchTweenSequence = null;
	}

	private void TrackSwitchTo(FortressMainUIPanel.MainPanelLayerSetup targetSetup)
	{
		if (targetSetup.layer == FortressMainUIPanel.MainPanelLayer.MainRoom)
		{
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "层级-主仪式间");
		}
		else if (targetSetup.layer == FortressMainUIPanel.MainPanelLayer.StargazingRoom)
		{
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "层级-观星室");
		}
		else if (targetSetup.layer == FortressMainUIPanel.MainPanelLayer.IdolRoom)
		{
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "层级-神像间");
		}
		else if (targetSetup.layer == FortressMainUIPanel.MainPanelLayer.Library)
		{
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "层级-档案馆");
		}
	}
}
