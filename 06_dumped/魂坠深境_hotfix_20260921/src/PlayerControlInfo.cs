using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using ContainerOfSoul.Display.Game.BattleDisplay;
using UnityEngine;

public class PlayerControlInfo
{
	public enum State
	{
		None,
		Prepare,
		ClickSelected,
		DragSelecting,
		GettingTarget,
		TargetSelected
	}

	public class SpellRequest
	{
		public float interval;

		public Item spellItem;

		public Item targetItem;

		public bool withAnim;

		public bool waitForAnimComplete;
	}

	public class ItemInfo
	{
		public bool added;

		public Item addedToItem;

		public int index;

		public Item item;

		public bool shift;
	}

	private const int InvalidPointerId = int.MinValue;

	[NonSerialized]
	public DisplaySpellCastInfo displaySpellInfo;

	[NonSerialized]
	public Dictionary<Item, CastInfoCache> castInfoCaches;

	[NonSerialized]
	public Vector3? frameDragPosition;

	[NonSerialized]
	private readonly GameStateInfo gameState;

	[NonSerialized]
	public PooledList<ItemInfo> itemInfos;

	[NonSerialized]
	private readonly Dictionary<Item, ItemInfo> itemInfoByItem;

	[NonSerialized]
	private ItemDisplayBase pointerClicked;

	[NonSerialized]
	private ItemDisplayBase pointerBeginDrag;

	[NonSerialized]
	private ItemDisplayBase pointerDrag;

	[NonSerialized]
	private ItemDisplayBase pointerEndDrag;

	[NonSerialized]
	private ItemDisplayBase pointerEnter;

	[NonSerialized]
	private ItemDisplayBase pointerExit;

	[NonSerialized]
	private ItemDisplayBase pointerUp;

	[NonSerialized]
	public ItemDisplayBase pointerOver;

	[NonSerialized]
	public ItemDisplayBase rawOver;

	[NonSerialized]
	public Queue<SpellRequest> requests;

	[NonSerialized]
	public bool inSpellArea;

	[NonSerialized]
	public bool isCasting;

	[NonSerialized]
	private Item holdRepeatCastItem;

	[NonSerialized]
	private int holdRepeatCastPointerId = int.MinValue;

	[NonSerialized]
	private float holdRepeatCastStartTime = -1f;

	[NonSerialized]
	private float nextHoldRepeatCastTime;

	[NonSerialized]
	private int holdRepeatCastCount;

	[NonSerialized]
	private bool holdRepeatCastTriggered;

	[NonSerialized]
	private bool holdRepeatCastStopped;

	[NonSerialized]
	private bool holdRepeatCastInSpellArea;

	public State state;

	public Item selected;

	public PlayerItemArea playerItemArea => gameState.UIManager.playerItemArea;

	public EnemyItemArea enemyItemArea => gameState.UIManager.enemyBftUIDisplay.tableItemArea;

	public RealSpellCastInfo RealSpellCastInfo => gameState.playerState.playerSpellInfo;

	public Item spellItem => displaySpellInfo.spellItem;

	public Item targetItem => displaySpellInfo.targetItem;

	public PlayerControlInfo(GameStateInfo gameState)
	{
		requests = new Queue<SpellRequest>();
		castInfoCaches = new Dictionary<Item, CastInfoCache>();
		displaySpellInfo = new DisplaySpellCastInfo(gameState);
		this.gameState = gameState;
		state = State.Prepare;
		pointerClicked = null;
		pointerEnter = null;
		pointerExit = null;
		pointerUp = null;
		pointerBeginDrag = null;
		pointerDrag = null;
		pointerEndDrag = null;
		itemInfos = new PooledList<ItemInfo>(15);
		itemInfoByItem = new Dictionary<Item, ItemInfo>(15);
	}

	public CastInfoCache GetCastInfoCache(Item item)
	{
		if (castInfoCaches.TryGetValue(item, out var value))
		{
			return value;
		}
		CastInfoCache castInfoCache = new CastInfoCache(item);
		castInfoCaches[item] = castInfoCache;
		return castInfoCache;
	}

	public void ClearCastInfoCache()
	{
		castInfoCaches.Clear();
	}

