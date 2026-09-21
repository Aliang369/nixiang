using System;
using System.Collections.Generic;

public class CheckItemWithTag : CheckNode
{
	public enum OpType
	{
		And,
		Or
	}

	public ItemTagGenerator tagGenerator;

	public GameEvent.EventItemType target;

	public OpType type;

	public override bool? Check(GameEvent eve)
	{
		Item item = eve.GetEventItem(target);
		if (item == null)
		{
			return false;
		}
		List<ItemTag> list = tagGenerator.GenerateItemTags(eve);
		if (list == null)
		{
			return null;
		}
		return type switch
		{
			OpType.And => list.TrueForAll((ItemTag t) => item.HasTag(t)), 
			OpType.Or => list.Find((ItemTag t) => item.HasTag(t)) != null, 
			_ => throw new Exception($"OpType error: {(int)type}"), 
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { tagGenerator };
	}
}
