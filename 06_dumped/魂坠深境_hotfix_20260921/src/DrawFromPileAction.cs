using System;
using System.Collections.Generic;
using GameRandom;

internal class DrawFromPileAction : ExecuteAction
{
	public enum DrawMode
	{
		Random,
		FirstAvailable
	}

	private readonly BattleFieldTransform bft;

	private readonly DrawMode drawMode;

	private readonly Item sourceItem;

	private readonly int insertIndex;

	private readonly Action<Item> onDrawCompleted;

	public DrawFromPileAction(BattleFieldTransform bft, Item sourceItem, int insertIndex = 0, DrawMode drawMode = DrawMode.Random, Action<Item> onDrawCompleted = null)
	{
		this.bft = bft;
		this.sourceItem = sourceItem;
		this.insertIndex = insertIndex;
		this.drawMode = drawMode;
		this.onDrawCompleted = onDrawCompleted;
		Ignore(ActionLock.Type.DrawFromPile);
	}

	protected override void Update()
	{
		if (!bft.tableContainer.CanAcceptItemWithinHardLimit())
		{
			gameState.actionManager.ShowTableItemHardLimitHintOnce();
			onDrawCompleted?.Invoke(null);
			return;
		}
		ItemContainer drawPileItemContainer = bft.drawPileItemContainer;
		ItemContainer wastePileItemContainer = bft.wastePileItemContainer;
		List<Item> allItems = drawPileItemContainer.GetAllItems();
		List<Item> allItems2 = wastePileItemContainer.GetAllItems();
		List<(Item, GameEvent.ResolveInfo)> list = allItems.ConvertAll((Item it) => (it: it, gameState.gameEventManager.TriggerDrawItemResolve(it, GameEvent.TriggerType.Real))).FindAll(((Item, GameEvent.ResolveInfo) ir) => !ir.Item2.reject);
		if (list.Count == 0)
		{
			if (!gameState.gameEventManager.TriggerDrawCardRecycleResolve(bft, GameEvent.TriggerType.Real).reject)
			{
				if (allItems2.Count != 0)
				{
					EnqueueAction(new RecycleWastePileAction(bft), passIgnoreLocks: false);
					EnqueueAction(new DrawFromPileAction(bft, sourceItem, insertIndex, drawMode, onDrawCompleted), passIgnoreLocks: false);
				}
				else
				{
					onDrawCompleted?.Invoke(null);
				}
			}
			else
			{
				onDrawCompleted?.Invoke(null);
			}
			return;
		}
		Item drawItem = ((drawMode == DrawMode.FirstAvailable) ? list[0].Item1 : list.Find(((Item, GameEvent.ResolveInfo) ir) => ir.Item2.promote).Item1);
		if (drawItem == null)
		{
			drawItem = list.SelectRandom(gameState.randomManager.NextDouble).Item1;
		}
		GameEvent.ResolveInfo resolveInfo = gameState.gameEventManager.TriggerDrawItemReplaceResolve(drawItem, bft);
		if (resolveInfo.replaceItem != null)
		{
			drawItem = resolveInfo.replaceItem;
		}
		EnqueueAction(new TransferItemAction(drawItem, bft.tableContainer, null, TransferItemAction.TransferAnimType.Draw, () => insertIndex, onDrawCompleted));
		EnqueueAction(new WaitAction(0.1f / base.actionTimeScale));
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerDrawFromPile(drawItem, sourceItem);
		});
	}
}
