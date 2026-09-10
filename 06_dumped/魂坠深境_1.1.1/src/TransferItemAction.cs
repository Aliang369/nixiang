using System;

public class TransferItemAction : ExecuteAction
{
	public enum TransferAnimType
	{
		None,
		Transfer,
		Draw
	}

	private readonly TransferAnimType animType;

	private readonly Func<int> insertIndexer;

	private readonly Item sourceItem;

	private readonly ItemContainer targetContainer;

	private readonly Item targetItem;

	private readonly Action<Item> onItemTransferred;

	public TransferItemAction(Item targetItem, ItemContainer targetContainer, Item sourceItem, TransferAnimType animType, Func<int> insertIndexer = null, Action<Item> onItemTransferred = null)
	{
		this.targetItem = targetItem;
		this.targetContainer = targetContainer;
		this.sourceItem = sourceItem;
		this.insertIndexer = insertIndexer;
		this.animType = animType;
		this.onItemTransferred = onItemTransferred;
	}

	protected override void Update()
	{
		if (targetItem == null || targetContainer == null)
		{
			onItemTransferred?.Invoke(null);
			return;
		}
		ItemContainer owner = targetItem.owner;
		if (owner != null && owner.type == ItemContainer.ContainerType.Table && targetItem.BlocksExternalAction(sourceItem))
		{
			onItemTransferred?.Invoke(null);
			return;
		}
		if (!targetContainer.CanAcceptItemWithinHardLimit(targetItem))
		{
			gameState.actionManager.ShowTableItemHardLimitHintOnce();
			onItemTransferred?.Invoke(null);
			return;
		}
		Action<Item> onItemAdded = null;
		switch (animType)
		{
		case TransferAnimType.Transfer:
		{
			BattleFieldTransform ownerBft = targetItem.OwnerBft;
			bool fromPlayerTable = ownerBft != null && ownerBft.isPlayer && targetItem.owner.type == ItemContainer.ContainerType.Table;
			BattleFieldTransform owner2 = targetContainer.owner;
			bool toPlayerTable = owner2 != null && owner2.isPlayer && targetContainer.type == ItemContainer.ContainerType.Table;
			BattleFieldTransform ownerBft2 = targetItem.OwnerBft;
			bool fromEnemyTable = ownerBft2 != null && !ownerBft2.isPlayer && targetItem.owner.type == ItemContainer.ContainerType.Table;
			BattleFieldTransform owner3 = targetContainer.owner;
			bool toEnemyTable = owner3 != null && !owner3.isPlayer && targetContainer.type == ItemContainer.ContainerType.Table;
			ConvertablePivot fromPivot = targetItem.GetPivot(gameState);
			onItemAdded = OnItemTransfer;
			break;
		}
		case TransferAnimType.Draw:
			onItemAdded = OnItemDraw;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case TransferAnimType.None:
			break;
		}
		EnqueueAction(new RemoveItemAction(targetItem, sourceItem));
		EnqueueAction(new AddItemToContainerAction(targetContainer, targetItem, sourceItem, AddItemEventInfo.AddType.Transfer, onItemAdded: OnItemAdded, insertIndexer: insertIndexer, onPreAddItem: null, onItemAddRejected: delegate
		{
			onItemTransferred?.Invoke(null);
		}));
		void OnItemAdded(Item addedItem)
		{
			onItemAdded?.Invoke(addedItem);
			onItemTransferred?.Invoke(addedItem);
		}
		void OnItemDraw(Item item)
		{
			BattleFieldTransform ownerBft3 = item.OwnerBft;
			if (ownerBft3 != null && ownerBft3.isPlayer)
			{
				ItemContainer owner4 = item.owner;
				if (owner4 != null && owner4.type == ItemContainer.ContainerType.Table)
				{
					gameState.UIManager.playerItemArea.OnDrawFromPile(item);
					return;
				}
			}
			BattleFieldTransform ownerBft4 = item.OwnerBft;
			if (ownerBft4 != null && ownerBft4.isEnemy)
			{
				ItemContainer owner5 = item.owner;
				if (owner5 != null && owner5.type == ItemContainer.ContainerType.Table)
				{
					gameState.UIManager.enemyBftUIDisplay.tableItemArea.OnDraw(item);
				}
			}
		}
	}
}
