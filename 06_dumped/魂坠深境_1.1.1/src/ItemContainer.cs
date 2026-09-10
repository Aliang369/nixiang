using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemContainer : IBftOwnable
{
	public enum ContainerType
	{
		Deck,
		DrawPile,
		Table,
		WastePile,
		Temp,
		Display,
		Influcence,
		Companion,
		Skill,
		ExilePile
	}

	public class ItemSequenceState
	{
		public Dictionary<Item, int?> itemIndexs;

		public int persistentFixedSlotCount;

		public int tempFixedSlotCount;

		public object sequenceChangeSource;

		public IEnumerable<Item> AllItems => itemIndexs.Keys;

		public Item Tail
		{
			get
			{
				if (itemIndexs.Count != 0)
				{
					return itemIndexs.MaxBy((KeyValuePair<Item, int?> pair) => pair.Value ?? int.MinValue).Key;
				}
				return null;
			}
		}

		public Item Head
		{
			get
			{
				if (itemIndexs.Count != 0)
				{
					return itemIndexs.MinBy((KeyValuePair<Item, int?> pair) => pair.Value ?? int.MaxValue).Key;
				}
				return null;
			}
		}

		public bool IsNullIndexItem(Item item)
		{
			return !itemIndexs[item].HasValue;
		}

		public int GetIndex(Item item)
		{
			if (itemIndexs.ContainsKey(item))
			{
				return itemIndexs[item] ?? (-1);
			}
			return -1;
		}

		public Item GetItem(int index)
		{
			foreach (KeyValuePair<Item, int?> itemIndex in itemIndexs)
			{
				if (itemIndex.Value == index)
				{
					return itemIndex.Key;
				}
			}
			return null;
		}

		public ItemSequenceState(ItemContainer container)
		{
			itemIndexs = new Dictionary<Item, int?>();
			foreach (Item item in container.items)
			{
				itemIndexs.Add(item, item.ownerSlotIndex);
			}
			persistentFixedSlotCount = container.persistentFixedSlotCount;
			tempFixedSlotCount = container.tempFixedSlotCount;
		}
	}

	public const int TableItemHardLimit = 30;

	[SaveField(null, null)]
	public GameStateInfo gameState;

	[SaveField(null, null)]
	public List<Item> items;

	public Action<ItemSequenceState, ItemSequenceState> itemSequenceChangeEvent;

	[SaveField(null, null)]
	public BattleFieldTransform owner;

	[SaveField(null, null)]
	public int persistentFixedSlotCount;

	[SaveField(null, null)]
	public int tempFixedSlotCount;

	[SaveField(null, null)]
	public ContainerType type;

	public List<Item> fixedSlots => items.GetRange(0, GetFinalFixedSlotCount());

	public bool hasValidDefence => items.Find((Item item) => item.DamageAble && item.DefenceLike) != null;

	public int ItemsCount => items.Count;

	public Item lastItem => items.LastOrDefault();

	public Item firstItem => items.FirstOrDefault();

	public BattleFieldTransform OwnerBft => owner;

	public ItemContainer(BattleFieldTransform owner, ContainerType type)
	{
		this.owner = owner;
		this.type = type;
		gameState = BootSetter<GameStateInfo>.GetCurrentBootTarget();
		items = new List<Item>();
		itemSequenceChangeEvent = (Action<ItemSequenceState, ItemSequenceState>)Delegate.Combine(itemSequenceChangeEvent, new Action<ItemSequenceState, ItemSequenceState>(_OnItemSequenceChange));
	}

	private ItemContainer()
	{
		itemSequenceChangeEvent = (Action<ItemSequenceState, ItemSequenceState>)Delegate.Combine(itemSequenceChangeEvent, new Action<ItemSequenceState, ItemSequenceState>(_OnItemSequenceChange));
	}

	public bool CanAcceptItemWithinHardLimit(Item item = null)
	{
		BattleFieldTransform battleFieldTransform = owner;
		int num = 30 + ((battleFieldTransform != null && battleFieldTransform.isPlayer && type == ContainerType.Table) ? ((int)owner.handLimitBonus) : 0);
		if (type == ContainerType.Table && !Contains(item))
		{
			return ItemsCount < num;
		}
		return true;
	}

	public void SetSlotCount(int count, bool temp = false)
	{
		ItemSequenceState arg = new ItemSequenceState(this);
		if (temp)
		{
			tempFixedSlotCount = Mathf.Max(count, 0);
		}
		else
		{
			persistentFixedSlotCount = Mathf.Max(count, 0);
		}
		ItemSequenceState arg2 = new ItemSequenceState(this);
		itemSequenceChangeEvent?.Invoke(arg, arg2);
	}

	public bool AddItem(AddItemEventInfo addInfo, int insertIndex = -1)
	{
		Item addItem = addInfo.addItem;
		if (addItem == null)
		{
			return false;
		}
		if (items.Contains(addItem))
		{
			TraceLog.Warn($"[ItemContainer] Item already added to container - {addItem}");
			return false;
		}
		if (!CanAcceptItemWithinHardLimit(addItem))
		{
			BattleFieldTransform battleFieldTransform = owner;
			TraceLog.Warn($"[ItemContainer] Table item hard limit reached - {30 + ((battleFieldTransform != null && battleFieldTransform.isPlayer) ? ((int)owner.handLimitBonus) : 0)}");
			return false;
		}
		if (type == ContainerType.Table)
		{
			insertIndex = EternalItemRules.ResolveTableInsertIndex(this, addItem, insertIndex);
		}
		ItemSequenceState arg = new ItemSequenceState(this);
		if (insertIndex < 0 || insertIndex > items.Count)
		{
			items.Add(addItem);
		}
		else
		{
			items.Insert(insertIndex, addItem);
		}
		addItem.owner = this;
		gameState.gameEventManager.TriggerAddItem(addInfo);
		ItemSequenceState arg2 = new ItemSequenceState(this);
		itemSequenceChangeEvent?.Invoke(arg, arg2);
		return true;
	}

	public void AddItemSilently(Item item, int insertIndex = -1)
	{
		if (item == null)
		{
			return;
		}
		if (items.Contains(item))
		{
			Debug.LogError($"[ItemContainer] Item already silently added to container - {item}");
			return;
		}
		if (type == ContainerType.Table)
		{
			insertIndex = EternalItemRules.ResolveTableInsertIndex(this, item, insertIndex);
		}
		if (insertIndex < 0 || insertIndex > items.Count)
		{
			items.Add(item);
		}
		else
		{
			items.Insert(insertIndex, item);
		}
		item.owner = this;
	}

	public void ClearItemsSilently()
	{
		if (items == null)
		{
			items = new List<Item>();
			return;
		}
		foreach (Item item in items)
		{
			if (item?.owner == this)
			{
				item.owner = null;
			}
		}
		items.Clear();
	}

	public void RemoveItem(Item item, object source, bool clean)
	{
		if (item == null)
		{
			Debug.LogError($"[ItemContainer] RemoveItem() - Item is null - {source}");
		}
		else
		{
			if (!items.Contains(item) || (!clean && item.BlocksExternalAction(source)))
			{
				return;
			}
			ItemSequenceState arg = new ItemSequenceState(this);
			bool flag = gameState.playerState.playerSpellInfo.CheckItemAdded(item);
			item.owner = null;
			items.Remove(item);
			if (!clean)
			{
				gameState.gameEventManager.TriggerAfterRemoveItem(item, source, this);
				if (!flag)
				{
					ItemSequenceState arg2 = new ItemSequenceState(this);
					itemSequenceChangeEvent?.Invoke(arg, arg2);
				}
			}
		}
	}

	public void MoveItem(Item item, int deltaPos, object source = null)
	{
		int rawIndex = GetRawIndex(item);
		ItemSequenceState before;
		if (item == null)
		{
			Debug.LogError($"[ItemContainer] MoveItem() - item is null- {source}");
		}
		else if (!items.Contains(item))
		{
			Debug.LogError($"MoveItem() - item not in container - {item} - {source}");
		}
		else
		{
			if (item.BlocksExternalMove(source, gameState))
			{
				return;
			}
			before = new ItemSequenceState(this);
			int value = rawIndex + deltaPos;
			value = Mathf.Clamp(value, 0, items.Count - 1);
			value = EternalItemRules.ClampNonEternalTargetIndex(item, value);
			int num = rawIndex;
			int num2 = value - num;
			if (num2 != 0)
			{
				items.RemoveAt(num);
				items.Insert(value, item);
				gameState.gameEventManager.TriggerSlotMove(source, item, value, num2);
				if (item.OwnerBft.isPlayer && item.owner != null && item.owner.type == ContainerType.Table)
				{
					gameState.UIManager.playerItemArea.OnItemMove(item, InvokeSequenceChange);
				}
				else
				{
					InvokeSequenceChange();
				}
			}
		}
		void InvokeSequenceChange()
		{
			ItemSequenceState arg = new ItemSequenceState(this)
			{
				sequenceChangeSource = source
			};
			itemSequenceChangeEvent?.Invoke(before, arg);
		}
	}

	public void SwapItem(Item itemA, Item itemB, Item source)
	{
		int indexA = GetRawIndex(itemA);
		int indexB = GetRawIndex(itemB);
		ItemSequenceState before;
		if (!items.Contains(itemA))
		{
			Debug.LogError("[ItemContainer] SwapItem() - itemA not in container");
		}
		else if (!items.Contains(itemB))
		{
			Debug.LogError("[ItemContainer] SwapItem() - itemB not in container");
		}
		else
		{
			if (itemA.BlocksExternalMove(source, gameState) || itemB.BlocksExternalMove(source, gameState))
			{
				return;
			}
			before = new ItemSequenceState(this);
			items[indexA] = itemB;
			items[indexB] = itemA;
			BattleFieldTransform ownerBft = itemA.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer)
			{
				BattleFieldTransform ownerBft2 = itemB.OwnerBft;
				if (ownerBft2 != null && ownerBft2.isPlayer && itemA.owner.type == ContainerType.Table && itemB.owner.type == ContainerType.Table)
				{
					gameState.UIManager.playerItemArea.OnItemSwap(itemA, itemB, InvokeSequenceChange);
					return;
				}
			}
			InvokeSequenceChange();
		}
		void InvokeSequenceChange()
		{
			ItemSequenceState arg = new ItemSequenceState(this)
			{
				sequenceChangeSource = source
			};
			itemSequenceChangeEvent?.Invoke(before, arg);
			gameState.gameEventManager.TriggerAfterSwapItem(itemA, itemB, source, this, indexA, indexB);
		}
	}

	public void ShuffleSlots()
	{
		ItemSequenceState before = new ItemSequenceState(this);
		List<(Item, int)> list = new List<(Item, int)>();
		List<Item> list2 = new List<Item>();
		for (int i = 0; i < items.Count; i++)
		{
			if (gameState.gameEventManager.TriggerShuffleResolve(items[i], GameEvent.TriggerType.Real).reject)
			{
				list.Add((items[i], i));
			}
			else
			{
				list2.Add(items[i]);
			}
		}
		for (int j = 0; j < list2.Count - 1; j++)
		{
			int indexB = gameState.randomManager.Next(j + 1, list2.Count);
			list2.Swap(j, indexB);
		}
		for (int k = 0; k < list.Count; k++)
		{
			var (item, index) = list[k];
			list2.Insert(index, item);
		}
		items.Clear();
		items.AddRange(list2);
		BattleFieldTransform ownerBft = OwnerBft;
		if (ownerBft != null && ownerBft.isPlayer)
		{
			gameState.UIManager.playerItemArea.OnShuffle(InvokeSequenceChange);
			return;
		}
		BattleFieldTransform ownerBft2 = OwnerBft;
		if (ownerBft2 != null && ownerBft2.isEnemy)
		{
			gameState.UIManager.enemyBftUIDisplay.tableItemArea.OnShuffle(InvokeSequenceChange);
		}
		else
		{
			InvokeSequenceChange();
		}
		void InvokeSequenceChange()
		{
			ItemSequenceState arg = new ItemSequenceState(this);
			itemSequenceChangeEvent?.Invoke(before, arg);
		}
	}

	public void ReverseSlots()
	{
		ItemSequenceState arg = new ItemSequenceState(this);
		List<(Item, int)> list = new List<(Item, int)>();
		List<Item> list2 = new List<Item>();
		for (int i = 0; i < items.Count; i++)
		{
			if (gameState.gameEventManager.TriggerShuffleResolve(items[i], GameEvent.TriggerType.Real).reject)
			{
				list.Add((items[i], i));
			}
			else
			{
				list2.Add(items[i]);
			}
		}
		list2.Reverse();
		for (int j = 0; j < list.Count; j++)
		{
			var (item, index) = list[j];
			list2.Insert(index, item);
		}
		items.Clear();
		items.AddRange(list2);
		ItemSequenceState arg2 = new ItemSequenceState(this);
		itemSequenceChangeEvent?.Invoke(arg, arg2);
	}

	public List<Item> GetAllItems(bool orderReverse = false)
	{
		return GetAllItemsEnumerator(orderReverse).ToList();
	}

	public IEnumerable<Item> GetAllItemsEnumerator(bool orderReverse = false)
	{
		if (orderReverse)
		{
			for (int i = items.Count - 1; i >= 0; i--)
			{
				yield return items[i];
			}
		}
		else
		{
			for (int i = 0; i < items.Count; i++)
			{
				yield return items[i];
			}
		}
	}

	public int? GetItemIndex(Item item)
	{
		ItemContainer itemContainer = item.owner;
		if (itemContainer != null && itemContainer.type == ContainerType.Table)
		{
			BattleFieldTransform ownerBft = item.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer)
			{
				if (gameState.playerState.playerSpellInfo.CheckItemAdded(item))
				{
					return null;
				}
				int num = 0;
				for (int i = 0; i < items.Count; i++)
				{
					if (items[i] == item)
					{
						return num;
					}
					if (!gameState.playerState.playerSpellInfo.CheckItemAdded(items[i]))
					{
						num++;
					}
				}
				return num;
			}
		}
		int rawIndex = GetRawIndex(item);
		if (rawIndex == -1)
		{
			return null;
		}
		return rawIndex;
	}

	public int GetRawIndex(Item item)
	{
		return items.IndexOf(item);
	}

	public int GetItemPreviewIndex(Item item)
	{
		ItemContainer itemContainer = item.owner;
		if (itemContainer != null && itemContainer.type == ContainerType.Table)
		{
			BattleFieldTransform ownerBft = item.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer)
			{
				if (gameState.UIManager.playerItemArea.controlInfo.displaySpellInfo.CheckItemAdded(item))
				{
					return -1;
				}
				if (gameState.playerState.playerSpellInfo.CheckItemAdded(item))
				{
					return -1;
				}
				int num = 0;
				for (int i = 0; i < items.Count; i++)
				{
					if (items[i] == item)
					{
						return num;
					}
					if (!gameState.UIManager.playerItemArea.controlInfo.displaySpellInfo.CheckItemAdded(items[i]) && !gameState.playerState.playerSpellInfo.CheckItemAdded(items[i]))
					{
						num++;
					}
				}
				return num;
			}
		}
		return items.IndexOf(item);
	}

	public int GetFinalFixedSlotCount()
	{
		int num = persistentFixedSlotCount + tempFixedSlotCount;
		if (gameState?.gameEventManager != null && owner != null && owner.isPlayer)
		{
			GameEvent.ResolveInfo resolveInfo = gameState.gameEventManager.TriggerFixedSlotCountResolveInfo(GameEvent.TriggerType.InBattleDisplay);
			num += resolveInfo.deltaValue;
		}
		return num;
	}

	public bool Contains(Item item)
	{
		return items.Contains(item);
	}

	public bool ShouldOverflow(Item item)
	{
		if (item.owner != this)
		{
			return false;
		}
		if (item.IsAddAsSpellCost(gameState))
		{
			return false;
		}
		int finalFixedSlotCount = GetFinalFixedSlotCount();
		int num = 0;
		for (int i = 0; i < items.Count; i++)
		{
			if (!items[i].IsAddAsSpellCost(gameState))
			{
				if (item == items[i])
				{
					return num >= finalFixedSlotCount;
				}
				num++;
			}
		}
		throw new Exception($"ShouldOverflow({item}) -- Should not be here.");
	}

	public bool IsBattleContainer()
	{
		if (type != ContainerType.Table && type != ContainerType.DrawPile && type != ContainerType.WastePile)
		{
			return type == ContainerType.Companion;
		}
		return true;
	}

	private void _OnItemSequenceChange(ItemSequenceState before, ItemSequenceState after)
	{
		if (type == ContainerType.ExilePile)
		{
			return;
		}
		foreach (Item allItem in after.AllItems)
		{
			if (!after.IsNullIndexItem(allItem))
			{
				int index = before.GetIndex(allItem);
				int index2 = after.GetIndex(allItem);
				if (index != -1 && index != index2)
				{
					int deltaIndex = index2 - index;
					gameState.gameEventManager.TriggerSlotIndexChange(after.sequenceChangeSource, allItem, index, deltaIndex);
				}
			}
		}
		foreach (Item allItem2 in after.AllItems)
		{
			int index3 = before.GetIndex(allItem2);
			int index4 = after.GetIndex(allItem2);
			Item item = after.GetItem(index4 - 1);
			Item item2 = after.GetItem(index4 + 1);
			if (index3 == -1)
			{
				if (item != null)
				{
					gameState.gameEventManager.TriggerSideItemChange(allItem2, item);
				}
				if (item2 != null)
				{
					gameState.gameEventManager.TriggerSideItemChange(allItem2, item2);
				}
				continue;
			}
			Item item3 = before.GetItem(index3 - 1);
			Item item4 = before.GetItem(index3 + 1);
			if (item != item3)
			{
				gameState.gameEventManager.TriggerSideItemChange(allItem2, item);
			}
			if (item2 != item4)
			{
				gameState.gameEventManager.TriggerSideItemChange(allItem2, item2);
			}
		}
		Item head = after.Head;
		bool flag = head != null && before.AllItems.Contains(head);
		Item tail = after.Tail;
		bool flag2 = tail != null && before.AllItems.Contains(tail);
		Item head2 = before.Head;
		Item tail2 = before.Tail;
		if ((head != null && head != head2) & flag)
		{
			gameState.gameEventManager.TriggerMoveItemToHead(head, head2);
		}
		if ((tail != null && tail != tail2) & flag2)
		{
			gameState.gameEventManager.TriggerMoveItemToTail(tail, tail2);
		}
	}
}