	public void ResetBattleRuntimeState()
	{
		ResetHoldRepeatCastGesture();
		ClearClickInfo();
		requests.Clear();
		frameDragPosition = null;
		pointerOver = null;
		selected = null;
		inSpellArea = false;
		isCasting = false;
		ClearCastInfoCache();
		CancelSpell();
	}

	public void UpdatePlayerControlInfo()
	{
		if (gameState.battleState.isInBattle)
		{
			ClearClickInfo();
			if (gameState.battleState.autoPlayState != BattleAutoPlayState.Playing)
			{
				HandleInput();
				UpdateHoldRepeatCast();
			}
			if (RealSpellCastInfo.castState == SpellCastInfo.State.Preparing && spellItem != null)
			{
				displaySpellInfo.AddCost(GameEvent.TriggerType.InBattleDisplay);
			}
			else
			{
				displaySpellInfo.ReturnAllCost();
			}
			ComputeFrameItemInfo();
			for (int i = 0; i < playerItemArea.itemDisplays.Count; i++)
			{
				playerItemArea.itemDisplays[i].stateModule.RefreshDisplay();
			}
			for (int j = 0; j < enemyItemArea.itemDisplays.Count; j++)
			{
				enemyItemArea.itemDisplays[j].stateModule.RefreshDisplay();
			}
			ShowItemInfo();
			itemInfos.Clear();
		}
	}

	private void HandleInput()
	{
		for (int i = 0; i < playerItemArea.itemDisplays.Count; i++)
		{
			RecordPlayerClickInfo(playerItemArea.itemDisplays[i]);
		}
		for (int j = 0; j < enemyItemArea.itemDisplays.Count; j++)
		{
			RecordPlayerClickInfo(enemyItemArea.itemDisplays[j]);
		}
		if (PlatformHelper.IsMobile())
		{
			rawOver = null;
		}
		HandleClick();
		HandlePointerExit();
		HandlePointerEnter();
		HandlePointerUp();
		HandleBeginDrag();
		HandleEndDrag();
		HandlePointerDrag();
		HandlePointerOver();
	}

	private void RecordPlayerClickInfo(PlayerItemDisplay itemDisplay)
	{
		if (itemDisplay.stateModule.currentState != PlayerTableDisplayStateModule.State.Added)
		{
			InputModule inputModule = itemDisplay.inputModule;
			if (inputModule.pointerClicked)
			{
				pointerClicked = itemDisplay;
			}
			if (inputModule.pointerEnter)
			{
				pointerEnter = itemDisplay;
			}
			if (inputModule.pointerExit)
			{
				pointerExit = itemDisplay;
			}
			if (inputModule.pointerUp || inputModule.endDrag)
			{
				pointerUp = itemDisplay;
			}
			if (inputModule.beginDrag)
			{
				pointerBeginDrag = itemDisplay;
			}
			if (inputModule.drag)
			{
				pointerDrag = itemDisplay;
			}
			if (inputModule.endDrag)
			{
				pointerEndDrag = itemDisplay;
			}
			if (inputModule.over || (inputModule.rawOver && inputModule.CheckIsBlockedByInfo()))
			{
				rawOver = itemDisplay;
			}
		}
	}

	private void RecordPlayerClickInfo(EnemyItemDisplay itemDisplay)
	{
		InputModule inputModule = itemDisplay.inputModule;
		if (inputModule.pointerClicked)
		{
			pointerClicked = itemDisplay;
		}
		if (inputModule.pointerEnter)
		{
			pointerEnter = itemDisplay;
		}
		if (inputModule.pointerExit)
		{
			pointerExit = itemDisplay;
		}
		if (inputModule.pointerUp || inputModule.endDrag)
		{
			pointerUp = itemDisplay;
		}
		if (inputModule.beginDrag)
		{
			pointerBeginDrag = itemDisplay;
		}
		if (inputModule.drag)
		{
			pointerDrag = itemDisplay;
		}
		if (inputModule.endDrag)
		{
			pointerEndDrag = itemDisplay;
		}
		if (inputModule.over || (inputModule.rawOver && inputModule.CheckIsBlockedByInfo()))
		{
			rawOver = itemDisplay;
		}
	}

