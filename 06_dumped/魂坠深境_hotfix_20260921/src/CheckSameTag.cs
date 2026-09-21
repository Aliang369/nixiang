using System.Collections.Generic;

public class CheckSameTag : CheckNode
{
	public ItemTagGenerator tgA;

	public ItemTagGenerator tgB;

	public override bool? Check(GameEvent eve)
	{
		ItemTag itemTag = tgA?.GenerateSingleItemTag(eve);
		ItemTag itemTag2 = tgB?.GenerateSingleItemTag(eve);
		if (itemTag == null || itemTag2 == null)
		{
			return null;
		}
		return itemTag == itemTag2;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { tgA, tgB };
	}
}
