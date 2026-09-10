public class CheckOwnerBftWithHealth : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		if (eve.Caster is IBftOwnable { OwnerBft: not null } bftOwnable)
		{
			return (int)bftOwnable.OwnerBft.health > 0;
		}
		return null;
	}
}
