using System;
using System.Collections.Generic;

public abstract class ContainerItemArea : ItemArea
{
	[NonSerialized]
	private ItemContainer itemContainer;

	protected override void Init()
	{
		base.Init();
		UIBase.gameState.actionManager.actionFrameFinishEvent += _OnActionFrameFinished;
	}

	protected override void Dispose()
	{
		Unset();
		UIBase.gameState.actionManager.actionFrameFinishEvent -= _OnActionFrameFinished;
	}

	public virtual void Setup(ItemContainer container)
	{
		Unset();
		if (container != null)
		{
			itemContainer = container;
			ItemContainer obj = itemContainer;
			obj.itemSequenceChangeEvent = (Action<ItemContainer.ItemSequenceState, ItemContainer.ItemSequenceState>)Delegate.Combine(obj.itemSequenceChangeEvent, new Action<ItemContainer.ItemSequenceState, ItemContainer.ItemSequenceState>(_OnItemSequenceChange));
			SetDirty();
		}
	}

	public virtual void Unset()
	{
		if (itemContainer != null)
		{
			ItemContainer obj = itemContainer;
			obj.itemSequenceChangeEvent = (Action<ItemContainer.ItemSequenceState, ItemContainer.ItemSequenceState>)Delegate.Remove(obj.itemSequenceChangeEvent, new Action<ItemContainer.ItemSequenceState, ItemContainer.ItemSequenceState>(_OnItemSequenceChange));
			itemContainer = null;
		}
	}

	protected override List<Item> GetItems()
	{
		return itemContainer?.GetAllItems() ?? new List<Item>();
	}

	private void _OnItemSequenceChange(ItemContainer.ItemSequenceState before, ItemContainer.ItemSequenceState after)
	{
		SetDirty();
	}

	private void _OnActionFrameFinished()
	{
		SetDirty();
	}
}
