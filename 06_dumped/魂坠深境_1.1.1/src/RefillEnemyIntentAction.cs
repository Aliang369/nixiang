using System;

public class RefillEnemyIntentAction : ExecuteAction
{
	private readonly BattleFieldTransform bft;

	private readonly Item sourceItem;

	public RefillEnemyIntentAction(BattleFieldTransform bft, Item sourceItem)
	{
		this.bft = bft;
		this.sourceItem = sourceItem;
	}

	protected override void Update()
	{
		if (bft?.characterAI == null || bft.tableContainer == null)
		{
			return;
		}
		int num = Math.Max(0, bft.tableContainer.GetFinalFixedSlotCount() - bft.tableContainer.ItemsCount);
		if (num != 0)
		{
			int val = (bft.drawPileItemContainer?.ItemsCount ?? 0) + (bft.wastePileItemContainer?.ItemsCount ?? 0);
			int num2 = Math.Min(num, val);
			for (int i = 0; i < num2; i++)
			{
				EnqueueAction(new DrawFromPileAction(bft, sourceItem));
			}
		}
	}
}
