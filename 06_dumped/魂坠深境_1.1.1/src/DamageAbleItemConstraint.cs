public class DamageAbleItemConstraint : ItemConstraint
{
	public bool pierce;

	public override bool? CheckItemAccept(Item item)
	{
		bool? flag = base.CheckItemAccept(item);
		if (flag != true)
		{
			return flag;
		}
		if (!pierce && item.owner != null && item.owner.hasValidDefence)
		{
			return item.DefenceLike;
		}
		return item.DamageAble;
	}
}
