public class CheckResolveNotRejected : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		if (eve.resolveInfo == null)
		{
			return null;
		}
		return !eve.resolveInfo.reject;
	}
}
