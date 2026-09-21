public class CheckDamageIsOverflow : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		return eve.damage != null && eve.damage.isOverflowDamage;
	}
}
