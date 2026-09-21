using UnityEngine;

public class SummonCompanionAction : ExecuteAction
{
	public BattleFieldTransform bft;

	public Item item;

	public SummonCompanionAction(Item item, BattleFieldTransform bft)
	{
		this.item = item;
		this.bft = bft;
	}

	protected override void Update()
	{
		ItemContainer owner = item.owner;
		if ((owner != null && owner.type == ItemContainer.ContainerType.Companion) || bft == null)
		{
			return;
		}
		gameState.gameEventManager.TriggerBeforeSummonCompanion(item, bft);
		ItemContainer companionContainer = bft.companionContainer;
		Vector3? startPosition = item.GetMainCanvasWorldPosition(gameState);
		ItemContainer owner2 = item.owner;
		if (owner2 != null && owner2.type == ItemContainer.ContainerType.Table)
		{
			BattleFieldTransform ownerBft = item.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer)
			{
				gameState.UIManager.playerBftUIDisplay.tableItemArea.OnTransferOut(item);
				goto IL_0119;
			}
		}
		ItemContainer owner3 = item.owner;
		if (owner3 != null && owner3.type == ItemContainer.ContainerType.Table)
		{
			BattleFieldTransform ownerBft2 = item.OwnerBft;
			if (ownerBft2 != null && ownerBft2.isEnemy)
			{
				gameState.UIManager.enemyBftUIDisplay.tableItemArea.OnTransferOut(item);
			}
		}
		goto IL_0119;
		IL_0119:
		Item newItem = item.Copy();
		EnqueueAction(new AddItemToContainerAction(companionContainer, newItem, item, AddItemEventInfo.AddType.Spawn));
		EnqueueAction(new RemoveItemAction(item, item));
		EnqueueAction(new ActionAction(delegate
		{
			if (bft.isPlayer)
			{
				gameState.UIManager.playerBftUIDisplay.companionItemArea.OnAddItem(newItem, startPosition);
			}
			else if (bft.isEnemy)
			{
				gameState.UIManager.enemyBftUIDisplay.companionItemArea.OnAddItem(newItem, startPosition);
			}
		}));
	}
}
