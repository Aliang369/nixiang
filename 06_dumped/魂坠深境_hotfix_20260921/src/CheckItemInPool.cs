using System.Collections.Generic;

public class CheckItemInPool : CheckNode
{
	public enum Type
	{
		All,
		Or
	}

	public Type type;

	public ItemTargetSelector sample;

	public ItemTargetSelector pool;

	public override bool? Check(GameEvent eve)
	{
		List<Item> list = sample?.GetTargetItems(eve);
		List<Item> poolItems = pool?.GetTargetItems(eve);
		if (list.IsNullOrEmpty() || poolItems.IsNullOrEmpty())
		{
			return null;
		}
		return type switch
		{
			Type.All => list.TrueForAll((Item it) => poolItems.Contains(it)), 
			Type.Or => list.Find((Item it) => poolItems.Contains(it)) != null, 
			_ => false, 
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { sample, pool };
	}
}
