public class WithSpell : ItemConstraint
{
	public override bool? CheckItemAccept(Item item)
	{
		bool? flag = base.CheckItemAccept(item);
		if (flag != true)
		{
			return flag;
		}
		return item?.CurrentSpell != null;
	}
}
