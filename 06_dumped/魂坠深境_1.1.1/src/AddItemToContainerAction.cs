using System;
using System.Collections;
using UnityEngine;

public class AddItemToContainerAction : GameAction
{
	private readonly object _source;

	private readonly Func<int> insertIndexer;

	private readonly Action<Item> onItemAdded;

	private readonly Action<Item> onItemAddRejected;

	private readonly WrapperEnumerator onPreAddItem;

	private readonly Item targetItem;

	private readonly ItemContainer targetItemContainer;

	private readonly AddItemEventInfo.AddType type;

	public AddItemToContainerAction(ItemContainer targetItemContainer, Item targetItem, object source, AddItemEventInfo.AddType type, Func<int> insertIndexer = null, WrapperEnumerator onPreAddItem = null, Action<Item> onItemAdded = null, Action<Item> onItemAddRejected = null)
	{
		this.targetItemContainer = targetItemContainer;
		this.targetItem = targetItem;
		_source = source;
		this.insertIndexer = insertIndexer;
		this.onItemAdded = onItemAdded;
		this.onItemAddRejected = onItemAddRejected;
		this.onPreAddItem = onPreAddItem;
		this.type = type;
	}

	protected override void Update()
	{
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		if (targetItemContainer == null)
		{
			onItemAddRejected?.Invoke(targetItem);
			FinishAction();
			return;
		}
		if (!targetItemContainer.CanAcceptItemWithinHardLimit(targetItem))
		{
			gameState.actionManager.ShowTableItemHardLimitHintOnce();
			onItemAddRejected?.Invoke(targetItem);
			FinishAction();
			return;
		}
		AddItemEventInfo addInfo = new AddItemEventInfo();
		addInfo.source = _source;
		addInfo.addItem = targetItem;
		addInfo.targetContainer = targetItemContainer;
		addInfo.addType = type;
		if (gameState.gameEventManager.TriggerAddItemToContainerResolve(addInfo).reject)
		{
			onItemAddRejected?.Invoke(targetItem);
			FinishAction();
			return;
		}
		if (!targetItemContainer.CanAcceptItemWithinHardLimit(addInfo.addItem))
		{
			gameState.actionManager.ShowTableItemHardLimitHintOnce();
			onItemAddRejected?.Invoke(targetItem);
			FinishAction();
			return;
		}
		gameState.gameEventManager.TriggerBeforeItemAdd(addInfo);
		if (targetItemContainer.type == ItemContainer.ContainerType.Table)
		{
			BattleFieldTransform ownerBft = targetItemContainer.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer)
			{
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_player_card_draw);
			}
			else
			{
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_foe_card_draw);
			}
		}
		int index = insertIndexer?.Invoke() ?? 0;
		if (onPreAddItem != null && !onPreAddItem.Finished)
		{
			EnqueueAction(new EnumeratorAction(OnPreAddItem));
		}
		EnqueueAction(AddItem);
		ItemContainer itemContainer = targetItemContainer;
		if (itemContainer != null && itemContainer.type == ItemContainer.ContainerType.Deck)
		{
			BattleFieldTransform ownerBft = itemContainer.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer)
			{
				gameState.recordManager.TrackJournalRecord(new AddItemToDeckRecord(targetItem));
			}
		}
		FinishAction();
		void AddItem()
		{
			if (!targetItemContainer.AddItem(addInfo, index))
			{
				if (!targetItemContainer.CanAcceptItemWithinHardLimit(addInfo.addItem))
				{
					gameState.actionManager.ShowTableItemHardLimitHintOnce();
				}
				onItemAddRejected?.Invoke(targetItem);
			}
			else
			{
				onItemAdded?.Invoke(addInfo.addItem);
			}
		}
		IEnumerator OnPreAddItem()
		{
			yield return new WaitUntil(() => onPreAddItem.Finished);
		}
	}
}
