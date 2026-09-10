public class CheckAddType : CheckNode
{
	public AddItemEventInfo.AddType type;

	public override bool? Check(GameEvent eve)
	{
		if (eve.addItemInfo == null)
		{
			return false;
		}
		return eve.addItemInfo.addType == type;
	}
}
