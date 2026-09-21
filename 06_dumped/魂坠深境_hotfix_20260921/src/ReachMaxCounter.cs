public class ReachMaxCounter : ItemConstraint
{
	public override bool? CheckItemAccept(Item item)
	{
		bool? flag = base.CheckItemAccept(item);
		if (flag != true)
		{
			return flag;
		}
		return (int)item.counter >= (int)item.maxCounter;
	}
}
