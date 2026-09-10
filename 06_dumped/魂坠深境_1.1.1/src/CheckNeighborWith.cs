using System.Collections.Generic;
using UnityEngine;

public class CheckNeighborWith : CheckNode
{
	public enum NeighborType
	{
		Any,
		All,
		RightToPivot,
		LeftToPivot
	}

	[SerializeField]
	private readonly bool includePivotSelf;

	[SerializeField]
	private ItemTargetSelector itemSelector;

	[SerializeField]
	private ItemTargetSelector pivotSelector;

	[SerializeField]
	private NeighborType type;

	[SerializeField]
	private readonly bool ignoreSpellCost;

	private int GetIndex(Item item)
	{
		if (ignoreSpellCost)
		{
			return item.ownerSlotIndex.Value;
		}
		return item.owner.GetRawIndex(item);
	}

	private bool WithValidIndex(Item item)
	{
		if (!ignoreSpellCost || !item.ownerSlotIndex.HasValue)
		{
			if (!ignoreSpellCost)
			{
				return item.owner != null;
			}
			return false;
		}
		return true;
	}

	public override bool? Check(GameEvent eve)
	{
		List<Item> targetItems = itemSelector.GetTargetItems(eve);
		Item pivot = pivotSelector.GetSingleItem(eve);
		if (targetItems == null || pivot == null)
		{
			return null;
		}
		if (!WithValidIndex(pivot))
		{
			return null;
		}
		if (targetItems.Count == 0)
		{
			return false;
		}
		return type switch
		{
			NeighborType.Any => targetItems.Find((Item it) => (WithValidIndex(it) && it.owner == pivot.owner && Mathf.Abs(GetIndex(it) - GetIndex(pivot)) == 1) || (includePivotSelf && it == pivot)) != null, 
			NeighborType.All => targetItems.TrueForAll((Item it) => (WithValidIndex(it) && it.owner == pivot.owner && Mathf.Abs(GetIndex(it) - GetIndex(pivot)) == 1) || (includePivotSelf && it == pivot)), 
			NeighborType.RightToPivot => targetItems.TrueForAll((Item it) => (WithValidIndex(it) && it.owner == pivot.owner && GetIndex(it) - GetIndex(pivot) == 1) || (includePivotSelf && it == pivot)), 
			NeighborType.LeftToPivot => targetItems.TrueForAll((Item it) => (WithValidIndex(it) && it.owner == pivot.owner && GetIndex(it) - GetIndex(pivot) == -1) || (includePivotSelf && it == pivot)), 
			_ => false, 
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { pivotSelector, itemSelector };
	}
}
