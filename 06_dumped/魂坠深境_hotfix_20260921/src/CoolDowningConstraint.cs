public class CoolDowningConstraint : ItemConstraint
{
	public override bool? CheckItemAccept(Item item)
	{
		bool? flag = base.CheckItemAccept(item);
		return flag.HasValue && flag.Value && item.HasSpell && (int)item.cooldown > 0;
	}
}
