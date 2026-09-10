using System.Collections.Generic;

public class SequenceItemSelector : ItemTargetSelector
{
	public GameEvent.ContainerType containerType;

	public GameEvent.EventBFTType bftType;

	public List<CheckNode> sequence;

	public override List<Item> GetTargetItems(GameEvent eve)
	{
		List<Item> list = null;
		ItemContainer ownerContainer = eve.GetOwnerContainer(bftType, containerType);
		if (ownerContainer == null)
		{
			return null;
		}
		List<Item> allItems = ownerContainer.GetAllItems();
		if (sequence.IsNullOrEmpty() || allItems.IsNullOrEmpty())
		{
			return null;
		}
		Item nodeTempItem = eve.NodeTempItem;
		for (int i = 0; i < allItems.Count; i++)
		{
			if (CheckIsSequence(i, allItems))
			{
				list = new List<Item>();
				for (int j = 0; j < sequence.Count; j++)
				{
					list.Add(allItems[i + j]);
				}
				break;
			}
		}
		eve.NodeTempItem = nodeTempItem;
		return list;
		bool CheckIsSequence(int current, List<Item> all)
		{
			for (int k = 0; k < sequence.Count; k++)
			{
				eve.NodeTempItem = all[current + k];
				if (sequence[k].Check(eve) != true)
				{
					return false;
				}
			}
			return true;
		}
	}

	public override List<IEffectNode> GetChildren()
	{
		if (sequence != null)
		{
			return new List<IEffectNode>(sequence);
		}
		return new List<IEffectNode>();
	}
}
