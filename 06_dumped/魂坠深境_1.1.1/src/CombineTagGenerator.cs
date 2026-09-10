using System.Collections.Generic;

public class CombineTagGenerator : ItemTagGenerator
{
	public List<ItemTagGenerator> generators;

	public override List<ItemTag> GenerateItemTags(GameEvent eve)
	{
		List<ItemTag> list = new List<ItemTag>();
		foreach (ItemTagGenerator generator in generators)
		{
			List<ItemTag> list2 = generator?.GenerateItemTags(eve);
			if (list2 != null)
			{
				list.AddRange(list2);
			}
		}
		return list;
	}

	public override List<IEffectNode> GetChildren()
	{
		if (generators == null)
		{
			return new List<IEffectNode>();
		}
		return new List<IEffectNode>(generators);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			refTags = new NodeDisplayInfo.Element<List<ItemTag>>(GenerateItemTags(eve))
		};
	}
}
