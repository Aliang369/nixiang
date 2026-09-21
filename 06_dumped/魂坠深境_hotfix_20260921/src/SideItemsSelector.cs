using System;
using System.Collections.Generic;
using System.Linq;

public class SideItemsSelector : ItemTargetSelector
{
	public enum IterType
	{
		RightNeighbourStart,
		LeftNeighbourStart,
		RightestStart,
		LeftestStart,
		BothNeightbourStart,
		BothEndStart
	}

	public ItemTargetSelector pivot;

	public GameEvent.EventBFTType bftType;

	public CheckNode checker;

	public GameEvent.ContainerType containerType;

	public ValueGenerator countGenerator;

	public bool countNoAccept = true;

	public bool ignoreSpellCost = true;

	public bool includePivot;

	public bool stopAtNonAccpet;

	public IterType type;

	private bool NeedPivot
	{
		get
		{
			if (type != IterType.LeftNeighbourStart && type != IterType.RightNeighbourStart)
			{
				return type == IterType.BothNeightbourStart;
			}
			return true;
		}
	}

	public override List<Item> GetTargetItems(GameEvent eve)
	{
		if (eve.triggerType == GameEvent.TriggerType.OutBattleDisplay)
		{
			return null;
		}
		ItemContainer itemContainer = null;
		Item pivotItem = null;
		int pivotIndex = -1;
		if (NeedPivot)
		{
			pivotItem = pivot.GetSingleItem(eve);
			Item item = pivotItem;
			if (item == null || !item.ownerSlotIndex.HasValue)
			{
				return null;
			}
			pivotIndex = pivotItem.ownerSlotIndex.Value;
			itemContainer = pivotItem.owner;
			if (itemContainer == null)
			{
				return null;
			}
		}
		else
		{
			itemContainer = eve.GetOwnerContainer(bftType, containerType);
			if (itemContainer == null)
			{
				return null;
			}
		}
		int num = 1;
		if (countGenerator != null)
		{
			int? num2 = countGenerator.GenerateValue(eve);
			if (!num2.HasValue)
			{
				return null;
			}
			num = num2.Value;
		}
		HashSet<Item> hashSet = new HashSet<Item>();
		List<Item> items = itemContainer.GetAllItems();
		bool returnNull = false;
		Func<Item, bool> func = (Item item6) => ignoreSpellCost && item6.IsAddAsSpellCost(eve.gameState);
		Func<Item, bool> func2 = delegate(Item item6)
		{
			if (item6 == pivotItem)
			{
				return false;
			}
			Item nodeTempItem = eve.NodeTempItem;
			eve.NodeTempItem = item6;
			if (checker != null)
			{
				bool? flag = checker.Check(eve);
				if (!flag.HasValue)
				{
					returnNull = true;
					return false;
				}
				if (flag == false)
				{
					eve.NodeTempItem = nodeTempItem;
					return false;
				}
			}
			eve.NodeTempItem = nodeTempItem;
			return true;
		};
		if (includePivot)
		{
			hashSet.Add(pivotItem);
		}
		if (type == IterType.BothEndStart || type == IterType.BothNeightbourStart)
		{
			int num3 = num;
			int num4 = num;
			switch (type)
			{
			case IterType.BothNeightbourStart:
			{
				for (int num7 = pivotIndex - 1; num7 >= 0; num7--)
				{
					Item item4 = items[num7];
					if (!func(item4))
					{
						if (num3 == 0)
						{
							break;
						}
						if (countNoAccept)
						{
							num3--;
						}
						if (func2(item4))
						{
							hashSet.Add(item4);
							if (!countNoAccept)
							{
								num3--;
							}
						}
						else if (stopAtNonAccpet)
						{
							break;
						}
					}
				}
				for (int num8 = pivotIndex + 1; num8 < items.Count; num8++)
				{
					Item item5 = items[num8];
					if (func(item5))
					{
						continue;
					}
					if (num4 == 0)
					{
						break;
					}
					if (countNoAccept)
					{
						num4--;
					}
					if (func2(item5))
					{
						hashSet.Add(item5);
						if (!countNoAccept)
						{
							num4--;
						}
					}
					else if (stopAtNonAccpet)
					{
						break;
					}
				}
				break;
			}
			case IterType.BothEndStart:
			{
				for (int num5 = 0; num5 < items.Count; num5++)
				{
					Item item2 = items[num5];
					if (func(item2))
					{
						continue;
					}
					if (num3 == 0)
					{
						break;
					}
					if (countNoAccept)
					{
						num3--;
					}
					if (func2(item2))
					{
						hashSet.Add(item2);
						if (!countNoAccept)
						{
							num3--;
						}
					}
					else if (stopAtNonAccpet)
					{
						break;
					}
				}
				for (int num6 = items.Count - 1; num6 >= 0; num6--)
				{
					Item item3 = items[num6];
					if (!func(item3))
					{
						if (num4 == 0)
						{
							break;
						}
						if (countNoAccept)
						{
							num4--;
						}
						if (func2(item3))
						{
							hashSet.Add(item3);
							if (!countNoAccept)
							{
								num4--;
							}
						}
						else if (stopAtNonAccpet)
						{
							break;
						}
					}
				}
				break;
			}
			}
		}
		else
		{
			foreach (Item item6 in itemIter())
			{
				if (func(item6))
				{
					continue;
				}
				if (num == 0)
				{
					break;
				}
				if (countNoAccept)
				{
					num--;
				}
				if (func2(item6))
				{
					hashSet.Add(item6);
					if (!countNoAccept)
					{
						num--;
					}
				}
				else if (stopAtNonAccpet)
				{
					break;
				}
			}
		}
		if (returnNull)
		{
			return null;
		}
		return hashSet.ToList();
		IEnumerable<Item> itemIter()
		{
			switch (type)
			{
			case IterType.RightNeighbourStart:
			{
				for (int i = pivotIndex + 1; i < items.Count; i++)
				{
					yield return items[i];
				}
				break;
			}
			case IterType.LeftNeighbourStart:
			{
				for (int i = pivotIndex - 1; i >= 0; i--)
				{
					yield return items[i];
				}
				break;
			}
			case IterType.RightestStart:
			{
				for (int i = items.Count - 1; i >= 0; i--)
				{
					yield return items[i];
				}
				break;
			}
			case IterType.LeftestStart:
			{
				for (int i = 0; i < items.Count; i++)
				{
					yield return items[i];
				}
				break;
			}
			}
		}
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { pivot, checker, countGenerator };
	}
}
