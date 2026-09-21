using System.Collections.Generic;

public class FilterNewItems : NewItemGenerator
{
	public NewItemGenerator itemGenerator;

	public ItemConstraint constraint;

	public CheckNode checkFilter;

	public override List<Item> GenerateNewItems(GameEvent eve)
	{
		List<Item> list = itemGenerator?.GenerateNewItems(eve);
		if (list == null)
		{
			return new List<Item>();
		}
		if (constraint != null)
		{
			list = list.FindAll((Item it) => constraint.CheckItemAccept(it) == true);
		}
		if (checkFilter != null)
		{
			Item nodeTempItem = eve.NodeTempItem;
			list = list.FindAll(delegate(Item it)
			{
				eve.NodeTempItem = it;
				return checkFilter.Check(eve) == true;
			});
			eve.NodeTempItem = nodeTempItem;
		}
		return list;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { itemGenerator, constraint, checkFilter };
	}
}
