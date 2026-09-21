using System;
using System.Collections;
using System.Linq;
using Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NormalEventPanel : WorldEventPanel
{
	[Serializable]
	public class AnimConfig
	{
		[SerializeField]
		public float descriptionPanelShowTime;

		[SerializeField]
		public float choiceStartShowTime;

		[SerializeField]
		public float choiceShowTime;

		[SerializeField]
		public float choiceShowTimeOffset;

		[SerializeField]
		public float leaveWaitTime;

		[SerializeField]
		public float startInterval;

		[SerializeField]
		public float imageFadeInterval;
	}

	[SerializeField]
	public TextMeshProUGUI eventName;

	[SerializeField]
	private TextMeshProUGUI eventDescription;

	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private RectTransform choiceParent;

	[SerializeField]
	private WorldEventItemChoiceDisplay choicePrefab;

	[SerializeField]
	private LeaveButton leaveButton;

	[SerializeField]
	private MultiInfoDisplayPanel multiInfoDisplayPanel;

	[SerializeField]
	private RectTransform eventPanelPivot;

	[SerializeField]
	private RectTransform hudPanelPivot;

	[SerializeField]
	private RectTransform rightPivot;

	[SerializeField]
	private UIBase[] childrenUI;

	[NonSerialized]
	public CellUIPrefabPool<WorldEventItemChoiceDisplay, WorldEventChoice> choicePool;

	[NonSerialized]
	private bool isLeaveButtonShown;

	[NonSerialized]
	private bool isTextShown;

	[NonSerialized]
	private (WorldEventItemChoiceDisplay, MultiInfoDisplayPanel.SetupInfo) multiInfoDisplayPanelSetup;

	[NonSerialized]
	private TextMeshProUGUI hudButtonHint;

	[NonSerialized]
	private CanvasGroup hudButtonHintCanvasGroup;

	[NonSerialized]
	private Tween hudButtonHintHideTween;

	[NonSerialized]
	private Sequence sequence;

	[SerializeField]
	private CanvasGroup eventPanel;

	[NonSerialized]
	private WrapperEnumerator.Info refreshEventPanelDisplayCor;

	public bool refreshing { get; private set; }

	protected override void Init()
	{
		LeaveButton obj = leaveButton;
		obj.onLeave = (Action)Delegate.Combine(obj.onLeave, new Action(_OnLeaveButtonClicked));
		UIBase.gameState.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyClicked);
		choicePrefab.gameObject.SetActive(value: false);
		choicePool = GetPrefabPool<WorldEventItemChoiceDisplay, WorldEventChoice>(choicePrefab, choiceParent);
		base.Init();
		InitUI(multiInfoDisplayPanel);
		InitUI(leaveButton);
		UIBase[] array = childrenUI;
		foreach (UIBase child in array)
		{
			InitUI(child);
		}
	}

	protected override void Dispose()
	{
		base.Dispose();
		UIBase.gameState.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyClicked);
	}

	public override void Setup(WorldEvent worldEvent)
	{
		base.Setup(worldEvent);
		backgroundImage.sprite = ((worldEvent is IDynamicWorldEventBackground dynamicWorldEventBackground) ? dynamicWorldEventBackground.GetDynamicWorldEventBackground() : worldEvent.backgroundImage);
		backgroundImage.gameObject.SetActive(backgroundImage.sprite != null);
		eventName.text = worldEvent.currentDisplayInfo.title;
		eventPanel.alpha = 0f;
		isLeaveButtonShown = false;
		refreshing = false;
		leaveButton.Hide();
		SetDirty();
		if (UIBase.gameState.dungeonMapManager.currentGlobalProgress == 0)
		{
			UIBase.gameState.StartCoroutine(ShowStartGameEffect);
		}
	}

	public override void Unset()
	{
		base.Unset();
		choicePool.DespawnAll();
		backgroundImage.sprite = null;
		backgroundImage.gameObject.SetActive(value: false);
	}

	protected override void UpdateDisplay()
	{
		UpdateMultiInfoPanel();
		if (!UIBase.gameState.UIManager.multiInfoDisplayPanel.gameObject.activeSelf)
		{
			worldEvent.UpdateWorldEventDisplayInfo();
			if (worldEvent.currentDisplayInfo.needRefresh)
			{
				worldEvent.currentDisplayInfo.needRefresh = false;
				StartRefreshEventPanelDisplayCor();
			}
		}
	}

	public WorldEventItemChoiceDisplay GetChoiceDisplay(WorldEventChoice choice)
	{
		return choicePool.GetActiveElements().FirstOrDefault((WorldEventItemChoiceDisplay c) => c.itemChoice == choice);
	}

	public void SetDescription(string description)
	{
		eventDescription.text = description;
	}

	public void ShowHudButtonHint(string text)
	{
		ShowMapButton componentInChildren = GetComponentInChildren<ShowMapButton>(includeInactive: true);
		DeckButton componentInChildren2 = GetComponentInChildren<DeckButton>(includeInactive: true);
		if (!(componentInChildren == null) && !(componentInChildren2 == null))
		{
			if (hudButtonHint == null)
			{
				GameObject gameObject = new GameObject("NestActivityHint", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(CanvasGroup));
				gameObject.transform.SetParent(base.transform, worldPositionStays: false);
				hudButtonHint = gameObject.GetComponent<TextMeshProUGUI>();
				hudButtonHintCanvasGroup = gameObject.GetComponent<CanvasGroup>();
				hudButtonHint.font = eventDescription.font;
				hudButtonHint.fontSharedMaterial = eventDescription.fontSharedMaterial;
				hudButtonHint.fontSize = 22f;
				hudButtonHint.fontStyle = FontStyles.Bold;
				hudButtonHint.alignment = TextAlignmentOptions.Center;
				hudButtonHint.color = new Color(0.45f, 0.7f, 1f);
				hudButtonHint.enableWordWrapping = false;
				hudButtonHint.overflowMode = TextOverflowModes.Overflow;
			}
			RectTransform obj = base.transform as RectTransform;
			Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds((Transform)obj, componentInChildren.transform);
			Bounds bounds2 = RectTransformUtility.CalculateRelativeRectTransformBounds((Transform)obj, componentInChildren2.transform);
			RectTransform rectTransform = hudButtonHint.rectTransform;
			Vector2 anchorMin = (rectTransform.anchorMax = new Vector2(0.5f, 0.5f));
			rectTransform.anchorMin = anchorMin;
			rectTransform.pivot = new Vector2(0.5f, 1f);
			rectTransform.sizeDelta = new Vector2(220f, 26f);
			Vector2 vector2 = (rectTransform.anchoredPosition = new Vector2((Mathf.Min(bounds.min.x, bounds2.min.x) + Mathf.Max(bounds.max.x, bounds2.max.x)) * 0.5f, Mathf.Min(bounds.min.y, bounds2.min.y) - 8f));
			hudButtonHint.text = text;
			hudButtonHint.gameObject.SetActive(value: true);
			hudButtonHintHideTween?.Kill();
			hudButtonHintCanvasGroup.alpha = 1f;
			hudButtonHintHideTween = DOTween.Sequence().Append(DOTweenModuleUI.DOAnchorPosY(rectTransform, vector2.y - 28f, 0.6f, false)).Join(DOTweenModuleUI.DOFade(hudButtonHintCanvasGroup, 0f, 0.6f).SetDelay(0.3f))
				.OnComplete(delegate
				{
					hudButtonHint?.gameObject.SetActive(value: false);
				});
		}
	}

	public override bool HasAnim()
	{
		if (sequence == null && isTextShown && !refreshing)
		{
			return leaveButton.HasAnim();
		}
		return true;
	}

	private IEnumerator StartShowTexts()
	{
		isTextShown = false;
		eventDescription.TextEmergeEffect().timeScale = base.uiTimeScale;
		eventDescription.TextEmergeEffect().StartEmergeText(delegate
		{
			isTextShown = true;
		});
		yield return new WaitUntil(() => isTextShown);
		worldEvent?.onTextShown?.Invoke();
		SetDirty();
	}

	private IEnumerator ShowStartGameEffect()
	{
		AnimConfig config = ScriptableObjectSingleton<VFXDataPreset>.instance.normalEventPanelAnimConfig;
		yield return new WaitUntil(() => !BootstrapExtra.instance.loadingPanel.showing);
		yield return new WaitUntil(() => !UIBase.gameState.UIManager.startGameBuffConfigPanel.taskPanding && !UIBase.gameState.UIManager.startGameBuffConfigPanel.gameObject.activeInHierarchy);
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = base.uiTimeScale;
		sequence.AppendInterval(config.startInterval);
		Vector2 anchoredPosition = eventPanelPivot.anchoredPosition;
		sequence.Append(DOTweenModuleUI.DOAnchorPosX(eventPanelPivot, anchoredPosition.x, 0.5f, false));
		anchoredPosition.x -= 800f;
		eventPanelPivot.anchoredPosition = anchoredPosition;
		anchoredPosition = hudPanelPivot.anchoredPosition;
		sequence.Join(DOTweenModuleUI.DOAnchorPosY(hudPanelPivot, anchoredPosition.y, 0.5f, false));
		anchoredPosition.y -= 100f;
		hudPanelPivot.anchoredPosition = anchoredPosition;
		anchoredPosition = rightPivot.anchoredPosition;
		sequence.Join(DOTweenModuleUI.DOAnchorPosX(rightPivot, anchoredPosition.x, 0.5f, false));
		anchoredPosition.x += 800f;
		rightPivot.anchoredPosition = anchoredPosition;
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
	}

	private void UpdateMultiInfoPanel()
	{
		if (multiInfoDisplayPanelSetup.Item1 == selected)
		{
			return;
		}
		if (multiInfoDisplayPanelSetup.Item1 != null)
		{
			multiInfoDisplayPanel.Unset();
		}
		multiInfoDisplayPanelSetup.Item1 = selected;
		if (multiInfoDisplayPanelSetup.Item1 != null)
		{
			multiInfoDisplayPanelSetup.Item2 = new MultiInfoDisplayPanel.SetupInfo
			{
				items = selected.itemChoice.displayItems,
				influences = selected.itemChoice.displayInfluences,
				runes = selected.itemChoice.displayRunes?.ConvertAll((Rune r) => ((Rune, Item, int))(r, null, 1)),
				skills = selected.itemChoice.displaySkills,
				showUncollectedStatus = true
			};
			multiInfoDisplayPanel.Setup(multiInfoDisplayPanelSetup.Item2);
		}
	}

	private void ShowLeaveButton()
	{
		if (!isLeaveButtonShown)
		{
			isLeaveButtonShown = true;
			leaveButton.Show();
		}
	}

	private void Leave()
	{
		worldEvent.FinishedWorldEvent();
		refreshEventPanelDisplayCor?.enumerator.Stop();
	}

	private void _OnEmptyClicked()
	{
		if (base.gameObject.activeInHierarchy)
		{
			UnselectChoices();
		}
	}

	private void _OnLeaveButtonClicked()
	{
		if (UIBase.gameState.showConfirm && worldEvent is HealSpringEvent && choicePool.GetActiveElements().Any((WorldEventItemChoiceDisplay c) => c.interactable))
		{
			UIBase.gameState.UIManager.leaveConfirmPanel.Setup(new LeaveConfirmPanel.Info
			{
				text = ScriptableObjectSingleton<TextDataPreset>.instance.term_unview_heal_spring,
				onConfirm = Leave,
				onCancel = delegate
				{
					leaveButton.Show();
				}
			});
		}
		else
		{
			Leave();
		}
	}

	public override void HideDescription()
	{
		eventPanel.alpha = 0f;
	}

	public override void AfterChoiceConfirm(WorldEventChoice choice)
	{
		WorldEvent obj = worldEvent;
		if ((object)obj == null || !obj.KeepTitleAfterChoice)
		{
			eventName.text = "";
		}
	}

	private void StartRefreshEventPanelDisplayCor()
	{
		WrapperEnumerator.Info info = refreshEventPanelDisplayCor;
		if (info != null && !info.enumerator.Finished)
		{
			refreshEventPanelDisplayCor?.enumerator.Stop();
		}
		refreshEventPanelDisplayCor = new WrapperEnumerator.Info();
		refreshEventPanelDisplayCor.onFinish = delegate
		{
			refreshEventPanelDisplayCor = null;
		};
		UIBase.gameState.StartCoroutine(RefreshEventPanelDisplayCor, refreshEventPanelDisplayCor);
	}

	private IEnumerator RefreshEventPanelDisplayCor(WrapperEnumerator.Info info)
	{
		WorldEvent worldEvent = base.worldEvent;
		WorldEventDisplayInfo currentDisplayInfo = worldEvent.currentDisplayInfo;
		if (worldEvent == null)
		{
			yield break;
		}
		refreshing = true;
		choicePool.DespawnAll();
		SetDescription("");
		AnimConfig config = ScriptableObjectSingleton<VFXDataPreset>.instance.normalEventPanelAnimConfig;
		yield return DOTweenModuleUI.DOFade(eventPanel, 0f, 0f).WaitForCompletion();
		yield return DOTweenModuleUI.DOFade(eventPanel, 1f, config.descriptionPanelShowTime).WaitForCompletion();
		SetDescription(currentDisplayInfo.description);
		yield return StartShowTexts();
		foreach (WorldEventChoice item in currentDisplayInfo.choices.OrEmptyIfNull())
		{
			choicePool.Spawn(item).GetComponent<CanvasGroup>().alpha = 0f;
		}
		if (choicePool.activeCount > 0)
		{
			yield return new WaitForSeconds(config.choiceStartShowTime);
			foreach (WorldEventItemChoiceDisplay item2 in choicePool.GetActiveElements().ToList())
			{
				DOTweenModuleUI.DOFade(item2.GetComponent<CanvasGroup>(), 1f, config.choiceShowTime);
				yield return new WaitForSeconds(config.choiceShowTimeOffset);
			}
		}
		if (currentDisplayInfo.showLeaveButton || UIBase.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.PrepareForExcape))
		{
			ShowLeaveButton();
		}
		refreshing = false;
		SetDirty();
	}
}
