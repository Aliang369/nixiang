using System;
using System.Collections.Generic;
using Common.UI;
using UnityEngine;

public class BattleTableItemSelectPanel : ItemSelectPanel
{
	private const int InvalidPointerId = int.MinValue;

	[SerializeField]
	protected ItemDisplayBase playerItemDisplayPrefab;

	[SerializeField]
	protected RectTransform playerItemDisplayParent;

	[SerializeField]
	protected ItemDisplayBase enemyItemDisplayPrefab;

	[SerializeField]
	protected RectTransform enemyItemDisplayParent;

	[NonSerialized]
	public CellUIPrefabPool<ItemDisplayBase, Item> enemyItemDisplayPool;

	[NonSerialized]
	public CellUIPrefabPool<ItemDisplayBase, Item> playerItemDisplayPool;

	[NonSerialized]
	private readonly HashSet<Item> slideVisitedItems = new HashSet<Item>();

	[NonSerialized]
	private int slidePointerId = int.MinValue;

	[NonSerialized]
	private Item slideStartItem;

	[NonSerialized]
	private bool slideSelecting;

	[NonSerialized]
	private bool slideShouldSelect;

	protected override void Init()
	{
		base.Init();
		UIBase.gameState.actionManager.actionFrameFinishEvent += SetDirty;
	}

	protected override void Dispose()
	{
		base.Dispose();
		UIBase.gameState.actionManager.actionFrameFinishEvent -= SetDirty;
	}

	protected override void InitPools()
	{
		playerItemDisplayPool = GetPrefabPool<ItemDisplayBase, Item>(playerItemDisplayPrefab, playerItemDisplayParent);
		enemyItemDisplayPool = GetPrefabPool<ItemDisplayBase, Item>(enemyItemDisplayPrefab, enemyItemDisplayParent);
	}

	public override void Setup(SelectItemInfo info)
	{
		ResetSlideSelection();
		base.info = info;
		base.gameObject.SetActive(value: true);
		ToHint();
		textLinkObjectDisplayer.SetGenerator(info.generator);
		playerItemDisplayPool.DespawnAll();
		enemyItemDisplayPool.DespawnAll();
		int num = 0;
		foreach (Item item in info.pool.OrEmptyIfNull())
		{
			SpawnDisplay(item).transform.SetSiblingIndex(num++);
		}
		if (CanConfirm())
		{
			ToConfirm();
		}
		SetDirty();
	}

	public override void Unset()
	{
		ResetSlideSelection();
		info = null;
		playerItemDisplayPool.DespawnAll();
		enemyItemDisplayPool.DespawnAll();
		ToHide();
	}

	public void BeginSlideSelection(Item item, int pointerId)
	{
		ResetSlideSelection();
		if (info != null && !info.singleSelect && item != null)
		{
			bool flag = Selected(item);
			if (flag || CanSelect(item))
			{
				slidePointerId = pointerId;
				slideStartItem = item;
				slideShouldSelect = !flag;
			}
		}
	}

	public void ContinueSlideSelection(Item item, int pointerId, bool dragging)
	{
		if (dragging && pointerId == slidePointerId && item != null && item != slideStartItem)
		{
			if (!slideSelecting)
			{
				slideSelecting = true;
				ApplySlideSelection(slideStartItem);
			}
			ApplySlideSelection(item);
		}
	}

	public bool IsSlideSelecting(int pointerId)
	{
		if (slideSelecting)
		{
			return pointerId == slidePointerId;
		}
		return false;
	}

	public void EndSlideSelection(int pointerId)
	{
		if (pointerId == slidePointerId)
		{
			ResetSlideSelection();
		}
	}

	private void ApplySlideSelection(Item item)
	{
		if (item == null || !slideVisitedItems.Add(item))
		{
			return;
		}
		bool flag = Selected(item);
		if (slideShouldSelect)
		{
			if (!flag && CanSelect(item))
			{
				TriggerItemSelect(item);
			}
		}
		else if (flag)
		{
			TriggerItemSelect(item);
		}
	}

	private void ResetSlideSelection()
	{
		slidePointerId = int.MinValue;
		slideStartItem = null;
		slideSelecting = false;
		slideShouldSelect = false;
		slideVisitedItems.Clear();
	}

	private ItemDisplayBase SpawnDisplay(Item item)
	{
		if (item != null && item.owner?.OwnerBft?.isPlayer == true)
		{
			return playerItemDisplayPool.Spawn(item);
		}
		if (item != null && item.owner?.OwnerBft?.isEnemy == true)
		{
			return enemyItemDisplayPool.Spawn(item);
		}
		return null;
	}
}
