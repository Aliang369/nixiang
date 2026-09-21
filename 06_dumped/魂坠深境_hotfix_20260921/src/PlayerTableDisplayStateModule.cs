using System;
using System.Collections;
using System.Collections.Generic;
using ContainerOfSoul.Display.Game.BattleDisplay;
using DG.Tweening;
using ProceduralToolkit;
using Spine;
using Spine.Unity;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class PlayerTableDisplayStateModule : ItemDisplayBase.DisplayModule
{
	public enum AnimType
	{
		None,
		TriggerAbilityAnim,
		PlayTween_HintOverload,
		PlayTween_Draw,
		PlayTween_AddNew,
		PlayTween_Add,
		PlayTween_Remove,
		PlayTween_TransferToEnemy,
		PlayTween_TransferFromEnemy,
		PlayTween_Exile,
		PlayTween_Spell,
		PlayTween_Move,
		OnSwap,
		OnShuffle,
		OnConvert
	}

	public enum PositionState
	{
		Idle = 0,
		Shifting = 2
	}

	public enum State
	{
		Idle,
		UnDraw,
		Selected,
		Selecting,
		Added,
		Over,
		PlayingMutualExcludeAnim,
		OutOfControl
	}

	[SerializeField]
	private RectTransform contentTransform;

	[SerializeField]
	private RectTransform hiddenTransform;

	[SerializeField]
	private SkeletonGraphic positivePassive;

	[SerializeField]
	private SkeletonGraphic negativePassive;

	[SerializeField]
	private Image itemSpellImage;

	[SerializeField]
	private Image costImage;

	[SerializeField]
	private UILineRenderer costLine;

	[NonSerialized]
	public AnimType currentAnimType;

	[NonSerialized]
	private int currentCount;

	[NonSerialized]
	private int currentIndex;

	[NonSerialized]
	public PositionState currentPositionState;

	[NonSerialized]
	private bool forceShift;

	[NonSerialized]
	public PlayerControlInfo.ItemInfo frameItemInfo;

	[NonSerialized]
	private bool isIntentHidden;

	[NonSerialized]
	private bool isShift;

	[NonSerialized]
	private DG.Tweening.Sequence sequence;

	[NonSerialized]
	private DG.Tweening.Sequence sequenceForCost;

	[NonSerialized]
	private DG.Tweening.Sequence sequenceForShift;

	[NonSerialized]
	private DG.Tweening.Sequence sequenceForIdlePose;

	[NonSerialized]
	private DG.Tweening.Sequence sequenceForVisibility;

	[NonSerialized]
	private DG.Tweening.Sequence sequenceForAbilityEnd;

	[NonSerialized]
	private Ability.AbilityAnimPriority currentAbilityAnimPriority;

	[NonSerialized]
	private bool stateDirty;

	[NonSerialized]
	private TrackEntry track;

	public State currentState;

	private Vector2? currentAddedPosition;

	[NonSerialized]
	private Item lastAddedToItem;

	[NonSerialized]
	private bool playingStateAnim;

	private bool? castableCache;

	public bool IsAtRest
	{
		get
		{
			if (currentState.IsAny(State.Idle, State.Over))
			{
				return !playingStateAnim;
			}
			return false;
		}
	}

	private Canvas rootCameraCanvas => UIDisplayModule.gameState.UIManager.ui3DCameraCanvas;

	private SpellCastInfo displaySpellCastInfo => UIDisplayModule.gameState.UIManager.playerItemArea.controlInfo.displaySpellInfo;

	private SpellCastInfo spellCastInfo => UIDisplayModule.gameState.playerState.playerSpellInfo;

	public override void Setup()
	{
		currentState = State.UnDraw;
		currentAnimType = AnimType.None;
		currentAbilityAnimPriority = Ability.AbilityAnimPriority.None;
		currentPositionState = PositionState.Idle;
		currentIndex = -1;
		isShift = false;
		forceShift = false;
		isIntentHidden = base.slotDisplay.item.IsHidden(UIDisplayModule.gameState);
		contentTransform.localScale = Vector3.one;
		hiddenTransform.localScale = Vector3.one;
		contentTransform.gameObject.SetActive(!isIntentHidden);
		hiddenTransform.gameObject.SetActive(isIntentHidden);
		playingStateAnim = false;
		stateDirty = false;
		costLine.gameObject.SetActive(value: false);
	}

	public override void Unset()
	{
		KillAllSequence();
		ClearPassiveEffect();
		currentAnimType = AnimType.None;
		currentAbilityAnimPriority = Ability.AbilityAnimPriority.None;
		costImage.gameObject.SetActive(value: false);
		costLine.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (currentState == State.Added)
		{
			FrameUpdateAddAsCost();
		}
	}

	public Vector3 GetIdleWorldPosition(int index)
	{
		CheckAndRefreshData();
		return this.RectTransform().AnchoredToWorldPosition(GetIdlePosition(index, currentCount, isShift));
	}

	private void CheckAndRefreshData()
	{
		int num = currentIndex;
		int num2 = currentCount;
		bool flag = isShift;
		Item item = lastAddedToItem;
		RefreshData();
		if (num != currentIndex || num2 != currentCount || flag != isShift || item != lastAddedToItem)
		{
			stateDirty = true;
		}
	}

	private void RefreshData()
	{
		RefreshIndex();
		RefreshShift();
		RefreshAddedInfo();
	}

	private void RefreshIndex()
	{
		currentIndex = GetItemIndex();
		currentCount = GetItemCount();
	}

	private void RefreshShift()
	{
		if (frameItemInfo != null)
		{
			isShift = frameItemInfo.shift;
			return;
		}
		Item spellItem = UIDisplayModule.gameState.UIManager.playerItemArea.controlInfo.spellItem;
		if (spellItem == null || spellItem.OwnerBft?.isPlayer != true)
		{
			isShift = false;
			return;
		}
		int? num = spellItem.owner?.GetRawIndex(spellItem);
		if (!num.HasValue)
		{
			isShift = false;
		}
		else
		{
			isShift = num.Value < currentIndex;
		}
	}

	private void RefreshAddedInfo()
	{
		if (frameItemInfo != null)
		{
			lastAddedToItem = frameItemInfo.addedToItem;
		}
		else if (spellCastInfo.addedCosts.Contains(base.slotDisplay.item))
		{
			lastAddedToItem = spellCastInfo.spellItem;
		}
		else if (displaySpellCastInfo.addedCosts.Contains(base.slotDisplay.item))
		{
			lastAddedToItem = displaySpellCastInfo.spellItem;
		}
		else
		{
			lastAddedToItem = null;
		}
	}

	private void KillAllSequence()
	{
		sequence?.Kill();
		sequenceForCost?.Kill();
		sequenceForShift?.Kill();
		sequenceForIdlePose?.Kill();
		sequenceForVisibility?.Kill();
		sequenceForAbilityEnd?.Kill();
	}

	private void KillSequenceForAbilityStart()
	{
		sequence?.Kill();
		sequenceForCost?.Kill();
		sequenceForIdlePose?.Kill();
		sequenceForAbilityEnd?.Kill();
	}

	public void SkipMutualExcludeAnim(int? index = null, int? count = null)
	{
		KillAllSequence();
		sequence = null;
		currentState = State.OutOfControl;
		currentAnimType = AnimType.None;
		currentAbilityAnimPriority = Ability.AbilityAnimPriority.None;
		RefreshData();
		if (index.HasValue)
		{
			currentIndex = index.Value;
		}
		if (count.HasValue)
		{
			currentCount = count.Value;
		}
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		base.rectTransform.anchoredPosition = GetIdlePosition(isShift);
		base.rectTransform.localScale = playerItemAnimConfig.idleScale;
		base.rectTransform.localRotation = Quaternion.Euler(playerItemAnimConfig.idleRotation);
		base.gameObject.SetActive(value: true);
		base.slotDisplay.gameObject.SetActive(value: true);
		base.slotDisplay.SetDirty();
	}

	private int GetItemIndex()
	{
		if (frameItemInfo != null)
		{
			return frameItemInfo.index;
		}
		return UIDisplayModule.gameState.playerState.TableContainer.GetItemIndex(base.slotDisplay.item) ?? (-1);
	}

	private int GetItemCount()
	{
		return UIDisplayModule.gameState.playerState.TableContainer.ItemsCount;
	}

	private Vector2 GetIdlePosition(bool shift)
	{
		return GetIdlePosition(currentIndex, currentCount, shift);
	}

	private Vector2 GetIdlePosition(int index, int count, bool shift)
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.playerItemAnimConfig;
		float num = playerItemAnimConfig.itemOffset + playerItemAnimConfig.itemIdleSpace + 0.001f;
		float width = (base.slotDisplay.transform.parent as RectTransform).GetWidth();
		int num2 = (int)(width / num);
		float2 val = default(float2);
		if (count <= num2)
		{
			float num3 = (float)index - (float)count / 2f + 0.5f;
			((float2)(ref val))._002Ector(num3 * num, 0f);
		}
		else
		{
			float num4 = width / (float)count;
			float num5 = (float)index - (float)count / 2f + 0.5f;
			((float2)(ref val))._002Ector(num5 * num4, 0f);
		}
		if (shift)
		{
			val.x += GetShiftOffset();
		}
		return float2.op_Implicit(val);
	}

	private Vector2? GetAddedPosition()
	{
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		PlayerItemDisplay playerItemDisplay = UIDisplayModule.gameState.UIManager.playerItemArea.GetSlotDisplay(lastAddedToItem);
		if (playerItemDisplay == null)
		{
			return null;
		}
		PlayerTableDisplayStateModule module = playerItemDisplay.GetModule<PlayerTableDisplayStateModule>();
		int num = displaySpellCastInfo.addedCosts.Count + spellCastInfo.addedCosts.Count;
		int num2 = displaySpellCastInfo.addedCosts.IndexOf(base.slotDisplay.item);
		if (num2 == -1)
		{
			num2 = spellCastInfo.addedCosts.IndexOf(base.slotDisplay.item);
		}
		module.RefreshData();
		module.forceShift = true;
		Vector2 anchoredPosition = module.slotDisplay.RectTransform().anchoredPosition;
		anchoredPosition.y -= 50f;
		float num3 = playerItemAnimConfig.multiAddedTotalOffset / (float)num;
		anchoredPosition.x += -50f + num3 * (float)num2;
		return anchoredPosition;
	}

	private float GetShiftOffset()
	{
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		float num = playerItemAnimConfig.itemOffset + playerItemAnimConfig.itemIdleSpace;
		float width = (base.slotDisplay.transform.parent as RectTransform).GetWidth();
		int num2 = (int)(width / num);
		if (currentCount <= num2)
		{
			return playerItemAnimConfig.rightShiftOffset - num;
		}
		float num3 = width / (float)currentCount;
		return playerItemAnimConfig.rightShiftOffset - num3;
	}

	private Vector3 GetDrawPilePosition()
	{
		return UIDisplayModule.gameState.UIManager.battlePanel.drawPileButtonPivot?.ToCanvasWorldPosition(rootCameraCanvas) ?? Vector3.zero;
	}

	private Vector2 GetMiddlePosition()
	{
		return new Vector2(500f, 0f);
	}

	private void AppendToIdleSequence(DG.Tweening.Sequence sequence, float time)
	{
		if (currentIndex != -1)
		{
			ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
			RectTransform rectTransform = base.slotDisplay.RectTransform();
			Vector2 idlePosition = GetIdlePosition(isShift);
			sequence.Append(DOTweenModuleUI.DOAnchorPos(rectTransform, idlePosition, time, false));
			sequence.Join(rectTransform.DOLocalMoveZ(0f, time));
			sequence.Join(rectTransform.DOLocalRotate(playerItemAnimConfig.idleRotation, time));
			sequence.Join(rectTransform.DOScale(Vector3.one, time));
		}
	}

	public void RefreshDisplay()
	{
		if (currentState.IsAny(State.PlayingMutualExcludeAnim, State.UnDraw) || !base.slotDisplay.enableUpdate || UIDisplayModule.gameState.UIManager.playerItemArea.blockItemDisplayUpdate || UIDisplayModule.gameState.battleState.isConsumingOverflowItems)
		{
			frameItemInfo = null;
			return;
		}
		UpdateHideIntend();
		UpdateStateAnim();
		frameItemInfo = null;
	}

	private void UpdateStateAnim()
	{
		State state = GetState();
		if (state == currentState)
		{
			if (state == State.Selecting)
			{
				OnUpdateSelect();
			}
			switch (state)
			{
			case State.Added:
				OnUpdateAddAsCost();
				return;
			case State.Idle:
				OnShift();
				RestoreIdlePose();
				return;
			}
			CheckAndRefreshData();
			if (!stateDirty)
			{
				return;
			}
		}
		currentState = state;
		stateDirty = false;
		switch (state)
		{
		case State.Idle:
			OnToIdle();
			base.slotDisplay.SetDirty();
			break;
		case State.Over:
			OnToOver();
			base.slotDisplay.SetDirty();
			break;
		case State.Selected:
			OnSelected();
			base.slotDisplay.SetDirty();
			break;
		case State.Added:
			OnAddAsCost();
			break;
		case State.Selecting:
			OnUpdateSelect(switchState: true);
			break;
		default:
			TraceLog.Exception(new ArgumentOutOfRangeException($"{state}"));
			break;
		}
		base.slotDisplay.GetComponent<CanvasGroup>().blocksRaycasts = state != State.Added;
	}

	private State GetState()
	{
		PlayerItemArea playerItemArea = UIDisplayModule.gameState.UIManager.playerItemArea;
		DisplaySpellCastInfo displaySpellInfo = playerItemArea.controlInfo.displaySpellInfo;
		RealSpellCastInfo playerSpellInfo = UIDisplayModule.gameState.playerState.playerSpellInfo;
		bool flag = displaySpellInfo.spellItem == base.slotDisplay.item;
		bool flag2 = playerSpellInfo.spellItem == base.slotDisplay.item;
		bool flag3 = playerItemArea.controlInfo.targetItem == base.slotDisplay.item;
		bool flag4 = playerItemArea.controlInfo.rawOver == base.slotDisplay;
		if ((frameItemInfo == null) ? (displaySpellInfo.CheckItemAdded(base.slotDisplay.item) || playerSpellInfo.CheckItemAdded(base.slotDisplay.item)) : frameItemInfo.added)
		{
			return State.Added;
		}
		if (currentState == State.Added)
		{
			return State.Idle;
		}
		switch (playerItemArea.controlInfo.state)
		{
		case PlayerControlInfo.State.Prepare:
			if (flag2)
			{
				return State.Selected;
			}
			if (flag4)
			{
				return State.Over;
			}
			return State.Idle;
		case PlayerControlInfo.State.ClickSelected:
			if (flag)
			{
				return State.Selected;
			}
			return State.Idle;
		case PlayerControlInfo.State.DragSelecting:
			if (flag)
			{
				return State.Selecting;
			}
			return State.Idle;
		case PlayerControlInfo.State.GettingTarget:
		case PlayerControlInfo.State.TargetSelected:
			if (flag | flag3)
			{
				return State.Selected;
			}
			return State.Idle;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void OnToIdle()
	{
		KillAllSequence();
		playingStateAnim = true;
		RefreshData();
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		costLine.gameObject.SetActive(value: false);
		AppendToIdleSequence(sequence, playerItemAnimConfig.unselectTime);
		DG.Tweening.Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			playingStateAnim = false;
			UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
			itemSpellImage.gameObject.SetActive(value: false);
			costImage.gameObject.SetActive(value: false);
		});
		sequence.Play();
		UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
	}

	private void OnToOver()
	{
		KillAllSequence();
		playingStateAnim = true;
		RefreshData();
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		Vector2 idlePosition = GetIdlePosition(isShift);
		sequence.Append(DOTweenModuleUI.DOAnchorPos(base.rectTransform, idlePosition + Vector2.up * playerItemAnimConfig.overOffset, playerItemAnimConfig.selectTime1, false));
		sequence.Join(base.rectTransform.DOLocalRotate(playerItemAnimConfig.overRotation, playerItemAnimConfig.selectTime1));
		sequence.Append(base.rectTransform.DOScale(playerItemAnimConfig.overScale, playerItemAnimConfig.selectTime2));
		DG.Tweening.Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			playingStateAnim = false;
			UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
			DisplaySpellCastInfo displaySpellInfo = UIDisplayModule.gameState.UIManager.playerItemArea.controlInfo.displaySpellInfo;
			if (displaySpellInfo.spellItem == base.slotDisplay.item && displaySpellInfo.spellItem.HasSpell && displaySpellInfo.HasEnoughCost() && displaySpellInfo.SpellCastAble(out var _))
			{
				UIDisplayModule.gameState.StartCoroutine(PlayFullFilledCost);
			}
		});
		sequence.Play();
		UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
	}

	private void OnSelected()
	{
		castableCache = null;
		KillAllSequence();
		playingStateAnim = true;
		RefreshData();
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		Vector2 idlePosition = GetIdlePosition(isShift);
		sequence.Append(DOTweenModuleUI.DOAnchorPos(base.rectTransform, idlePosition + Vector2.up * playerItemAnimConfig.selectPointerMinOffset, playerItemAnimConfig.selectTime1, false));
		sequence.Join(base.rectTransform.DOLocalRotate(playerItemAnimConfig.selectRotation, playerItemAnimConfig.selectTime1));
		sequence.Append(base.rectTransform.DOScale(playerItemAnimConfig.selectAnimScale2, playerItemAnimConfig.selectTime2));
		DG.Tweening.Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			playingStateAnim = false;
			UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
			DisplaySpellCastInfo displaySpellInfo = UIDisplayModule.gameState.UIManager.playerItemArea.controlInfo.displaySpellInfo;
			if (displaySpellInfo.spellItem == base.slotDisplay.item && displaySpellInfo.spellItem.HasSpell && displaySpellInfo.HasEnoughCost() && displaySpellInfo.SpellCastAble(out var _))
			{
				UIDisplayModule.gameState.StartCoroutine(PlayFullFilledCost);
			}
		});
		sequence.Play();
		UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
	}

	private void OnUpdateSelect(bool switchState = false)
	{
		Vector3? frameDragPosition = UIDisplayModule.gameState.UIManager.playerItemArea.controlInfo.frameDragPosition;
		if (frameDragPosition.HasValue)
		{
			KillAllSequence();
			RefreshData();
			if (!UIDisplayModule.gameState.actionManager.isEmpty || UIDisplayModule.gameState.UIManager.playerItemArea.controlInfo.isCasting)
			{
				castableCache = null;
			}
			bool valueOrDefault = castableCache == true;
			SpellCastInfo.SpellCastFailureReason failureReason;
			if (!castableCache.HasValue)
			{
				valueOrDefault = SpellCastInfo.SpellCastAble(UIDisplayModule.gameState, base.slotDisplay.item, null, inBattleDisplay: true, out failureReason);
				castableCache = valueOrDefault;
			}
			frameDragPosition = frameDragPosition.Value.WorldPositionFromCanvasToCanvas(UIDisplayModule.gameState.UIManager.mainCanvas, UIDisplayModule.gameState.UIManager.ui3DCameraCanvas);
			ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
			sequence = DOTween.Sequence();
			sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
			sequence.Append(base.rectTransform.DOLocalRotate(playerItemAnimConfig.selectRotation, playerItemAnimConfig.selectTime1));
			sequence.Join(base.rectTransform.DOScale(playerItemAnimConfig.selectMaxScale, playerItemAnimConfig.selectTime2));
			base.rectTransform.position = frameDragPosition.Value;
			float? y;
			if (base.rectTransform.anchoredPosition.y > playerItemAnimConfig.selectPointerMaxOffset)
			{
				RectTransform obj = base.rectTransform;
				y = playerItemAnimConfig.selectPointerMaxOffset;
				obj.SetAnchoredPosition(null, y);
			}
			RectTransform obj2 = base.rectTransform;
			y = 0f;
			obj2.SetAnchoredPosition3D(null, null, y);
			if (UIDisplayModule.gameState.UIManager.playerItemArea.controlInfo.inSpellArea && castableCache.Value)
			{
				itemSpellImage.gameObject.SetActive(value: true);
				itemSpellImage.transform.localScale = Vector3.one;
			}
			else
			{
				itemSpellImage.gameObject.SetActive(value: false);
			}
			DG.Tweening.Sequence obj3 = sequence;
			obj3.onKill = (TweenCallback)Delegate.Combine(obj3.onKill, (TweenCallback)delegate
			{
				sequence = null;
				playingStateAnim = false;
				UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
			});
			sequence.Play();
			DisplaySpellCastInfo displaySpellInfo = UIDisplayModule.gameState.UIManager.playerItemArea.controlInfo.displaySpellInfo;
			if (switchState && displaySpellInfo.spellItem == base.slotDisplay.item && base.slotDisplay.item.HasSpell && displaySpellInfo.HasEnoughCost() && displaySpellInfo.SpellCastAble(out failureReason))
			{
				UIDisplayModule.gameState.StartCoroutine(PlayFullFilledCost);
			}
		}
	}

	private void OnAddAsCost()
	{
		TraceLog.Debug("OnAddAsCost");
		RefreshData();
		KillAllSequence();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		playingStateAnim = true;
		currentAddedPosition = null;
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		sequence.Join(base.rectTransform.DOScale(playerItemAnimConfig.addedScale, playerItemAnimConfig.addTime));
		sequence.Join(base.rectTransform.DOLocalRotate(playerItemAnimConfig.idleRotation, playerItemAnimConfig.addTime));
		DG.Tweening.Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			costImage.gameObject.SetActive(value: true);
			sequence = null;
			playingStateAnim = false;
			UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
		});
		sequence.Play();
	}

	private void FrameUpdateAddAsCost()
	{
		TraceLog.Debug("FrameUpdateAddAsCost");
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		PlayerTableDisplayStateModule playerTableDisplayStateModule = UIDisplayModule.gameState.UIManager.playerItemArea.GetSlotDisplay(lastAddedToItem)?.GetModule<PlayerTableDisplayStateModule>();
		if (!(playerTableDisplayStateModule == null))
		{
			CostModule module = UIDisplayModule.gameState.UIManager.playerItemArea.GetSlotDisplay(lastAddedToItem).GetModule<CostModule>();
			costImage.gameObject.SetActive(value: true);
			costLine.gameObject.SetActive(value: true);
			if (playerTableDisplayStateModule.itemSpellImage.gameObject.activeSelf)
			{
				costImage.color = playerItemAnimConfig.costImageColorHighLight;
				costLine.color = playerItemAnimConfig.costLineColorHighLight;
			}
			else
			{
				costImage.color = playerItemAnimConfig.costImageColor;
				costLine.color = playerItemAnimConfig.costLineColor;
			}
			costLine.transform.position = base.transform.position;
			UILineRenderer uILineRenderer = costLine;
			Vector2[] obj = new Vector2[2]
			{
				Vector2.zero,
				default(Vector2)
			};
			Vector3 vector = costLine.transform.parent.InverseTransformPoint(module.costIconImage.transform.position);
			float? z = 0f;
			obj[1] = vector.GetWith(null, null, z);
			uILineRenderer.Points = obj;
		}
	}

	private void OnUpdateAddAsCost()
	{
		RefreshData();
		Vector2? addedPosition = GetAddedPosition();
		if (!addedPosition.HasValue || (currentAddedPosition.HasValue && Vector2.Distance(addedPosition.Value, currentAddedPosition.Value) < 0.1f))
		{
			return;
		}
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		currentAddedPosition = addedPosition;
		sequenceForCost?.Kill();
		DG.Tweening.Sequence costSequence = DOTween.Sequence();
		sequenceForCost = costSequence;
		costSequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		costSequence.Append(DOTweenModuleUI.DOAnchorPos(base.rectTransform, currentAddedPosition.Value, playerItemAnimConfig.addTime, false));
		DG.Tweening.Sequence obj = costSequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			if (sequenceForCost == costSequence)
			{
				sequenceForCost = null;
			}
		});
		costSequence.Play();
	}

	public void OnShift()
	{
		if (playingStateAnim || currentState != State.Idle || UIDisplayModule.gameState.UIManager.playerItemArea.blockItemDisplayUpdate || (currentPositionState != PositionState.Idle && currentPositionState != PositionState.Shifting))
		{
			return;
		}
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		int num = currentIndex;
		int num2 = currentCount;
		bool flag = isShift;
		RefreshData();
		Vector2 idlePosition = GetIdlePosition(isShift);
		bool flag2 = num == currentIndex && num2 == currentCount && flag == isShift;
		if (!forceShift & flag2)
		{
			DG.Tweening.Sequence obj = sequenceForShift;
			if ((obj != null && obj.IsActive()) || Mathf.Abs(base.rectTransform.anchoredPosition.x - idlePosition.x) <= 0.1f)
			{
				return;
			}
		}
		sequenceForShift?.Kill();
		currentPositionState = PositionState.Shifting;
		forceShift = false;
		DG.Tweening.Sequence shiftSequence = DOTween.Sequence();
		sequenceForShift = shiftSequence;
		shiftSequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		if (num == currentIndex && num2 == currentCount && flag != isShift)
		{
			shiftSequence.Append(DOTweenModuleUI.DOAnchorPosX(base.rectTransform, idlePosition.x, playerItemAnimConfig.rightShiftTime, false));
		}
		else if (num != currentIndex || num2 != currentCount)
		{
			shiftSequence.Append(DOTweenModuleUI.DOAnchorPosX(base.rectTransform, idlePosition.x, playerItemAnimConfig.leftShiftTime, false));
		}
		else
		{
			shiftSequence.Append(DOTweenModuleUI.DOAnchorPosX(base.rectTransform, idlePosition.x, playerItemAnimConfig.leftShiftTime, false));
		}
		DG.Tweening.Sequence obj2 = shiftSequence;
		obj2.onKill = (TweenCallback)Delegate.Combine(obj2.onKill, (TweenCallback)delegate
		{
			if (sequenceForShift == shiftSequence)
			{
				sequenceForShift = null;
				currentPositionState = PositionState.Idle;
			}
		});
		shiftSequence.Play();
		UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
	}

	private void RestoreIdlePose()
	{
		if (playingStateAnim || currentState != State.Idle || UIDisplayModule.gameState.UIManager.playerItemArea.blockItemDisplayUpdate)
		{
			return;
		}
		DG.Tweening.Sequence obj = sequence;
		if (obj != null && obj.IsActive())
		{
			return;
		}
		DG.Tweening.Sequence obj2 = sequenceForIdlePose;
		if ((obj2 != null && obj2.IsActive()) || currentIndex == -1)
		{
			return;
		}
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		Vector2 idlePosition = GetIdlePosition(isShift);
		Quaternion b = Quaternion.Euler(playerItemAnimConfig.idleRotation);
		if (Mathf.Abs(base.rectTransform.anchoredPosition.y - idlePosition.y) <= 0.1f && Mathf.Abs(base.rectTransform.localPosition.z) <= 0.1f && Quaternion.Angle(base.rectTransform.localRotation, b) <= 0.1f && (base.rectTransform.localScale - Vector3.one).sqrMagnitude <= 0.0001f)
		{
			return;
		}
		DG.Tweening.Sequence idlePoseSequence = DOTween.Sequence();
		sequenceForIdlePose = idlePoseSequence;
		idlePoseSequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		idlePoseSequence.Insert(0f, DOTweenModuleUI.DOAnchorPosY(base.rectTransform, idlePosition.y, playerItemAnimConfig.unselectTime, false));
		idlePoseSequence.Insert(0f, base.rectTransform.DOLocalMoveZ(0f, playerItemAnimConfig.unselectTime));
		idlePoseSequence.Insert(0f, base.rectTransform.DOLocalRotate(playerItemAnimConfig.idleRotation, playerItemAnimConfig.unselectTime));
		idlePoseSequence.Insert(0f, base.rectTransform.DOScale(Vector3.one, playerItemAnimConfig.unselectTime));
		DG.Tweening.Sequence obj3 = idlePoseSequence;
		obj3.onKill = (TweenCallback)Delegate.Combine(obj3.onKill, (TweenCallback)delegate
		{
			if (sequenceForIdlePose == idlePoseSequence)
			{
				sequenceForIdlePose = null;
			}
		});
		idlePoseSequence.Play();
	}

	private void UpdateHideIntend()
	{
		bool flag = base.slotDisplay.item.IsHidden(UIDisplayModule.gameState);
		if (isIntentHidden != flag)
		{
			isIntentHidden = flag;
			if (isIntentHidden)
			{
				OnHideIntend();
			}
			else
			{
				OnShowIntend();
			}
		}
	}

	private void OnHideIntend()
	{
		sequenceForVisibility?.Kill();
		DG.Tweening.Sequence visibilitySequence = DOTween.Sequence();
		sequenceForVisibility = visibilitySequence;
		visibilitySequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		visibilitySequence.Append(contentTransform.DOScaleX(0f, playerItemAnimConfig.hideIntentTime / 2f));
		visibilitySequence.AppendCallback(delegate
		{
			contentTransform.gameObject.SetActive(value: false);
			hiddenTransform.gameObject.SetActive(value: true);
		});
		visibilitySequence.Append(hiddenTransform.DOScaleX(0f, 0f));
		visibilitySequence.Append(hiddenTransform.DOScaleX(1f, playerItemAnimConfig.hideIntentTime / 2f));
		visibilitySequence.onKill = delegate
		{
			if (sequenceForVisibility == visibilitySequence)
			{
				contentTransform.localScale = Vector3.one;
				hiddenTransform.localScale = Vector3.one;
				contentTransform.gameObject.SetActive(value: false);
				hiddenTransform.gameObject.SetActive(value: true);
				sequenceForVisibility = null;
				base.slotDisplay.SetDirty();
			}
		};
		visibilitySequence.Play();
	}

	private void OnShowIntend()
	{
		sequenceForVisibility?.Kill();
		DG.Tweening.Sequence visibilitySequence = DOTween.Sequence();
		sequenceForVisibility = visibilitySequence;
		visibilitySequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		visibilitySequence.Append(hiddenTransform.DOScale(0f, playerItemAnimConfig.hideIntentTime / 2f));
		visibilitySequence.AppendCallback(delegate
		{
			contentTransform.gameObject.SetActive(value: true);
			hiddenTransform.gameObject.SetActive(value: false);
		});
		visibilitySequence.Append(contentTransform.DOScaleX(0f, 0f));
		visibilitySequence.Append(contentTransform.DOScaleX(1f, playerItemAnimConfig.hideIntentTime / 2f));
		visibilitySequence.onKill = delegate
		{
			if (sequenceForVisibility == visibilitySequence)
			{
				contentTransform.localScale = Vector3.one;
				hiddenTransform.localScale = Vector3.one;
				contentTransform.gameObject.SetActive(value: true);
				hiddenTransform.gameObject.SetActive(value: false);
				sequenceForVisibility = null;
				base.slotDisplay.SetDirty();
			}
		};
		visibilitySequence.Play();
	}

	public IEnumerator TriggerAbilityAnim(Ability.AbilityAnimPriority priority, Ability.AbilityAnimType abilityAnimType, float timeScale)
	{
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		if (priority == Ability.AbilityAnimPriority.Weak)
		{
			UIDisplayModule.gameState.StartCoroutine(() => PlayIconEffect());
		}
		else
		{
			UIDisplayModule.gameState.StartCoroutine(() => PlayIconEffect(4));
		}
		UIDisplayModule.gameState.StartCoroutine(() => PlayPassiveEffect(abilityAnimType));
		if (priority < Ability.AbilityAnimPriority.Medium)
		{
			yield break;
		}
		bool flag = currentState == State.PlayingMutualExcludeAnim && currentAnimType == AnimType.TriggerAbilityAnim;
		bool flag2 = !flag || priority > currentAbilityAnimPriority;
		if (!flag)
		{
			KillSequenceForAbilityStart();
			currentState = State.PlayingMutualExcludeAnim;
			currentAnimType = AnimType.TriggerAbilityAnim;
			currentAbilityAnimPriority = priority;
			RefreshData();
		}
		else if (priority > currentAbilityAnimPriority)
		{
			currentAbilityAnimPriority = priority;
		}
		RestartAbilityEndSequence(playerItemAnimConfig.passiveSpellTime1);
		if (!flag2)
		{
			yield break;
		}
		sequence?.Kill();
		DG.Tweening.Sequence abilitySequence = DOTween.Sequence();
		sequence = abilitySequence;
		abilitySequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		if (currentAbilityAnimPriority == Ability.AbilityAnimPriority.Medium)
		{
			abilitySequence.Append(base.rectTransform.DOLocalRotate(Vector3.zero, playerItemAnimConfig.passiveSpellTime1));
			abilitySequence.Join(DOTweenModuleUI.DOAnchorPosY(base.rectTransform, playerItemAnimConfig.passiveOffset1.y, playerItemAnimConfig.passiveSpellTime1, false));
		}
		else
		{
			if (!flag)
			{
				abilitySequence.Append(base.rectTransform.DOScale(new Vector3(0f, 1f, 1f), 0f));
			}
			abilitySequence.Append(base.rectTransform.DOLocalRotate(Vector3.zero, playerItemAnimConfig.passiveSpellTime1));
			abilitySequence.Join(DOTweenModuleUI.DOAnchorPosY(base.rectTransform, playerItemAnimConfig.passiveOffset2.y, playerItemAnimConfig.passiveSpellTime1, false));
			abilitySequence.Join(base.rectTransform.DOScale(Vector3.one, playerItemAnimConfig.passiveSpellTime1));
		}
		DG.Tweening.Sequence obj = abilitySequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			if (sequence == abilitySequence)
			{
				sequence = null;
			}
		});
		abilitySequence.Play();
		UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
		yield return abilitySequence.WaitForKill();
	}

	private void RestartAbilityEndSequence(float delay)
	{
		sequenceForAbilityEnd?.Kill();
		DG.Tweening.Sequence abilityEndSequence = DOTween.Sequence();
		sequenceForAbilityEnd = abilityEndSequence;
		abilityEndSequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		abilityEndSequence.AppendInterval(delay);
		DG.Tweening.Sequence obj = abilityEndSequence;
		obj.onComplete = (TweenCallback)Delegate.Combine(obj.onComplete, (TweenCallback)delegate
		{
			if (sequenceForAbilityEnd == abilityEndSequence && currentState == State.PlayingMutualExcludeAnim && currentAnimType == AnimType.TriggerAbilityAnim)
			{
				currentState = State.OutOfControl;
				currentAnimType = AnimType.None;
				currentAbilityAnimPriority = Ability.AbilityAnimPriority.None;
				UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
			}
		});
		DG.Tweening.Sequence obj2 = abilityEndSequence;
		obj2.onKill = (TweenCallback)Delegate.Combine(obj2.onKill, (TweenCallback)delegate
		{
			if (sequenceForAbilityEnd == abilityEndSequence)
			{
				sequenceForAbilityEnd = null;
			}
		});
		abilityEndSequence.Play();
	}

	public IEnumerator PlayTween_HintOverload()
	{
		KillAllSequence();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
		currentAnimType = AnimType.PlayTween_HintOverload;
		RefreshData();
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		Vector2 idlePosition = GetIdlePosition(isShift);
		sequence.Append(DOTweenModuleUI.DOAnchorPos(base.rectTransform, idlePosition, 0f, false));
		sequence.Join(base.rectTransform.DOScale(playerItemAnimConfig.idleScale, 0f));
		sequence.Join(base.rectTransform.DOLocalRotate(playerItemAnimConfig.idleRotation, 0f));
		if (base.slotDisplay.item.IsOverflow)
		{
			sequence.Append(DOTweenModuleUI.DOAnchorPos(base.rectTransform, idlePosition + playerItemAnimConfig.hintOverloadOffset, playerItemAnimConfig.hintOverloadTime1, false));
			sequence.Append(DOTweenModuleUI.DOAnchorPos(base.rectTransform, idlePosition, playerItemAnimConfig.hintOverloadTime2, false));
		}
		else
		{
			sequence.AppendInterval(playerItemAnimConfig.hintOverloadTime2);
		}
		DG.Tweening.Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
			currentAnimType = AnimType.None;
			UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator PlayTween_Draw()
	{
		KillAllSequence();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
		currentAnimType = AnimType.PlayTween_Draw;
		RefreshData();
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		base.gameObject.SetActive(value: true);
		base.rectTransform.position = GetDrawPilePosition();
		base.rectTransform.localScale = playerItemAnimConfig.unDrawScale;
		base.rectTransform.localRotation = Quaternion.Euler(playerItemAnimConfig.unDrawRotation);
		UIDisplayModule.gameState.UIManager.battlePanel.drawPileButton.PlayFlick();
		AppendToIdleSequence(sequence, playerItemAnimConfig.drawTime);
		DG.Tweening.Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
			currentAnimType = AnimType.None;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator PlayTween_AddNew(Item source, int insertIndex)
	{
		KillAllSequence();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
		currentAnimType = AnimType.PlayTween_AddNew;
		RefreshIndex();
		currentIndex = insertIndex;
		Vector2 idlePosition = GetIdlePosition(insertIndex, UIDisplayModule.gameState.playerState.TableContainer.ItemsCount + 1, isShift);
		_ = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		ItemDisplayBase.EnemyAnimConfig enemyItemAnimConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.enemyItemAnimConfig;
		ItemInfoDisplayBase.Config itemInfoDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.itemInfoDisplayConfig;
		Vector3 vector = Vector3.Lerp(base.rectTransform.AnchoredToWorldPosition(idlePosition), UIDisplayModule.gameState.UIManager.battleItemInfoPanel.enemyPivot.ToCanvasWorldPosition(rootCameraCanvas), 1f - itemInfoDisplayConfig.showInfoOffsetFactor);
		base.rectTransform.position = vector;
		base.rectTransform.localScale = enemyItemAnimConfig.showInfoScale;
		base.rectTransform.localRotation = Quaternion.Euler(enemyItemAnimConfig.showInfoRotate);
		base.gameObject.SetActive(value: false);
		UIDisplayModule.gameState.UIManager.battleItemInfoPanel.ConfigTween_Insert(sequence, base.slotDisplay, source, vector);
		sequence.AppendCallback(delegate
		{
			base.slotDisplay.gameObject.SetActive(value: true);
		});
		AppendToIdleSequence(sequence, enemyItemAnimConfig.showInfoTime);
		DG.Tweening.Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
			currentAnimType = AnimType.None;
			base.slotDisplay.gameObject.SetActive(value: true);
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator PlayTween_Add(Vector3? fromPosition)
	{
		KillAllSequence();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
		currentAnimType = AnimType.PlayTween_Add;
		RefreshData();
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		Vector2 idlePosition = GetIdlePosition(isShift);
		if (!fromPosition.HasValue)
		{
			fromPosition = idlePosition;
		}
		base.gameObject.SetActive(value: true);
		base.rectTransform.transform.position = fromPosition.Value;
		base.rectTransform.localScale = playerItemAnimConfig.unDrawScale;
		base.rectTransform.localRotation = Quaternion.Euler(playerItemAnimConfig.unDrawRotation);
		AppendToIdleSequence(sequence, playerItemAnimConfig.drawTime);
		DG.Tweening.Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
			currentAnimType = AnimType.None;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator PlayTween_Remove()
	{
		KillAllSequence();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
		currentAnimType = AnimType.PlayTween_Remove;
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		ConvertablePivot pivot = base.slotDisplay.item.GetPivot(UIDisplayModule.gameState);
		if (pivot == null)
		{
			sequence = null;
			currentState = State.OutOfControl;
			currentAnimType = AnimType.None;
			yield break;
		}
		Vector3 endValue = pivot.ToCanvasWorldPosition(rootCameraCanvas);
		RefreshData();
		sequence.Append(base.rectTransform.DOMove(endValue, playerItemAnimConfig.drawTime));
		sequence.Join(base.rectTransform.DOScale(playerItemAnimConfig.unDrawScale, 0f));
		sequence.Join(base.rectTransform.DOLocalRotate(playerItemAnimConfig.unDrawRotation, playerItemAnimConfig.drawTime));
		ItemContainer owner = base.item.owner;
		if (owner != null && owner.type == ItemContainer.ContainerType.WastePile && (base.item.OwnerBft?.isPlayer ?? false))
		{
			sequence.AppendCallback(delegate
			{
				UIDisplayModule.gameState.UIManager.battlePanel.wastePileButton.PlayFlick();
			});
		}
		DG.Tweening.Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
			currentAnimType = AnimType.None;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator PlayTween_TransferToEnemy()
	{
		KillAllSequence();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
		currentAnimType = AnimType.PlayTween_TransferToEnemy;
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		RefreshData();
		UIDisplayModule.gameState.UIManager.enemyBftUIDisplay.tableItemArea.ConfigTransferFromPlayerTween1(sequence, base.item);
		Vector3 position = UIDisplayModule.gameState.UIManager.enemyBftUIDisplay.tableItemArea.GetItemDisplay(base.item).transform.position;
		sequence.Append(base.rectTransform.DOMove(position, playerItemAnimConfig.drawTime));
		sequence.Join(base.rectTransform.DOScale(playerItemAnimConfig.idleScale, 0f));
		sequence.Join(base.rectTransform.DOLocalRotate(playerItemAnimConfig.idleRotation, playerItemAnimConfig.drawTime));
		UIDisplayModule.gameState.UIManager.enemyBftUIDisplay.tableItemArea.ConfigTransferFromPlayerTween2(sequence, base.item);
		DG.Tweening.Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
			currentAnimType = AnimType.None;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator PlayTween_TransferFromEnemy()
	{
		KillAllSequence();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
		currentAnimType = AnimType.PlayTween_TransferFromEnemy;
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		EnemyItemDisplay itemDisplay = UIDisplayModule.gameState.UIManager.enemyBftUIDisplay.tableItemArea.GetItemDisplay(base.item);
		RefreshData();
		itemDisplay.gameObject.SetActive(value: false);
		base.gameObject.SetActive(value: true);
		base.rectTransform.transform.position = itemDisplay.transform.position;
		base.rectTransform.localScale = playerItemAnimConfig.unDrawScale;
		base.rectTransform.localRotation = Quaternion.Euler(playerItemAnimConfig.unDrawRotation);
		AppendToIdleSequence(sequence, playerItemAnimConfig.drawTime);
		DG.Tweening.Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
			currentAnimType = AnimType.None;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator PlayTween_Exile()
	{
		KillAllSequence();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
		currentAnimType = AnimType.PlayTween_Exile;
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		EffectModule module = base.slotDisplay.GetModule<EffectModule>();
		RefreshData();
		base.slotDisplay.GetModule<CostModule>().OnExile();
		base.slotDisplay.GetModule<CounterModule>().OnExile();
		module.AppendEffectTween(sequence, 0f, playerItemAnimConfig.drawTime);
		DG.Tweening.Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
			currentAnimType = AnimType.None;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator PlayTween_Spell()
	{
		itemSpellImage.gameObject.SetActive(value: false);
		itemSpellImage.color = itemSpellImage.color.WithA(1f);
		UIDisplayModule.gameState.applicationState.nfxManager.SpawnAndPlay(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_itemSpell, base.slotDisplay.pivot.GetVFXRoot(UIDisplayModule.gameState), base.slotDisplay.pivot.WorldPosition);
		yield break;
	}

	public IEnumerator PlayTween_Move()
	{
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		int num = currentIndex;
		int num2 = currentCount;
		bool flag = isShift;
		RefreshData();
		if (num != currentIndex || num2 != currentCount || flag != isShift)
		{
			KillAllSequence();
			sequence = DOTween.Sequence();
			sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
			currentState = State.PlayingMutualExcludeAnim;
			currentAnimType = AnimType.PlayTween_Move;
			Vector2 idlePosition = GetIdlePosition(isShift);
			if (num == currentIndex && num2 == currentCount && flag != isShift)
			{
				sequence.Append(DOTweenModuleUI.DOAnchorPos(base.rectTransform, idlePosition, playerItemAnimConfig.rightShiftTime, false));
			}
			else if (num != currentIndex || num2 != currentCount)
			{
				sequence.Append(DOTweenModuleUI.DOAnchorPos(base.rectTransform, idlePosition, playerItemAnimConfig.leftShiftTime, false));
			}
			DG.Tweening.Sequence obj = sequence;
			obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
			{
				sequence = null;
				currentState = State.OutOfControl;
				currentAnimType = AnimType.None;
			});
			sequence.Play();
			yield return sequence.WaitForKill();
		}
	}

	public IEnumerator OnSwap()
	{
		KillAllSequence();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
		currentAnimType = AnimType.OnSwap;
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		RefreshData();
		Vector2 anchoredPosition = base.rectTransform.anchoredPosition;
		anchoredPosition.x = GetIdlePosition(isShift).x;
		sequence.Append(DOTweenModuleUI.DOAnchorPos(base.rectTransform, anchoredPosition, playerItemAnimConfig.switchTime, false));
		DG.Tweening.Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
			currentAnimType = AnimType.None;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator OnShuffle()
	{
		KillAllSequence();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
		currentAnimType = AnimType.OnShuffle;
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		RefreshData();
		Vector2 idlePosition = GetIdlePosition(isShift);
		sequence.Append(DOTweenModuleUI.DOAnchorPos(base.rectTransform, GetMiddlePosition(), playerItemAnimConfig.shuffleTime1, false));
		sequence.Join(base.rectTransform.DOScale(playerItemAnimConfig.idleScale, playerItemAnimConfig.shuffleTime1));
		sequence.Join(base.rectTransform.DOLocalRotate(playerItemAnimConfig.idleRotation, playerItemAnimConfig.shuffleTime1));
		sequence.AppendInterval(playerItemAnimConfig.shuffleTime2);
		sequence.Append(DOTweenModuleUI.DOAnchorPos(base.rectTransform, idlePosition, playerItemAnimConfig.shuffleTime3, false));
		DG.Tweening.Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
			currentAnimType = AnimType.None;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator OnConvert()
	{
		KillAllSequence();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		currentState = State.PlayingMutualExcludeAnim;
		currentAnimType = AnimType.OnConvert;
		RefreshData();
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		Vector2 idlePosition = GetIdlePosition(isShift);
		sequence.Append(DOTweenModuleUI.DOAnchorPos(base.rectTransform, idlePosition, playerItemAnimConfig.drawTime, false));
		sequence.Join(base.rectTransform.DOScale(playerItemAnimConfig.idleScale, playerItemAnimConfig.drawTime));
		sequence.Join(base.rectTransform.DOLocalRotate(playerItemAnimConfig.idleRotation, playerItemAnimConfig.drawTime));
		DG.Tweening.Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
			currentAnimType = AnimType.None;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator TriggerStanceAbilityAnim(Ability.AbilityAnimPriority priority)
	{
		UIDisplayModule.gameState.actionManager.AcquireLock(ActionLock.Type.Default);
		RefreshData();
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		Vector2 anchoredPosition = base.rectTransform.anchoredPosition;
		anchoredPosition.y = playerItemAnimConfig.spellPositionY;
		Vector2 anchoredPosition2 = base.rectTransform.anchoredPosition;
		KillAllSequence();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		if (priority == Ability.AbilityAnimPriority.Strong)
		{
			sequence.Append(base.rectTransform.DOLocalRotate(Vector3.zero, playerItemAnimConfig.passiveSpellTime1));
			sequence.Join(DOTweenModuleUI.DOAnchorPos(base.rectTransform, anchoredPosition2 + playerItemAnimConfig.passiveOffset1, playerItemAnimConfig.passiveSpellTime1, false));
		}
		if (priority >= Ability.AbilityAnimPriority.Weak)
		{
			sequence.JoinCallback(delegate
			{
				StartCoroutine(PlayStanceEffect());
			});
		}
		DG.Tweening.Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
			UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
		});
		sequence.Play();
		UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
		yield return sequence.WaitForKill();
		UIDisplayModule.gameState.actionManager.ReleaseLock(ActionLock.Type.Default);
	}

	public IEnumerator TriggerEnchantAbilityAnim(Ability.AbilityAnimPriority priority, Enchant enchant)
	{
		UIDisplayModule.gameState.actionManager.AcquireLock(ActionLock.Type.Default);
		RefreshData();
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		Vector2 anchoredPosition = base.rectTransform.anchoredPosition;
		anchoredPosition.y = playerItemAnimConfig.spellPositionY;
		Vector2 anchoredPosition2 = base.rectTransform.anchoredPosition;
		KillAllSequence();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		if (priority == Ability.AbilityAnimPriority.Strong)
		{
			sequence.Append(base.rectTransform.DOLocalRotate(Vector3.zero, playerItemAnimConfig.passiveSpellTime1));
			sequence.Join(DOTweenModuleUI.DOAnchorPos(base.rectTransform, anchoredPosition2 + playerItemAnimConfig.passiveOffset1, playerItemAnimConfig.passiveSpellTime1, false));
		}
		if (priority == Ability.AbilityAnimPriority.Medium || priority == Ability.AbilityAnimPriority.Strong)
		{
			sequence.JoinCallback(delegate
			{
				UIDisplayModule.gameState.StartCoroutine(() => PlayEnchantEffect(enchant));
			});
		}
		DG.Tweening.Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
			UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
		});
		sequence.Play();
		UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
		yield return sequence.WaitForKill();
		UIDisplayModule.gameState.actionManager.ReleaseLock(ActionLock.Type.Default);
	}

	public IEnumerator TriggerRuneAbilityAnim(Ability.AbilityAnimPriority priority, Rune rune)
	{
		UIDisplayModule.gameState.actionManager.AcquireLock(ActionLock.Type.Default);
		RefreshData();
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.applicationState.vfxDataPreset.playerItemAnimConfig;
		Vector2 anchoredPosition = base.rectTransform.anchoredPosition;
		anchoredPosition.y = playerItemAnimConfig.spellPositionY;
		Vector2 anchoredPosition2 = base.rectTransform.anchoredPosition;
		KillAllSequence();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		if (priority == Ability.AbilityAnimPriority.Strong)
		{
			sequence.Append(base.rectTransform.DOLocalRotate(Vector3.zero, playerItemAnimConfig.passiveSpellTime1));
			sequence.Join(DOTweenModuleUI.DOAnchorPos(base.rectTransform, anchoredPosition2 + playerItemAnimConfig.passiveOffset1, playerItemAnimConfig.passiveSpellTime1, false));
		}
		if (priority == Ability.AbilityAnimPriority.Medium || priority == Ability.AbilityAnimPriority.Strong)
		{
			sequence.JoinCallback(delegate
			{
				UIDisplayModule.gameState.StartCoroutine(() => PlayRuneEffect(rune));
			});
		}
		DG.Tweening.Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			currentState = State.OutOfControl;
			UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
		});
		sequence.Play();
		UIDisplayModule.gameState.UIManager.playerItemArea.SetDirty();
		yield return sequence.WaitForKill();
		UIDisplayModule.gameState.actionManager.ReleaseLock(ActionLock.Type.Default);
	}

	public void TriggerAbilityEndAnim()
	{
	}

	public IEnumerator PlayIconEffect(int times = 1)
	{
		Image itemEffectImage = UIDisplayModule.gameState.UIManager.playerItemArea.itemEffectImage;
		List<Image> images = new List<Image>();
		DG.Tweening.Sequence sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		for (int i = 0; i < times; i++)
		{
			Image image = UnityEngine.Object.Instantiate(itemEffectImage, base.transform);
			image.gameObject.SetActive(value: true);
			image.sprite = base.slotDisplay.item.GetSkinIcon(UIDisplayModule.applicationState);
			image.rectTransform.position = base.transform.position;
			image.rectTransform.localScale = Vector3.one;
			images.Add(image);
			sequence.Insert((float)i * 0.25f, DOTweenModuleUI.DOFade(image, 0f, 1f));
			sequence.Insert((float)i * 0.25f, image.rectTransform.DOScale(new Vector3(2f, 2f, 1f), 1f));
		}
		sequence.Play();
		yield return sequence.WaitForKill();
		for (int j = 0; j < times; j++)
		{
			if (images[j]?.gameObject != null)
			{
				UnityEngine.Object.Destroy(images[j].gameObject);
			}
		}
	}

	private IEnumerator PlayStanceEffect(int times = 1)
	{
		yield return 0;
	}

	private IEnumerator PlayEnchantEffect(Enchant enchant, int times = 1)
	{
		Image enchantEffectImage = UIDisplayModule.gameState.UIManager.playerItemArea.enchantEffectImage;
		List<Image> images = new List<Image>();
		DG.Tweening.Sequence sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		for (int i = 0; i < times; i++)
		{
			Image image = UnityEngine.Object.Instantiate(enchantEffectImage, enchantEffectImage.transform.parent);
			image.gameObject.SetActive(value: true);
			image.sprite = enchant.def.Icon;
			image.rectTransform.parent = base.slotDisplay.GetModule<EnchantModule>().GetEnchantDisplay(enchant).transform;
			image.rectTransform.position = base.slotDisplay.GetModule<EnchantModule>().GetEnchantDisplay(enchant).transform.position;
			image.rectTransform.localScale = Vector3.one;
			images.Add(image);
			sequence.Insert((float)i * 0.25f, DOTweenModuleUI.DOFade(image, 0f, 1f));
			sequence.Insert((float)i * 0.25f, image.rectTransform.DOScale(new Vector3(3f, 3f, 1f), 1f));
		}
		sequence.Play();
		yield return sequence.WaitForKill();
		for (int j = 0; j < times; j++)
		{
			if (images[j]?.gameObject != null)
			{
				UnityEngine.Object.Destroy(images[j].gameObject);
			}
		}
	}

	private IEnumerator PlayRuneEffect(Rune rune, int times = 1)
	{
		Image enchantEffectImage = UIDisplayModule.gameState.UIManager.playerItemArea.enchantEffectImage;
		List<Image> images = new List<Image>();
		DG.Tweening.Sequence sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		for (int i = 0; i < times; i++)
		{
			Image image = UnityEngine.Object.Instantiate(enchantEffectImage, enchantEffectImage.transform.parent);
			image.gameObject.SetActive(value: true);
			image.sprite = rune.def.Icon;
			image.rectTransform.parent = base.slotDisplay.GetModule<RuneModule>().GetRuneDisplay(rune).transform;
			image.rectTransform.position = base.slotDisplay.GetModule<RuneModule>().GetRuneDisplay(rune).transform.position;
			image.rectTransform.localScale = Vector3.one;
			images.Add(image);
			sequence.Insert((float)i * 0.25f, DOTweenModuleUI.DOFade(image, 0f, 1f));
			sequence.Insert((float)i * 0.25f, image.rectTransform.DOScale(new Vector3(3f, 3f, 1f), 1f));
		}
		sequence.Play();
		yield return sequence.WaitForKill();
		for (int j = 0; j < times; j++)
		{
			if (images[j]?.gameObject != null)
			{
				UnityEngine.Object.Destroy(images[j].gameObject);
			}
		}
	}

	private IEnumerator PlayPassiveEffect(Ability.AbilityAnimType abilityAnimType)
	{
		ClearPassiveEffect();
		SkeletonGraphic skeletonGraphic;
		TrackEntry currentTrack;
		switch (abilityAnimType)
		{
		case Ability.AbilityAnimType.Positive:
			skeletonGraphic = positivePassive;
			currentTrack = skeletonGraphic.PlayAnimation("卡牌特效-光辉", loop: false);
			break;
		case Ability.AbilityAnimType.Negative:
			skeletonGraphic = negativePassive;
			currentTrack = skeletonGraphic.PlayAnimation("卡牌特效-黑雾", loop: false);
			break;
		case Ability.AbilityAnimType.None:
			yield break;
		default:
			throw new ArgumentOutOfRangeException("abilityAnimType", abilityAnimType, null);
		}
		track = currentTrack;
		((Component)(object)skeletonGraphic).gameObject.SetActive(value: true);
		bool finished = false;
		currentTrack.Complete += Finish;
		currentTrack.End += Finish;
		yield return new WaitUntil(() => finished);
		currentTrack.Complete -= Finish;
		currentTrack.End -= Finish;
		void Finish(TrackEntry finishedTrack)
		{
			if (!finished && finishedTrack == currentTrack)
			{
				finished = true;
				if (track == currentTrack)
				{
					track = null;
					((Component)(object)skeletonGraphic).gameObject.SetActive(value: false);
				}
			}
		}
	}

	private void ClearPassiveEffect()
	{
		positivePassive.AnimationState?.ClearTracks();
		negativePassive.AnimationState?.ClearTracks();
		((Component)(object)positivePassive).gameObject.SetActive(value: false);
		((Component)(object)negativePassive).gameObject.SetActive(value: false);
		track = null;
	}

	private IEnumerator PlayFullFilledCost()
	{
		itemSpellImage.gameObject.SetActive(value: false);
		itemSpellImage.color = itemSpellImage.color.WithA(1f);
		UIDisplayModule.gameState.applicationState.nfxManager.SpawnAndPlay(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_itemFullFilledCost, base.slotDisplay.transform);
		yield break;
	}

	private void OnDrawGizmos()
	{
		GizmosHelper.DrawString(currentState.ToString(), base.transform.position);
	}
}
