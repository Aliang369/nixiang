public class CheckEveString : CheckNode
{
	public string str;

	public override bool? Check(GameEvent eve)
	{
		if (eve.stringValue == null)
		{
			return null;
		}
		if (eve.stringValue == str)
		{
			return true;
		}
		return false;
	}
}
