using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DungeonMapDisplay : UIBase, IDisplayModulesController, IFrameUpdate
{
	[Serializable]
	public class Config
	{
		[Serializable]
		public class StateConfig
		{
			public Color32 iconColor;

			public Color32 nameImageColor;

			public Color32 nameTextColor;
		}

		public float locationHorizontalOffset;

		public float locationVerticalOffset;

		public float startHorizontalOffset;

		public float endHorizontalOffset;

		public float mapExtraOffset;

		public float bossExtraOffset;

		public Vector2 locationRandomOffset;

		public float locationPathLineBendThreshold;

		public float connectionLineRadius;

		public float connectionLineArrowLength;

		public float minScale = 0.5f;

		public float maxScale = 3f;

		public float scaleSpeed = 0.5f;

		public float locationIdleAlpha;

		public float locationDisableAlpha;

		[SerializeField]
		public StateConfig canReach;

		[SerializeField]
		public StateConfig cannotReach;

		[SerializeField]
		public StateConfig finished;

		[SerializeField]
		public StateConfig selectable;

		[SerializeField]
		public StateConfig customPivot;

		public float locationBackgroundShowTime;

		public float locationHintShowTime;

		public float locationHintShowOffset;

		public Vector2 pathLineStartPointOffset;

		public float pathLineAnchorOffset;

		public Vector2 pathLineEndPointOffset;

		public Color finishedColor;

		public Color canJumpColor;

		public Color canReachColor;

		public Color disableColor;

		public Material normalMaterial;

		public Material canReachMaterial;

		public Sprite node_Finished;

		public Sprite node_Unfinished;

		public Sprite node_Boss;

		public Sprite node_SectionStart;

		public Sprite node_SectionEnd;

		public Gradient backgroundGradient;

		public Gradient progressBarGradient;

		public float dungeonMapPreviewStayTime;

		public float dungeonMapPreviewMoveTime;

		public Ease dungeonMapPreviewMoveEase;

		public float dungeonMapScrollToLocMoveTime;

		public Ease dungeonMapScrollToLocMoveEase;

		public float fogFadeTime;

		public float fogWaitTime;

		public float darkenTime;

		public float darkenWaitTime;
	}

	private const float mapOffset = 40f;

	[SerializeField]
	private RectTransform viewContentRect;

	[SerializeField]
	private RectTransform draftMapPivot;

	[SerializeField]
	private PathLocationDisplay locationDisplayPrefab;

	[SerializeField]
	private PathLineDisplay pathConnectLinePrefab;

	[SerializeField]
	private Transform pathLineParent;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	public SpriteRenderer bossBackground;

	[SerializeField]
	public ScrollRect scrollRect;

	[NonSerialized]
	public Config config;

	[NonSerialized]
	public DraftMap currentDraftMap;

	[NonSerialized]
	public PathLocationDisplay focusedLocationDisplay;

	[NonSerialized]
	public PathLocationDisplay selectedLocationDisplay;

	[NonSerialized]
	private CellUIPrefabPool<PathLocationDisplay, PathLocation> locationDisplayPool;

	[NonSerialized]
	private CellUIPrefabPool<PathLineDisplay, PathConnectionLine> pathLineDisplayPool;

	[NonSerialized]
	private InputModule inputModule;

	[NonSerialized]
	private Vector2 lastTouchPosition;

	[NonSerialized]
	private Vector2 lastPivot;

	[NonSerialized]
	private Vector2[] lastTwoTouchPositions = new Vector2[2];

	[NonSerialized]
	private Vector3 targetPosition;

	[NonSerialized]
	private float lastTouchDistance;

	[NonSerialized]
	private float targetScale;

	[NonSerialized]
	private Sequence mapMoveSequence;

	[NonSerialized]
	public bool updateLocations;

	private int touchCount => Input.touchCount;

	private float currentScale => viewContentRect.localScale.x;

	private IEnumerable<PathLocationDisplay> locationDisplays => locationDisplayPool.GetActiveElements();

	public PathLocationDisplay LastVisibleLocationDisplay => locationDisplays.First((PathLocationDisplay ld) => ld.location == UIBase.gameState.dungeonMapManager.currentDungeonMap.VisibleEndLocation);

	public PathLocationDisplay CurrentLocationDisplay => GetLocationDisplay(UIBase.gameState.dungeonMapManager.currentLocation);

	protected override void Init()
	{
		updateLocations = false;
		locationDisplayPrefab.gameObject.SetActive(value: false);
		locationDisplayPool = GetPrefabPool<PathLocationDisplay, PathLocation>(locationDisplayPrefab, locationDisplayPrefab.transform.parent);
		pathLineDisplayPool = GetPrefabPool<PathLineDisplay, PathConnectionLine>(pathConnectLinePrefab, pathLineParent);
		UIBase.gameState.dungeonMapManager.inMapMode = false;
		config = UIBase.applicationState.vfxDataPreset.dungeonMapDisplayConfig;
		UIBase.gameState.actionManager.actionQueueCleanEvent += SetDirty;
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyClicked);
		UIBase.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.EnterLocation, _OnEnterLocation);
		inputModule = GetModule<InputModule>();
		InputModule obj = inputModule;
		obj.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(obj.onHandleInput, new Action<InputModuleBase>(OnHandleInput));
		lastTouchPosition = Vector2.zero;
		lastTwoTouchPositions[0] = Vector2.zero;
		lastTwoTouchPositions[1] = Vector2.zero;
		lastTouchDistance = 0f;
		void KeepInside()
		{
			RectTransform rectTransform = viewContentRect;
			Vector3 lossyScale = rectTransform.lossyScale;
			Vector2 vector = new Vector2(rectTransform.rect.width * lossyScale.x, rectTransform.rect.height * lossyScale.y);
			Vector3[] array = new Vector3[4];
			base.rectTransform.GetWorldCorners(array);
			Vector2 vector2 = new Vector2((array[0].x + array[2].x) / 2f, (array[0].y + array[2].y) / 2f);
			Vector3 position = rectTransform.position;
			position.x = Sep(position.x, vector2.x + vector.x * rectTransform.pivot.x, vector2.x - vector.x * (1f - rectTransform.pivot.x));
			position.y = Sep(position.y, vector2.y + vector.y * rectTransform.pivot.y, vector2.y - vector.y * (1f - rectTransform.pivot.y));
			rectTransform.position = position;
			Vector3 anchoredPosition3D = viewContentRect.anchoredPosition3D;
			anchoredPosition3D.z = 0f;
			viewContentRect.anchoredPosition3D = anchoredPosition3D;
		}
		void OnBeginDrag()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			if (touchCount == 1)
			{
				Touch touch = Input.GetTouch(0);
				lastTouchPosition = ((Touch)(ref touch)).position;
			}
			else if (touchCount == 2)
			{
				ResetTwoTouch();
			}
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_map_movement);
		}
		void OnBeginDrag2()
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			lastTouchPosition = inputModule.eventData.position;
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_map_movement);
		}
		void OnDrag()
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			Touch touch;
			if (touchCount == 1)
			{
				touch = Input.GetTouch(0);
				Vector2 position = ((Touch)(ref touch)).position;
				viewContentRect.anchoredPosition += position - lastTouchPosition;
				lastTouchPosition = position;
			}
			else if (touchCount == 2)
			{
				touch = Input.GetTouch(0);
				Vector2 position2 = ((Touch)(ref touch)).position;
				touch = Input.GetTouch(1);
				Vector2 position3 = ((Touch)(ref touch)).position;
				Vector2 vector = (position2 + position3) / 2f;
				if (lastTwoTouchPositions[0] == Vector2.zero || lastTwoTouchPositions[1] == Vector2.zero || Vector2.Distance(position2, lastTwoTouchPositions[0]) > 200f || Vector2.Distance(position3, lastTwoTouchPositions[1]) > 200f || Vector2.Distance(vector, lastPivot) > 200f)
				{
					ResetTwoTouch();
				}
				else
				{
					float num = Vector2.Distance(position2, position3);
					targetScale *= num / lastTouchDistance;
					targetScale = targetScale.Clamp(config.minScale, config.maxScale);
					viewContentRect.anchoredPosition += vector - lastPivot;
					lastPivot = vector;
					lastTouchDistance = num;
					lastTwoTouchPositions[0] = position2;
					lastTwoTouchPositions[1] = position3;
				}
			}
		}
		void OnDrag2()
		{
			Vector2 position = inputModule.eventData.position;
			viewContentRect.anchoredPosition += position - lastTouchPosition;
			lastTouchPosition = position;
		}
		static void OnEndDrag()
		{
		}
		void OnHandleInput(InputModuleBase _)
		{
			if (PlatformHelper.IsMobile(debug: false))
			{
				OnHandleInput_Mobile();
			}
			else
			{
				OnHandleInput_PC();
			}
			KeepInside();
		}
		void OnHandleInput_Mobile()
		{
			if (inputModule.pointerClicked)
			{
				UIBase.applicationState.eventManager.UIEmptyClickedEvent.TriggerEvent();
			}
			if (inputModule.beginDrag)
			{
				OnBeginDrag();
			}
			if (inputModule.drag)
			{
				OnDrag();
			}
			if (inputModule.endDrag)
			{
				OnEndDrag();
			}
		}
		void OnHandleInput_PC()
		{
			if (inputModule.pointerClicked)
			{
				UIBase.applicationState.eventManager.UIEmptyClickedEvent.TriggerEvent();
			}
			if (inputModule.beginDrag)
			{
				OnBeginDrag2();
			}
			if (inputModule.drag)
			{
				OnDrag2();
			}
			if (Input.mouseScrollDelta.y != 0f)
			{
				targetScale *= 1f + Input.mouseScrollDelta.y * 0.5f;
				targetScale = targetScale.Clamp(config.minScale, config.maxScale);
			}
		}
		void ResetTwoTouch()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			Touch touch = Input.GetTouch(0);
			Vector2 position = ((Touch)(ref touch)).position;
			touch = Input.GetTouch(1);
			Vector2 position2 = ((Touch)(ref touch)).position;
			lastTwoTouchPositions[0] = position;
			lastTwoTouchPositions[1] = position2;
			lastTouchDistance = Vector2.Distance(position, position2);
			lastPivot = (position + position2) / 2f;
			Debug.Log($"Reset {viewContentRect.anchoredPosition} - touch1{position} touch2{position2} - {(position + position2) / 2f} - {lastPivot}");
		}
		static float Sep(float x, float min, float max)
		{
			if (x > min)
			{
				return min;
			}
			if (x < max)
			{
				return max;
			}
			return x;
		}
	}

	protected override void Dispose()
	{
		UIBase.gameState.actionManager.actionQueueCleanEvent -= SetDirty;
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyClicked);
		UIBase.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.EnterLocation, _OnEnterLocation);
		config = null;
	}

	protected override void UpdateDisplay()
	{
		foreach (PathLineDisplay activeElement in pathLineDisplayPool.GetActiveElements())
		{
			activeElement.gameObject.SetActive(activeElement.setupData.ShouldShow(currentDraftMap, UIBase.applicationState));
		}
		foreach (PathLocationDisplay activeElement2 in locationDisplayPool.GetActiveElements())
		{
			activeElement2.gameObject.SetActive(activeElement2.location.ShouldShow(currentDraftMap, UIBase.applicationState));
		}
	}

	public void Setup(DungeonMap map)
	{
		base.rectTransform.anchoredPosition3D = Vector3.zero;
		viewContentRect.SetWidth(map.mapSize.x + (float)Screen.width);
		viewContentRect.SetHeight(map.mapSize.y + (float)Screen.height);
		foreach (PathLocation location in map.locations)
		{
			if (location != null && location.spawnDisplay)
			{
				locationDisplayPool.Spawn(location).name = $"PathLocationDisplay({location.displayPosition})";
			}
		}
		foreach (PathConnectionLine connectionLine in map.connectionLines)
		{
			pathLineDisplayPool.Spawn(connectionLine);
		}
		ClearDraftMap();
		currentDraftMap = UnityEngine.Object.Instantiate(map.GetLoadedDraftMap(UIBase.applicationState.assetCatalog), draftMapPivot);
		currentDraftMap.transform.localPosition = Vector3.zero;
		currentDraftMap.SetRenderOrder();
		viewContentRect.SetWidth((currentDraftMap.transform as RectTransform).GetWidth());
		viewContentRect.SetHeight((currentDraftMap.transform as RectTransform).GetHeight());
		scrollRect.content = currentDraftMap.RectTransform();
		bossBackground.transform.position = locationDisplays.First((PathLocationDisplay l) => l.location == map.VisibleEndLocation).transform.position;
		Vector3 localPosition = bossBackground.transform.localPosition;
		localPosition.z = 40f;
		bossBackground.transform.localPosition = localPosition;
		viewContentRect.localScale = Vector3.one;
		targetScale = 1f;
		SetDirty();
	}

	public void Unset()
	{
		locationDisplayPool.DespawnAll();
		pathLineDisplayPool.DespawnAll();
	}

	public void ClearDraftMap()
	{
		if (!(currentDraftMap == null))
		{
			UnityEngine.Object.Destroy(currentDraftMap.gameObject);
			currentDraftMap = null;
		}
	}

	public void FrameUpdate()
	{
		targetScale = targetScale.Clamp(config.minScale, config.maxScale);
		if (!Mathf.Approximately(targetScale, currentScale))
		{
			float num = Mathf.MoveTowards(currentScale, targetScale, Time.deltaTime * config.scaleSpeed);
			viewContentRect.localScale = Vector3.one * num;
		}
	}

	public void FocusLocation(PathLocationDisplay display)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (!(focusedLocationDisplay == display))
		{
			focusedLocationDisplay = display;
			focusedLocationDisplay.currentState = PathLocationDisplay.State.Focused;
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_map_pindown);
			(parent as UIBase).SetDirty();
		}
	}

	public void UnfocusLocation()
	{
		if (!(focusedLocationDisplay == null))
		{
			if (focusedLocationDisplay != null)
			{
				focusedLocationDisplay.currentState = PathLocationDisplay.State.Selectable;
			}
			focusedLocationDisplay = null;
			SetDirty();
		}
	}

	public void SelectLocation(PathLocationDisplay display)
	{
		focusedLocationDisplay = null;
		selectedLocationDisplay = display;
		UIBase.gameState.StartCoroutine(() => StartSelectLocationAnim(display));
	}

	public PathLocationDisplay GetLocationDisplay(PathLocation location)
	{
		return locationDisplays.FirstOrDefault((PathLocationDisplay d) => d.location == location);
	}

	private void RefreshLocations()
	{
		foreach (PathLocationDisplay locationDisplay in locationDisplays)
		{
			locationDisplay.SetDirty();
		}
	}

	private void _OnEmptyClicked()
	{
		if (canvasGroup.interactable)
		{
			UnfocusLocation();
			SetDirty();
		}
	}

	private void _OnEnterLocation(GameEvent.Type t_, GameEvent e_)
	{
		RefreshLocations();
	}

	public IEnumerator StartSelectLocationAnim(PathLocationDisplay display)
	{
		UIHelper.DisableAllButton();
		updateLocations = false;
		display.currentIconModule.Select();
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_map_confirm);
		yield return display.selectImageModule.OnSelect();
		if (UIBase.applicationState.settingManager.uiTimeScaleTypeSetting == TimeScaleManager.TimeScaleType.X1)
		{
			yield return PlayTween_ScrollToLocation(display);
		}
		else
		{
			UIBase.gameState.StartCoroutine(() => PlayTween_ScrollToLocation(display));
		}
		UIBase.gameState.actionManager.EnqueueAction(new EnterLocationAction(display.location));
		UIBase.gameState.actionManager.EnqueueAction(new ActionAction(delegate
		{
			selectedLocationDisplay = null;
			UIHelper.EnableAllButton();
			updateLocations = true;
		}));
	}

	public void SetCurrentCenter()
	{
		PathLocation currentLocation = UIBase.gameState.dungeonMapManager.currentLocation;
		if (currentLocation != null)
		{
			Ray ray = UIBase.applicationState.currentVirtualCamera.bindedCamera.finalCamera.ScreenPointToRay(new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f));
			Vector3 vector = Vector3.Normalize(base.rectTransform.rotation * Vector3.back);
			PathLocationDisplay locationDisplay = GetLocationDisplay(currentLocation);
			Vector3 vector2 = viewContentRect.position + (ray.GetPoint((0f - Vector3.Dot(ray.origin, vector) + Vector3.Dot(vector, base.rectTransform.position)) / Vector3.Dot(ray.direction, vector)) - locationDisplay.transform.position);
			viewContentRect.SetPosition(vector2.x, vector2.y);
			RectTransform obj = viewContentRect;
			float? z = 0f;
			obj.SetLocalPosition(null, null, z);
		}
	}

	private Vector3 GetToCenterLocationMapWorldPosition(PathLocationDisplay display)
	{
		return base.transform.position + viewContentRect.position - display.transform.position;
	}

	public IEnumerator PlayTween_ScrollMap()
	{
		if (!(CurrentLocationDisplay == null) && !(LastVisibleLocationDisplay == null))
		{
			canvasGroup.blocksRaycasts = false;
			SetViewRectToLocation(LastVisibleLocationDisplay.location);
			canvasGroup.blocksRaycasts = true;
			yield return new WaitForSeconds(config.dungeonMapPreviewStayTime / base.uiTimeScale);
			yield return PlayTween_ScrollToLocation(CurrentLocationDisplay);
			canvasGroup.blocksRaycasts = true;
		}
	}

	public void SetViewRectToLocation(PathLocation loc)
	{
		PathLocationDisplay locationDisplay = GetLocationDisplay(loc);
		Vector3 toCenterLocationMapWorldPosition = GetToCenterLocationMapWorldPosition(locationDisplay);
		viewContentRect.SetPosition(toCenterLocationMapWorldPosition.x, toCenterLocationMapWorldPosition.y);
		RectTransform obj = viewContentRect;
		float? z = 0f;
		obj.SetLocalPosition(null, null, z);
	}

	public IEnumerator PlayTween_ScrollToLocation(PathLocationDisplay display)
	{
		mapMoveSequence?.Kill();
		Ray ray = UIBase.applicationState.currentVirtualCamera.bindedCamera.finalCamera.ScreenPointToRay(new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f));
		Vector3 vector = Vector3.Normalize(base.rectTransform.rotation * Vector3.back);
		Vector3 position = viewContentRect.position + (ray.GetPoint((0f - Vector3.Dot(ray.origin, vector) + Vector3.Dot(vector, base.rectTransform.position)) / Vector3.Dot(ray.direction, vector)) - display.transform.position);
		Vector3 vector2 = viewContentRect.transform.parent.InverseTransformPoint(position);
		float? z = 0f;
		position = vector2.GetWith(null, null, z);
		mapMoveSequence = DOTween.Sequence();
		mapMoveSequence.timeScale = base.uiTimeScale;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		mapMoveSequence.Append(viewContentRect.DOLocalMove(position, ScriptableObjectSingleton<VFXDataPreset>.instance.dungeonMapDisplayConfig.dungeonMapScrollToLocMoveTime).SetEase(ScriptableObjectSingleton<VFXDataPreset>.instance.dungeonMapDisplayConfig.dungeonMapScrollToLocMoveEase));
		Sequence sequence = mapMoveSequence;
		sequence.onKill = (TweenCallback)Delegate.Combine(sequence.onKill, (TweenCallback)delegate
		{
			mapMoveSequence = null;
			viewContentRect.localPosition = position;
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
		});
		mapMoveSequence.Play();
		yield return mapMoveSequence.WaitForKill();
	}
}