	private void HandleClick()
	{
		if (pointerClicked == null || state == State.TargetSelected)
		{
			return;
		}
		selected = pointerClicked.item;
		Item item = pointerClicked.item;
		if (item != null && item.OwnerBft?.isPlayer == true)
		{
			if (pointerClicked.item == displaySpellInfo.spellItem && !inSpellArea)
			{
				CancelSpell();
			}
			else
			{
				SelectSpellItem(pointerClicked.item, isClicked: true);
			}
		}
	}

	private void HandleBeginDrag()
	{
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		if (pointerBeginDrag == null)
		{
			return;
		}
		Item item = pointerBeginDrag.item;
		if (item == null || item.OwnerBft?.isPlayer != true)
		{
			return;
		}
		BattleStateInfo battleState = gameState.battleState;
		if (battleState != null && battleState.turnPhase == TurnPhase.PlayerControlPhase)
		{
			inSpellArea = false;
			if (state != State.Prepare && pointerBeginDrag.item != displaySpellInfo.spellItem)
			{
				CancelSpell();
			}
			SelectSpellItem(pointerBeginDrag.item, isClicked: false);
			BeginHoldRepeatCast(pointerBeginDrag.item, pointerBeginDrag.itemSlotInputModule?.eventData?.pointerId ?? int.MinValue);
			Bootstrap.instance.audioManager.PlaySoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_card_drag);
		}
	}

	private void HandleEndDrag()
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!(pointerEndDrag == null))
		{
			bool num = CompleteHoldRepeatCastGesture(pointerEndDrag.item);
			Item item = pointerEndDrag.item;
			if (item != null && item.OwnerBft?.isPlayer == true)
			{
				Bootstrap.instance.audioManager.StopSoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_card_drag);
			}
			if (num)
			{
				CancelSpell();
				inSpellArea = false;
			}
		}
	}

	private void HandlePointerDrag()
	{
		if (pointerDrag == null)
		{
			return;
		}
		Item item = pointerDrag.item;
		if (item == null || item.OwnerBft?.isPlayer != true)
		{
			return;
		}
		BattleStateInfo battleState = gameState.battleState;
		if (battleState == null || battleState.turnPhase != TurnPhase.PlayerControlPhase)
		{
			return;
		}
		ItemDisplayBase itemDisplayBase = pointerDrag;
		InputModule itemSlotInputModule = pointerDrag.itemSlotInputModule;
		if (!itemDisplayBase.item.spellNeedsTarget && itemSlotInputModule.eventData != null)
		{
			Vector2 position = itemSlotInputModule.eventData.position;
			ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = gameState.applicationState.vfxDataPreset.playerItemAnimConfig;
			UpdateHoldRepeatCastDrag(itemDisplayBase.item, position.y / (float)Screen.height > playerItemAnimConfig.selectSpellOffset);
		}
		if (itemDisplayBase.item == displaySpellInfo.spellItem)
		{
			if (!displaySpellInfo.spellItem.spellNeedsTarget)
			{
				ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig2 = gameState.applicationState.vfxDataPreset.playerItemAnimConfig;
				Vector2 position2 = itemSlotInputModule.eventData.position;
				frameDragPosition = position2;
				inSpellArea = position2.y / (float)Screen.height > playerItemAnimConfig2.selectSpellOffset;
			}
			else
			{
				frameDragPosition = null;
				Canvas ui3DCameraCanvas = gameState.UIManager.ui3DCameraCanvas;
				Canvas mainCanvas = gameState.UIManager.mainCanvas;
				gameState.UIManager.playerBftUIDisplay.connectLineDisplay.SetValue(itemDisplayBase.transform.position.WorldPositionFromCanvasToCanvas(ui3DCameraCanvas, mainCanvas), itemSlotInputModule.eventData.position);
			}
		}
	}

	private void HandlePointerUp()
	{
		if (pointerUp == null)
		{
			return;
		}
		if (CompleteHoldRepeatCastGesture(pointerUp.item))
		{
			CancelSpell();
			inSpellArea = false;
			return;
		}
		Item item = pointerUp.item;
		if (item == null || item.OwnerBft?.isPlayer != true || (pointerClicked != null && !inSpellArea && displaySpellInfo.spellItem != displaySpellInfo.targetItem))
		{
			return;
		}
		if (displaySpellInfo.spellItem != pointerUp.item)
		{
			CancelSpell();
			return;
		}
		if (!gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.BattleAutomaton))
		{
			if (!displaySpellInfo.spellItem.spellNeedsTarget)
			{
				if (inSpellArea)
				{
					TryCastDisplaySpell();
				}
				else
				{
					CancelSpell();
				}
			}
			else if (displaySpellInfo.targetItem != null)
			{
				TryCastDisplaySpell();
			}
			else
			{
				CancelSpell();
			}
		}
		else
		{
			CancelSpell();
		}
		inSpellArea = false;
	}

	private void HandlePointerEnter()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if ((state == State.GettingTarget || state == State.TargetSelected) && !(pointerEnter == null))
		{
			Item item = pointerEnter.item;
			if (item != displaySpellInfo.targetItem && CheckItemAsTargetAble(item))
			{
				SelectSpellTarget(item);
				gameState.UIManager.battleItemInfoPanel.SetDirty();
			}
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_card_over);
		}
	}

	private void HandlePointerExit()
	{
		if (state == State.TargetSelected && Input.GetMouseButton(0) && !(pointerExit == null) && pointerExit.item == displaySpellInfo.targetItem)
		{
			SelectSpellTarget(null);
		}
	}

	private void HandlePointerOver()
	{
		if (pointerEnter != null)
		{
			pointerOver = pointerEnter;
		}
		else if (pointerExit != null)
		{
			pointerOver = null;
		}
	}

	private void ClearClickInfo()
	{
		pointerClicked = null;
		pointerEnter = null;
		pointerExit = null;
		pointerUp = null;
		pointerBeginDrag = null;
		pointerDrag = null;
		pointerEndDrag = null;
		rawOver = null;
	}

	private void BeginHoldRepeatCast(Item item, int pointerId)
	{
		ResetHoldRepeatCastGesture();
		if (TryGetHoldRepeatCastConfig(item, out var _) && !item.spellNeedsTarget)
		{
			holdRepeatCastItem = item;
			holdRepeatCastPointerId = pointerId;
		}
	}

	private void UpdateHoldRepeatCastDrag(Item item, bool isInSpellArea)
	{
		if (item == null || item != holdRepeatCastItem)
		{
			return;
		}
		holdRepeatCastInSpellArea = isInSpellArea;
		if (holdRepeatCastStopped)
		{
			return;
		}
		if (!isInSpellArea)
		{
			if (holdRepeatCastTriggered)
			{
				StopHoldRepeatCastContinuation();
			}
			else
			{
				holdRepeatCastStartTime = -1f;
			}
		}
		else if (holdRepeatCastStartTime < 0f)
		{
			holdRepeatCastStartTime = Time.unscaledTime;
		}
	}

	private void UpdateHoldRepeatCast()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (holdRepeatCastItem == null)
		{
			return;
		}
		if (!IsHoldRepeatPointerPressed())
		{
			ResetHoldRepeatCastGesture();
			CancelSpellSelection();
			inSpellArea = false;
			frameDragPosition = null;
			Bootstrap.instance.audioManager.StopSoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_card_drag);
		}
		else
		{
			if (holdRepeatCastStopped)
			{
				return;
			}
			if (!CanContinueHoldRepeatCast(out var config))
			{
				StopHoldRepeatCastContinuation();
			}
			else
			{
				if (!holdRepeatCastInSpellArea)
				{
					return;
				}
				float unscaledTime = Time.unscaledTime;
				if (holdRepeatCastStartTime < 0f)
				{
					holdRepeatCastStartTime = unscaledTime;
				}
				if (!holdRepeatCastTriggered)
				{
					if (unscaledTime - holdRepeatCastStartTime < Mathf.Max(0f, config.holdDuration))
					{
						return;
					}
					holdRepeatCastTriggered = true;
					nextHoldRepeatCastTime = unscaledTime;
				}
				if (!(unscaledTime < nextHoldRepeatCastTime) && !isCasting && RealSpellCastInfo.castState == SpellCastInfo.State.Preparing && gameState.actionManager.isEmpty)
				{
					if (!TryCastHoldRepeatSpell(holdRepeatCastItem))
					{
						StopHoldRepeatCastContinuation();
						return;
					}
					nextHoldRepeatCastTime = unscaledTime + config.GetIntervalAfterCast(holdRepeatCastCount);
					holdRepeatCastCount++;
				}
			}
		}
	}

	private bool TryCastHoldRepeatSpell(Item item)
	{
		if (!SpellCastInfo.SpellCastAble(gameState, item, null, inBattleDisplay: true, out var _))
		{
			return false;
		}
		UIHelper.HideAllInfo(null);
		RestoreHoldRepeatCastSelection(item);
		isCasting = true;
		gameState.StartCoroutine(() => CastHoldRepeatSpell(item));
		return true;
	}

	private IEnumerator CastHoldRepeatSpell(Item item)
	{
		yield return RealSpellCastInfo.TriggerSpellCast(item, null, SpellCastInfo.CasterType.Player);
		gameState.UIManager.playerItemArea.OnItemSpell(item);
		gameState.UIManager.playerItemArea.SetDirty();
		gameState.UIManager.enemyBftUIDisplay.tableItemArea.SetDirty();
		isCasting = false;
		if (holdRepeatCastItem == item && !holdRepeatCastStopped && CanContinueHoldRepeatCast(out var _))
		{
			RestoreHoldRepeatCastSelection(item);
		}
		else if (displaySpellInfo.spellItem == item)
		{
			displaySpellInfo.CancelSpell();
		}
	}

	private void RestoreHoldRepeatCastSelection(Item item)
	{
		displaySpellInfo.CancelSpell();
		displaySpellInfo.spellItem = item;
		displaySpellInfo.AddCost(GameEvent.TriggerType.InBattleDisplay);
		state = State.DragSelecting;
	}

	private bool CanContinueHoldRepeatCast(out HoldRepeatCastConfig config)
	{
		config = null;
		Item item = holdRepeatCastItem;
		if (!TryGetHoldRepeatCastConfig(item, out config))
		{
			return false;
		}
		if (!item.spellNeedsTarget)
		{
			ItemContainer owner = item.owner;
			if (owner != null && owner.type == ItemContainer.ContainerType.Table)
			{
				BattleFieldTransform ownerBft = item.OwnerBft;
				if (ownerBft != null && ownerBft.isPlayer)
				{
					if (!gameState.battleState.isInBattle || gameState.battleState.turnPhase != TurnPhase.PlayerControlPhase || !gameState.PlayerControlAble)
					{
						return false;
					}
					return !gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.BattleAutomaton);
				}
			}
		}
		return false;
	}

	private static bool TryGetHoldRepeatCastConfig(Item item, out HoldRepeatCastConfig config)
	{
		ItemDefinition itemDefinition = item?.GetDef();
		config = itemDefinition?.holdRepeatCastConfig;
		if ((object)itemDefinition != null && itemDefinition.holdRepeatCastEnabled)
		{
			return config != null;
		}
		return false;
	}

	private bool CompleteHoldRepeatCastGesture(Item item)
	{
		if (item == null || item != holdRepeatCastItem)
		{
			return false;
		}
		bool result = holdRepeatCastTriggered;
		ResetHoldRepeatCastGesture();
		return result;
	}

	private void StopHoldRepeatCastContinuation()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		holdRepeatCastStopped = true;
		holdRepeatCastInSpellArea = false;
		CancelSpellSelection();
		inSpellArea = false;
		frameDragPosition = null;
		Bootstrap.instance.audioManager.StopSoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_card_drag);
	}

	private void ResetHoldRepeatCastGesture()
	{
		holdRepeatCastItem = null;
		holdRepeatCastPointerId = int.MinValue;
		holdRepeatCastStartTime = -1f;
		nextHoldRepeatCastTime = 0f;
		holdRepeatCastCount = 0;
		holdRepeatCastTriggered = false;
		holdRepeatCastStopped = false;
		holdRepeatCastInSpellArea = false;
	}

	private bool IsHoldRepeatPointerPressed()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Invalid comparison between Unknown and I4
		if (holdRepeatCastPointerId == int.MinValue)
		{
			return false;
		}
		if (holdRepeatCastPointerId < 0)
		{
			return Input.GetMouseButton(0);
		}
		for (int i = 0; i < Input.touchCount; i++)
		{
			Touch touch = Input.GetTouch(i);
			if (((Touch)(ref touch)).fingerId == holdRepeatCastPointerId)
			{
				if ((int)((Touch)(ref touch)).phase != 3)
				{
					return (int)((Touch)(ref touch)).phase != 4;
				}
				return false;
			}
		}
		return false;
	}

	public void SelectSpellItem(Item item, bool isClicked)
	{
		CancelSpell();
		if (isClicked)
		{
			state = State.ClickSelected;
		}
		else if (item.spellNeedsTarget)
		{
			state = State.GettingTarget;
		}
		else
		{
			state = State.DragSelecting;
		}
		displaySpellInfo.spellItem = item;
		displaySpellInfo.AddCost(GameEvent.TriggerType.InBattleDisplay);
		if (isClicked)
		{
			gameState.UIManager.battleItemInfoPanel.Show(item, PivotManager.PivotType.PlayerTablePivot);
		}
		gameState.UIManager.playerItemArea.SetDirty();
		gameState.UIManager.enemyBftUIDisplay.tableItemArea.SetDirty();
	}

	public void SelectSpellTarget(Item item)
	{
		if (item == null)
		{
			state = State.GettingTarget;
		}
		else
		{
			state = State.TargetSelected;
		}
		displaySpellInfo.targetItem = item;
		gameState.UIManager.playerItemArea.SetDirty();
		gameState.UIManager.enemyBftUIDisplay.tableItemArea.SetDirty();
	}

	private void EndSpell(SpellCastInfo.SpellCastFailureReason reason = SpellCastInfo.SpellCastFailureReason.None)
	{
		switch (reason)
		{
		case SpellCastInfo.SpellCastFailureReason.NotEnoughCost:
			gameState.UIManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_without_enough_cost_unable_cast);
			break;
		case SpellCastInfo.SpellCastFailureReason.NotCoolDowned:
			gameState.UIManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_without_cooldown_unable_cast);
			break;
		case SpellCastInfo.SpellCastFailureReason.ConditionFailure:
			gameState.UIManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_without_condition_unable_cast);
			break;
		case SpellCastInfo.SpellCastFailureReason.AlreadyAtHead:
			gameState.UIManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_item_already_at_head);
			break;
		case SpellCastInfo.SpellCastFailureReason.ConsecutiveDazzlingAtHead:
			gameState.UIManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_consecutive_dazzling_at_head);
			break;
		case SpellCastInfo.SpellCastFailureReason.NoFocusSpell:
			gameState.UIManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_no_focus_spell);
			break;
		default:
			throw new ArgumentOutOfRangeException("reason", reason, null);
		case SpellCastInfo.SpellCastFailureReason.None:
		case SpellCastInfo.SpellCastFailureReason.NotControl:
			break;
		}
		CancelSpell();
	}

	public bool CheckItemAsTargetAble(Item item)
	{
		return displaySpellInfo.CheckItemAsTargetAble(item);
	}

	public void CancelSpell()
	{
		ResetHoldRepeatCastGesture();
		CancelSpellSelection();
	}

	private void CancelSpellSelection()
	{
		state = State.Prepare;
		displaySpellInfo.CancelSpell();
		gameState.UIManager.battleItemInfoPanel.Hide();
		gameState.UIManager.playerItemArea.SetDirty();
		gameState.UIManager.enemyBftUIDisplay.tableItemArea.SetDirty();
	}

	public void EnqueueSpellRequest(SpellRequest request)
	{
		requests.Enqueue(request);
		gameState.actionManager.EnqueueActionToEnd(new DequeueSpellRequestAction(this));
	}

	public void TryCastDisplaySpell()
	{
		state = State.Prepare;
		if (RealSpellCastInfo.castState != SpellCastInfo.State.Preparing)
		{
			EnqueueSpellRequest(new SpellRequest
			{
				spellItem = spellItem,
				targetItem = targetItem
			});
			return;
		}
		UIHelper.HideAllInfo(null);
		bool isInBattle = gameState.battleState.isInBattle;
		BattleStateInfo battleState = gameState.battleState;
		bool flag = battleState != null && battleState.turnPhase == TurnPhase.PlayerControlPhase;
		if (!SpellCastInfo.SpellCastAble(gameState, spellItem, targetItem, inBattleDisplay: true, out var failureReason) || !isInBattle || !flag)
		{
			EndSpell(failureReason);
		}
		else
		{
			gameState.StartCoroutine(CastDisplaySpell);
		}
	}

	private IEnumerator CastDisplaySpell()
	{
		isCasting = true;
		yield return RealSpellCastInfo.TriggerSpellCast(spellItem, targetItem, SpellCastInfo.CasterType.Player);
		gameState.UIManager.playerItemArea.OnItemSpell(spellItem);
		gameState.UIManager.playerItemArea.SetDirty();
		gameState.UIManager.enemyBftUIDisplay.tableItemArea.SetDirty();
		displaySpellInfo.CancelSpell();
		isCasting = false;
	}

	private void ComputeFrameItemInfo()
	{
		itemInfos.Clear();
		itemInfoByItem.Clear();
		List<Item> items = gameState.playerState.TableContainer.items;
		int num = 0;
		int num2 = -1;
		itemInfos.Resize(items.Count);
		for (int i = 0; i < items.Count; i++)
		{
			Item item = items[i];
			ItemInfo itemInfo = itemInfos.array[i];
			itemInfo.item = item;
			if (item != null)
			{
				itemInfoByItem[item] = itemInfo;
			}
			itemInfo.added = RealSpellCastInfo.CheckItemAdded(item);
			itemInfo.addedToItem = null;
			if (itemInfo.added)
			{
				itemInfo.addedToItem = RealSpellCastInfo.spellItem;
				itemInfo.index = -1;
			}
			else
			{
				itemInfo.index = num;
				num++;
				itemInfo.added = displaySpellInfo.CheckItemAdded(item);
				if (itemInfo.added)
				{
					itemInfo.addedToItem = displaySpellInfo.spellItem;
				}
			}
			itemInfo.shift = num2 != -1;
			if (item == displaySpellInfo.spellItem)
			{
				num2 = i;
			}
		}
		PlayerItemArea playerItemArea = this.playerItemArea;
		for (int j = 0; j < playerItemArea.itemDisplays.Count; j++)
		{
			Item item2 = playerItemArea.itemDisplays[j].item;
			if (item2 != null && itemInfoByItem.TryGetValue(item2, out var value))
			{
				playerItemArea.itemDisplays[j].stateModule.frameItemInfo = value;
			}
		}
	}

	private void ShowItemInfo()
	{
		if (isCasting || gameState.UIManager.battleItemSelectPanel.gameObject.activeSelf || gameState.UIManager.battleSkillItemSelectPanel.gameObject.activeSelf || gameState.UIManager.battleSkillTableItemSelectPanel.gameObject.activeSelf || gameState.UIManager.battleTableItemSelectPanel.gameObject.activeSelf || gameState.UIManager.deckItemSelectPanel.gameObject.activeSelf || gameState.UIManager.skillPickItemPanel.gameObject.activeSelf)
		{
			return;
		}
		if (state == State.GettingTarget || state == State.TargetSelected)
		{
			gameState.UIManager.battleItemInfoPanel.Hide();
			return;
		}
		IReadOnlyList<PlayerItemDisplay> activeElements = playerItemArea.itemSlotDisplayPool.GetActiveElements();
		for (int i = 0; i < activeElements.Count; i++)
		{
			PlayerItemDisplay playerItemDisplay = activeElements[i];
			if ((playerItemDisplay.stateModule.currentState == PlayerTableDisplayStateModule.State.Selected || playerItemDisplay.stateModule.currentState == PlayerTableDisplayStateModule.State.Over) && targetItem != playerItemDisplay.item)
			{
				gameState.UIManager.battleItemInfoPanel.Show(playerItemDisplay.item, PivotManager.PivotType.PlayerTablePivot);
				return;
			}
		}
		for (int j = 0; j < enemyItemArea.itemDisplays.Count; j++)
		{
			EnemyItemDisplay enemyItemDisplay = enemyItemArea.itemDisplays[j];
			if (enemyItemDisplay.stateModule.currentState == EnemyItemDisplayStateModule.State.Selected || enemyItemDisplay.stateModule.currentState == EnemyItemDisplayStateModule.State.Over)
			{
				gameState.UIManager.battleItemInfoPanel.Show(enemyItemDisplay.item, PivotManager.PivotType.PlayerTablePivot);
				return;
			}
		}
		CompanionItemArea companionItemArea = gameState.UIManager.playerBftUIDisplay.companionItemArea;
		for (int k = 0; k < companionItemArea.slotDisplays.Count; k++)
		{
			ItemDisplayBase itemDisplayBase = companionItemArea.slotDisplays[k];
			if (itemDisplayBase.GetModule<CompanionItemInfoModule>().selected || itemDisplayBase.GetModule<CompanionItemInfoModule>().over)
			{
				gameState.UIManager.battleItemInfoPanel.Show(itemDisplayBase.item, PivotManager.PivotType.PlayerTablePivot);
				return;
			}
		}
		companionItemArea = gameState.UIManager.enemyBftUIDisplay.companionItemArea;
		for (int l = 0; l < companionItemArea.slotDisplays.Count; l++)
		{
			ItemDisplayBase itemDisplayBase2 = companionItemArea.slotDisplays[l];
			if (itemDisplayBase2.GetModule<CompanionItemInfoModule>().selected || itemDisplayBase2.GetModule<CompanionItemInfoModule>().over)
			{
				gameState.UIManager.battleItemInfoPanel.Show(itemDisplayBase2.item, PivotManager.PivotType.PlayerTablePivot);
				return;
			}
		}
		gameState.UIManager.battleItemInfoPanel.Hide();
	}

	public static void EnqueueSpellSelectItemAsTargetAction(Item spellItem, Action<Item> targetSetter, GameStateInfo gameState)
	{
		EnqueueSpellSelectItemAsTargetAction(spellItem, spellItem.CurrentSpell, targetSetter, gameState);
	}

	public static void EnqueueSpellSelectItemAsTargetAction(Item spellItem, ItemSpell spell, Action<Item> targetSetter, GameStateInfo gameState, Func<Item, bool> additionalTargetCheck = null, Action onCancel = null)
	{
		if (spell == null)
		{
			return;
		}
		ItemRequirement requirement = spell.requirement;
		List<Item> acceptItems = requirement.GetTargetableItems(spellItem, gameState);
		List<Item> list = new List<Item>();
		switch (requirement.targetType)
		{
		case ItemRequirement.TargetType.OpponentTable:
			list.AddRange(gameState.battleState.GetOpponent(spellItem.OwnerBft).tableContainer.GetAllItems());
			break;
		case ItemRequirement.TargetType.OwnerTable:
			list.AddRange(spellItem.OwnerBft.tableContainer.GetAllItems());
			break;
		case ItemRequirement.TargetType.BothTable:
			list.AddRange(spellItem.OwnerBft.tableContainer.GetAllItems());
			list.AddRange(gameState.battleState.GetOpponent(spellItem.OwnerBft).tableContainer.GetAllItems());
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case ItemRequirement.TargetType.None:
			break;
		}
		string panelHint = string.Format(ScriptableObjectSingleton<TextDataPreset>.instance.term_select_arg0_as_spell_target, spellItem.ItemName);
		List<Item> selected = new List<Item>();
		SelectItemInfo info = new SelectItemInfo
		{
			pool = list,
			panelHintGetter = () => panelHint,
			canConfirm = CanConfirm,
			canSelect = CanSelect,
			onConfirm = OnConfirm,
			onCancel = onCancel,
			shouldCancel = ShouldCancel,
			singleSelect = true,
			output = selected
		};
		if (spell.requirement.targetType == ItemRequirement.TargetType.OwnerTable)
		{
			BattleFieldTransform ownerBft = spellItem.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer)
			{
				gameState.actionManager.EnqueueAction(new SelectItemAction(info, PlayerSelectItemsEffect.SelectPanelType.Table));
				return;
			}
		}
		gameState.actionManager.EnqueueAction(new SelectItemAction(info, PlayerSelectItemsEffect.SelectPanelType.HorizontalSelect));
		bool CanConfirm()
		{
			return selected.Count == 1;
		}
		bool CanSelect(Item item)
		{
			if (acceptItems.Contains(item))
			{
				if (additionalTargetCheck != null)
				{
					return additionalTargetCheck(item);
				}
				return true;
			}
			return false;
		}
		void OnConfirm()
		{
			targetSetter(selected.FirstOrDefault());
		}
		bool ShouldCancel()
		{
			return acceptItems.All((Item item) => additionalTargetCheck != null && !additionalTargetCheck(item));
		}
	}
}
