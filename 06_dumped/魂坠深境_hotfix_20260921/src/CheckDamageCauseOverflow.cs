public class CheckDamageCauseOverflow : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		return eve.damage != null && eve.damage.IsWithDmaageOverflow;
	}
}
