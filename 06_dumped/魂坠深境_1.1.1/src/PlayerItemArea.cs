using System;
using System.Collections;
using System.Collections.Generic;
using Common.UI;
using DamageNumbersPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItemArea : UIBase, IFrameUpdate
{
	public enum State
	{
		None,
		BeforePlayerControl,
		PlayerControl,
		Enemy
	}

	public class Lock
	{
		private int count;

		public bool isLocked => count > 0;

		public void Acquire()
		{
			count++;
		}

		public void Release()
		{
			count--;
		}
	}

	[SerializeField]
	private PlayerItemDisplay itemDisplayPrefab;

	[SerializeField]
	private Transform itemSlotDisplayParent;

	[SerializeField]
	public Image itemEffectImage;

	[SerializeField]
	public Image enchantEffectImage;

	[SerializeField]
	public PlayerOverloadDisplay playerOverloadDisplay;

	[NonSerialized]
	private bool animLocked;

	[NonSerialized]
	private ItemDisplayBase.PlayerAnimConfig config;

	[NonSerialized]
	public PlayerControlInfo controlInfo;

	[NonSerialized]
	public State currentState;

	[NonSerialized]
	public List<PlayerItemDisplay> itemDisplays;

	[NonSerialized]
	public CellUIPrefabPool<PlayerItemDisplay, Item> itemSlotDisplayPool;

	[NonSerialized]
	public PlayerItemDisplay selectedDisplay;

	[NonSerialized]
	public PlayerItemDisplay targetDisplay;

	private Lock itemDisplayUpdateBlocker;

	public bool blockItemDisplayUpdate => itemDisplayUpdateBlocker.isLocked;

	protected override void Init()
	{
		InitUI(playerOverloadDisplay);
		itemDisplays = new List<PlayerItemDisplay>();
		controlInfo = new PlayerControlInfo(UIBase.gameState);
		itemDisplayPrefab.gameObject.SetActive(value: false);
		itemSlotDisplayPool = GetPrefabPool<PlayerItemDisplay, Item>(itemDisplayPrefab, itemSlotDisplayParent);
		selectedDisplay = null;
		targetDisplay = null;
		config = UIBase.applicationState.vfxDataPreset.playerItemAnimConfig;
		itemDisplayUpdateBlocker = new Lock();
		animLocked = false;
		UIBase.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.TriggerTurnPhaseChange, _OnBattleStateMachineMove);
		UIBase.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BattleStart, _BattleStart);
		UIBase.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeBattleEnd, _BeforeBattleEnd);
		UIBase.gameState.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyClicked);
		UIBase.gameState.actionManager.actionFrameFinishEvent += ClearCastInfoCacheAndRefreshItemDisplays;
	}

	protected override void Dispose()
	{
		base.Dispose();
		UIBase.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.TriggerTurnPhaseChange, _OnBattleStateMachineMove);
		UIBase.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BattleStart, _BattleStart);
		UIBase.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BeforeBattleEnd, _BeforeBattleEnd);
		UIBase.gameState.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyClicked);
		UIBase.gameState.actionManager.actionFrameFinishEvent -= ClearCastInfoCacheAndRefreshItemDisplays;
	}

	private void ClearCastInfoCacheAndRefreshItemDisplays()
	{
		controlInfo.ClearCastInfoCache();
		if (!UIBase.gameState.battleState.isConsumingOverflowItems)
		{
			for (int i = 0; i < itemDisplays.Count; i++)
			{
				itemDisplays[i].SetDirty();
			}
			playerOverloadDisplay.SetDirty();
		}
	}

	protected override void UpdateDisplay()
	{
		RefreshSlotDisplays();
		NotifyShift();
	}

	public void OnDrawFromPile(Item item)
	{
		if (item != null && !(GetSlotDisplay(item) != null))
		{
			UIBase.gameState.StartCoroutine(() => DrawFromPile(item));
		}
		IEnumerator DrawFromPile(Item item2)
		{
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.DrawFromPile);
			PlayerItemDisplay display = SpawnItem(item2);
			display.gameObject.SetActive(value: false);
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return AcquireAnimLock(config.addItemInterval / UIBase.gameState.actionManager.GetBattleScale());
			}
			IEnumerator enumerator = display.stateModule.PlayTween_Draw();
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return enumerator;
			}
			else
			{
				display.stateModule.SkipMutualExcludeAnim();
			}
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.DrawFromPile);
			SetDirty();
		}
	}

	public IEnumerator PreSpawnItem(Item item, Item source, int insertIndex, Action<PlayerItemDisplay> onDisplaySpawned = null)
	{
		if (item != null && !(GetSlotDisplay(item) != null))
		{
			PlayerItemDisplay display = SpawnItem(item);
			onDisplaySpawned?.Invoke(display);
			display.gameObject.SetActive(value: false);
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return AcquireAnimLock(config.addItemInterval / UIBase.gameState.actionManager.GetBattleScale());
			}
			IEnumerator enumerator = display.stateModule.PlayTween_AddNew(source, insertIndex);
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return enumerator;
			}
			else
			{
				display.stateModule.SkipMutualExcludeAnim(insertIndex, UIBase.gameState.playerState.TableContainer.ItemsCount + 1);
			}
			SetDirty();
			display.enableUpdate = false;
		}
	}

	public void CancelPreSpawnItem(PlayerItemDisplay display)
	{
		if (!(display == null) && itemDisplays.Contains(display) && !UIBase.gameState.playerState.TableContainer.Contains(display.item))
		{
			display.enableUpdate = false;
			DespawnItem(display);
			SetDirty();
		}
	}

	public void OnSpawnItem(Item item)
	{
		if (item != null && !(GetSlotDisplay(item) == null))
		{
			GetSlotDisplay(item).enableUpdate = true;
			SetDirty();
		}
	}

	public void OnAddItem(Item item, Vector3? fromPosition)
	{
		if (item != null && !(GetSlotDisplay(item) != null))
		{
			UIBase.gameState.StartCoroutine(() => AddItem(item, fromPosition));
		}
		IEnumerator AddItem(Item item2, Vector3? fromPosition2)
		{
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.AddItem);
			PlayerItemDisplay playerItemDisplay = SpawnItem(item2);
			playerItemDisplay.gameObject.SetActive(value: false);
			IEnumerator enumerator = playerItemDisplay.stateModule.PlayTween_Add(fromPosition2);
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return enumerator;
			}
			else
			{
				playerItemDisplay.stateModule.SkipMutualExcludeAnim();
			}
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.AddItem);
			SetDirty();
		}
	}

	public void OnRemoveItem(Item item)
	{
		if (item != null && !(GetSlotDisplay(item) == null))
		{
			UIBase.gameState.StartCoroutine(() => RemoveItem(item));
		}
		IEnumerator RemoveItem(Item item2)
		{
			PlayerItemDisplay display = GetSlotDisplay(item2);
			itemDisplays.Remove(display);
			display.enableUpdate = false;
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.RemoveItem);
			itemDisplayUpdateBlocker.Acquire();
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return AcquireAnimLock(config.removeInterval / UIBase.gameState.actionManager.GetBattleScale());
			}
			IEnumerator enumerator = display.stateModule.PlayTween_Remove();
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return enumerator;
			}
			itemDisplayUpdateBlocker.Release();
			DespawnItem(display);
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.RemoveItem);
			if (!blockItemDisplayUpdate)
			{
				SetDirty();
			}
		}
	}

	public void OnItemTransferToEnemy(Item item)
	{
		if (item != null && !(GetSlotDisplay(item) == null))
		{
			UIBase.gameState.StartCoroutine(() => ItemTransferToEnemy(item));
		}
		IEnumerator ItemTransferToEnemy(Item item2)
		{
			PlayerItemDisplay display = GetSlotDisplay(item2);
			display.enableUpdate = false;
			UIBase.gameState.UIManager.enemyBftUIDisplay.tableItemArea.OnItemTransferFromPlayer(item2);
			IEnumerator enumerator = display.stateModule.PlayTween_TransferToEnemy();
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return enumerator;
			}
			else
			{
				UIBase.gameState.UIManager.enemyBftUIDisplay.tableItemArea.SkipTransferFromPlayerAnim(item2);
			}
			DespawnItem(display);
			SetDirty();
		}
	}

	public void OnItemTransferFromEnemy(Item item)
	{
		if (item != null && !(GetSlotDisplay(item) != null))
		{
			UIBase.gameState.StartCoroutine(() => ItemTransferFromEnemy(item));
		}
		IEnumerator ItemTransferFromEnemy(Item item2)
		{
			PlayerItemDisplay playerItemDisplay = SpawnItem(item2);
			playerItemDisplay.gameObject.SetActive(value: false);
			IEnumerator enumerator = playerItemDisplay.stateModule.PlayTween_TransferFromEnemy();
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return enumerator;
			}
			else
			{
				playerItemDisplay.stateModule.SkipMutualExcludeAnim();
			}
			UIBase.gameState.UIManager.enemyBftUIDisplay.tableItemArea.OnItemTransferToPlayer(item2);
			SetDirty();
		}
	}

	public void OnTransferOut(Item item)
	{
		PlayerItemDisplay slotDisplay = GetSlotDisplay(item);
		slotDisplay.enableUpdate = false;
		DespawnItem(slotDisplay);
		SetDirty();
	}

	public void OnItemExile(Item item)
	{
		if (item != null && !(GetSlotDisplay(item) == null))
		{
			UIBase.gameState.StartCoroutine(() => ItemExile(item));
		}
		IEnumerator ItemExile(Item item2)
		{
			PlayerItemDisplay display = GetSlotDisplay(item2);
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.RemoveItem);
			display.enableUpdate = false;
			IEnumerator enumerator = display.stateModule.PlayTween_Exile();
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return enumerator;
			}
			DespawnItem(display);
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.RemoveItem);
			SetDirty();
		}
	}

	public void OnItemSpell(Item item)
	{
		if (item != null && !(GetSlotDisplay(item) == null))
		{
			UIBase.gameState.StartCoroutine(() => ItemSpell(item));
		}
		IEnumerator ItemSpell(Item item2)
		{
			IEnumerator enumerator = GetSlotDisplay(item2).stateModule.PlayTween_Spell();
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return enumerator;
			}
			SetDirty();
		}
	}

	public void OnItemMove(Item item, Action onAnimEnd)
	{
		if (item != null && !(GetSlotDisplay(item) == null))
		{
			UIBase.gameState.StartCoroutine(() => ItemMove(item));
		}
		IEnumerator ItemMove(Item item2)
		{
			PlayerItemDisplay slotDisplay = GetSlotDisplay(item2);
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.MoveItem);
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_card_exchange);
			IEnumerator enumerator = slotDisplay.stateModule.PlayTween_Move();
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return enumerator;
			}
			else
			{
				slotDisplay.stateModule.SkipMutualExcludeAnim();
			}
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.MoveItem);
			onAnimEnd?.Invoke();
			SetDirty();
		}
	}

	public void OnItemSwap(Item itemA, Item itemB, Action onAnimEnd)
	{
		PlayerItemDisplay displayA = GetSlotDisplay(itemA);
		PlayerItemDisplay displayB = GetSlotDisplay(itemB);
		if (!(displayA == null) && !(displayB == null))
		{
			UIBase.gameState.StartCoroutine(() => ItemSwap(displayA, displayB));
		}
		IEnumerator ItemSwap(PlayerItemDisplay playerItemDisplay, PlayerItemDisplay playerItemDisplay2)
		{
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.SwapItem);
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return WrapperEnumerator.WaitAll(new List<WrapperEnumerator>
				{
					UIBase.gameState.StartCoroutine(playerItemDisplay.stateModule.OnSwap),
					UIBase.gameState.StartCoroutine(playerItemDisplay2.stateModule.OnSwap)
				});
			}
			else
			{
				playerItemDisplay.stateModule.SkipMutualExcludeAnim();
				playerItemDisplay2.stateModule.SkipMutualExcludeAnim();
			}
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_card_exchange);
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.SwapItem);
			onAnimEnd?.Invoke();
			SetDirty();
		}
	}

	public void OnShuffle(Action onAnimEnd)
	{
		UIBase.gameState.StartCoroutine(Shuffle);
		IEnumerator Shuffle()
		{
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.ShuffleTable);
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				List<WrapperEnumerator> list = new List<WrapperEnumerator>();
				foreach (PlayerItemDisplay itemDisplay in itemDisplays)
				{
					list.Add(UIBase.gameState.StartCoroutine(itemDisplay.stateModule.OnShuffle));
				}
				yield return WrapperEnumerator.WaitAll(list);
			}
			else
			{
				foreach (PlayerItemDisplay itemDisplay2 in itemDisplays)
				{
					itemDisplay2.stateModule.SkipMutualExcludeAnim();
				}
			}
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.ShuffleTable);
			onAnimEnd?.Invoke();
			SetDirty();
		}
	}

	public void OnConvertItem(Item item)
	{
		if (item != null && !(GetSlotDisplay(item) == null))
		{
			UIBase.gameState.StartCoroutine(() => ConvertItem(item));
		}
		IEnumerator ConvertItem(Item item2)
		{
			PlayerItemDisplay slotDisplay = GetSlotDisplay(item2);
			slotDisplay.Unset();
			slotDisplay.Setup(item2);
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.Default);
			IEnumerator enumerator = slotDisplay.stateModule.OnConvert();
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return enumerator;
			}
			else
			{
				slotDisplay.stateModule.SkipMutualExcludeAnim();
			}
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.Default);
		}
	}

	public void OnTriggerAbilityAnim(Item item, Ability.AbilityAnimPriority priority, Ability.AbilityAnimType abilityAnimType)
	{
		PlayerItemDisplay display = GetSlotDisplay(item);
		if (!(display == null))
		{
			UIBase.gameState.StartCoroutine(() => TriggerAbilityAnim(display, priority, abilityAnimType));
		}
		static IEnumerator TriggerAbilityAnim(PlayerItemDisplay playerItemDisplay, Ability.AbilityAnimPriority abilityAnimPriority, Ability.AbilityAnimType abilityAnimType2)
		{
			PlayerTableDisplayStateModule stateModule = playerItemDisplay.stateModule;
			if (abilityAnimPriority >= Ability.AbilityAnimPriority.Weak)
			{
				UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.Spell);
			}
			IEnumerator enumerator = stateModule.TriggerAbilityAnim(abilityAnimPriority, abilityAnimType2, UIBase.gameState.actionManager.GetBattleScale());
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return enumerator;
			}
			if (abilityAnimPriority >= Ability.AbilityAnimPriority.Weak)
			{
				UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.Spell);
			}
		}
	}

	public void OnTriggerAbilityEndAnim(Item item)
	{
		PlayerItemDisplay display = GetSlotDisplay(item);
		if (!(display == null))
		{
			UIBase.gameState.StartCoroutine(() => TriggerAbilityEndAnim(display));
		}
		static IEnumerator TriggerAbilityEndAnim(PlayerItemDisplay playerItemDisplay)
		{
			playerItemDisplay.stateModule.TriggerAbilityEndAnim();
			yield break;
		}
	}

	public void TriggerStanceItemAnim(Item item, Ability.AbilityAnimPriority priority, GameEvent eve, Stance stance = null)
	{
		PlayerItemDisplay slotDisplay = GetSlotDisplay(item);
		if (!(slotDisplay == null))
		{
			_ = slotDisplay.stateModule;
			ShowPopTextAnim();
		}
		void ShowPopTextAnim()
		{
			if (!item.IsHidden(UIBase.gameState) && Enum.TryParse<Stance.ChangeType>(eve.stringValue, out var result))
			{
				string stanceChangeText = TextDisplayHelper.GetStanceChangeText(result);
				if (!stanceChangeText.IsNullOrEmpty())
				{
					DamageNumberGUI enchantPopupTextPrefab = ScriptableObjectSingleton<VFXDataPreset>.instance.enchantPopupTextPrefab;
					if (stance != null && stance.def != null)
					{
						enchantPopupTextPrefab.SetColor(stance.def.stanceColor);
					}
					PopupTextAction action = new PopupTextAction(item, stanceChangeText, enchantPopupTextPrefab);
					UIBase.gameState.actionManager.EnqueueAction(action);
				}
			}
		}
	}

	public void TriggerEnchantAbilityAnim(Item item, Ability.AbilityAnimPriority priority, Enchant enchant)
	{
		PlayerItemDisplay slotDisplay = GetSlotDisplay(item);
		if (!(slotDisplay == null) && !UIBase.gameState.actionManager.skipFrameAnim)
		{
			PlayerTableDisplayStateModule module = slotDisplay.stateModule;
			UIBase.gameState.StartCoroutine(() => module.TriggerEnchantAbilityAnim(priority, enchant));
		}
	}

	public void TriggerRuneAbilityAnim(Item item, Ability.AbilityAnimPriority priority, Rune rune)
	{
		PlayerItemDisplay slotDisplay = GetSlotDisplay(item);
		if (!(slotDisplay == null) && !UIBase.gameState.actionManager.skipFrameAnim)
		{
			PlayerTableDisplayStateModule module = slotDisplay.stateModule;
			UIBase.gameState.StartCoroutine(() => module.TriggerRuneAbilityAnim(priority, rune));
		}
	}

	public void OnHintOverload()
	{
		UIBase.gameState.StartCoroutine(HintOverload);
		IEnumerator HintOverload()
		{
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.HintOverload);
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				List<WrapperEnumerator> list = new List<WrapperEnumerator>();
				foreach (PlayerItemDisplay itemDisplay in itemDisplays)
				{
					list.Add(UIBase.gameState.StartCoroutine(itemDisplay.stateModule.PlayTween_HintOverload));
				}
				yield return WrapperEnumerator.WaitAll(list);
			}
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.HintOverload);
			currentState = State.PlayerControl;
		}
	}

	public PlayerItemDisplay GetSlotDisplay(Item item)
	{
		return itemDisplays.Find((PlayerItemDisplay display) => display.item == item);
	}

	public bool IsItemAnimationComplete(Item item)
	{
		PlayerItemDisplay slotDisplay = GetSlotDisplay(item);
		if (slotDisplay == null)
		{
			return !UIBase.gameState.actionManager.IsActionLocked(ActionLock.Type.RemoveItem);
		}
		return slotDisplay.stateModule.IsAtRest;
	}

	public PlayerItemDisplay GetSelectedDisplay()
	{
		return selectedDisplay;
	}

	public void OnBattleEnd()
	{
		foreach (ItemDisplayBase item in new List<ItemDisplayBase>(itemDisplays))
		{
			OnItemExile(item.item);
		}
		controlInfo.CancelSpell();
	}

	private PlayerItemDisplay SpawnItem(Item item)
	{
		PlayerItemDisplay playerItemDisplay = itemSlotDisplayPool.Spawn(item);
		itemDisplays.Add(playerItemDisplay);
		return playerItemDisplay;
	}

	private void DespawnItem(PlayerItemDisplay display)
	{
		itemDisplays.Remove(display);
		itemSlotDisplayPool.Despawn(display);
	}

	private void NotifyShift()
	{
		if (UIBase.gameState.battleState.isConsumingOverflowItems)
		{
			return;
		}
		foreach (PlayerItemDisplay itemDisplay in itemDisplays)
		{
			itemDisplay.stateModule.OnShift();
		}
	}

	private void RefreshSlotDisplays()
	{
		List<Item> allItems = UIBase.gameState.playerState.TableContainer.GetAllItems();
		DisplaySpellCastInfo displaySpellInfo = UIBase.gameState.UIManager.playerItemArea.controlInfo.displaySpellInfo;
		foreach (Item item in allItems)
		{
			if (selectedDisplay?.item != item && !displaySpellInfo.addedCosts.Contains(item))
			{
				GetSlotDisplay(item)?.transform.SetSiblingIndex(GetItemIndex(item));
			}
		}
		if (selectedDisplay != null)
		{
			selectedDisplay.transform.SetSiblingIndex(itemDisplays.Count - displaySpellInfo.addedCosts.Count - 1);
		}
		for (int i = 0; i < displaySpellInfo.addedCosts.Count; i++)
		{
			GetSlotDisplay(displaySpellInfo.addedCosts[i])?.transform.SetSiblingIndex(itemDisplays.Count - i - 1);
		}
	}

	private int GetItemIndex(Item item)
	{
		List<Item> allItems = UIBase.gameState.playerState.TableContainer.GetAllItems();
		List<SpellCastInfo.AddedCostInfo> addedCostInfos = UIBase.gameState.UIManager.playerItemArea.controlInfo.displaySpellInfo.addedCostInfos;
		int num = 0;
		for (int i = 0; i < allItems.Count; i++)
		{
			if (addedCostInfos.Find((SpellCastInfo.AddedCostInfo info) => info.item == item) == null)
			{
				if (allItems[i] == item)
				{
					return num;
				}
				num++;
			}
		}
		return -1;
	}

	private IEnumerator AcquireAnimLock(float time)
	{
		while (animLocked)
		{
			yield return null;
		}
		UIBase.gameState.StartCoroutine(() => Lock(time));
		IEnumerator Lock(float t)
		{
			animLocked = true;
			yield return new WaitForSeconds(t);
			animLocked = false;
		}
	}

	private void _OnBattleStateMachineMove(GameEvent.Type type, GameEvent eve)
	{
		switch (UIBase.gameState.battleState.turnPhase)
		{
		case TurnPhase.Prepare:
			currentState = State.None;
			break;
		case TurnPhase.PlayerControlPhase:
			currentState = State.PlayerControl;
			break;
		case TurnPhase.PlayerTurnStartPhase:
			currentState = State.BeforePlayerControl;
			break;
		case TurnPhase.PlayerTurnEndPhase:
			currentState = State.Enemy;
			UIBase.gameState.UIManager.playerItemArea.controlInfo.CancelSpell();
			UIBase.gameState.UIManager.battleItemInfoPanel.Hide();
			break;
		case TurnPhase.EnemyMovePhase:
			currentState = State.Enemy;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		SetDirty();
	}

	private void _OnEmptyClicked()
	{
		controlInfo.selected = null;
		controlInfo.CancelSpell();
	}

	private void _BeforeBattleEnd(GameEvent.Type type, GameEvent eve)
	{
		OnBattleEnd();
	}

	private void _BattleStart(GameEvent.Type type, GameEvent eve)
	{
		itemDisplays.Clear();
		itemSlotDisplayPool.DespawnAll();
	}

	void IFrameUpdate.FrameUpdate()
	{
		controlInfo.UpdatePlayerControlInfo();
	}
}
