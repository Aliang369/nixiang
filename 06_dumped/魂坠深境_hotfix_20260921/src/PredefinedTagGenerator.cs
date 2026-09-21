using System.Collections.Generic;

public class PredefinedTagGenerator : ItemTagGenerator
{
	public List<ItemTag> tags;

	public override List<ItemTag> GenerateItemTags(GameEvent eve)
	{
		return new List<ItemTag>(tags);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			refTags = new NodeDisplayInfo.Element<List<ItemTag>>(tags)
		};
	}
}
