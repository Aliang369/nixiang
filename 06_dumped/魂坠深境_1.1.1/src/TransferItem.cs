using System;
using System.Collections.Generic;

public class TransferItem : Effect
{
	public enum InsertType
	{
		InsertToEnd,
		InsertToHead,
		FixedIndex,
		PivotIndex,
		RandomInsert
	}

	public ItemTargetSelector itemSelector;

	public GameEvent.EventBFTType targetBFT;

	public GameEvent.ContainerType targetContainerType;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	public InsertType insertType;

	public ItemTargetSelector insertPivot;

	public ValueGenerator addSlotIndexGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (itemSelector == null)
		{
			return NodeCastResult.Failure;
		}
		foreach (Item item in itemSelector.GetTargetItems(eve).OrEmptyIfNull())
		{
			ItemContainer targetContainer = eve.GetOwnerContainer(targetBFT, targetContainerType);
			GameEvent eveCopy = eve.ShallowCopy();
			int fixedIndex = addSlotIndexGenerator?.GenerateValue(eveCopy) ?? (-1);
			Func<int> insertIndexer = delegate
			{
				switch (insertType)
				{
				case InsertType.InsertToEnd:
					return -1;
				case InsertType.InsertToHead:
					return 0;
				case InsertType.FixedIndex:
					return fixedIndex;
				case InsertType.PivotIndex:
				{
					Item singleItem = insertPivot.GetSingleItem(eveCopy);
					int num = ((addSlotIndexGenerator != null) ? addSlotIndexGenerator.GenerateValue(eveCopy).Value : 0);
					if (singleItem.owner != targetContainer)
					{
						return -1;
					}
					return (singleItem.ownerSlotIndex + num) ?? (-1);
				}
				case InsertType.RandomInsert:
					if (targetContainer.ItemsCount != 0)
					{
						return eve.gameState.randomManager.Next() % targetContainer.ItemsCount;
					}
					return 0;
				default:
					return -1;
				}
			};
			transformVFXEffect?.Play(eve, eve.CasterPivot, item);
			Effect.EnqueueAction(eve, new TransferItemAction(item, targetContainer, eve.CasterItem, TransferItemAction.TransferAnimType.Transfer, insertIndexer));
			hitEffect?.Play(eve, item);
		}
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			refItems = new NodeDisplayInfo.Element<List<Item>>(itemSelector.GetTargetItems(eve))
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { itemSelector, insertPivot };
	}
}
