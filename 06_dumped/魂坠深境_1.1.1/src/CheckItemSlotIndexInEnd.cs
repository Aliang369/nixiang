using System.Collections.Generic;
using UnityEngine;

public class CheckItemSlotIndexInEnd : CheckNode
{
	public enum CheckType
	{
		Head,
		Tail,
		Any
	}

	[SerializeField]
	private ItemTargetSelector targetSelector;

	public CheckType type;

	public override bool? Check(GameEvent eve)
	{
		Item singleItem = targetSelector.GetSingleItem(eve);
		if (singleItem == null)
		{
			return null;
		}
		if (singleItem.owner == null)
		{
			return false;
		}
		return type switch
		{
			CheckType.Head => singleItem.owner.firstItem == singleItem, 
			CheckType.Tail => singleItem.owner.lastItem == singleItem, 
			CheckType.Any => singleItem.owner.firstItem == singleItem || singleItem.owner.lastItem == singleItem, 
			_ => false, 
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector };
	}
}
