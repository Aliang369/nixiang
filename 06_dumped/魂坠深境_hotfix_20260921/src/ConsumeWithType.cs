using UnityEngine;

public class ConsumeWithType : CheckNode
{
	[SerializeField]
	private ConsumeItemAction.ConsumeType type;

	public override bool? Check(GameEvent eve)
	{
		if (eve.type != GameEvent.Type.BeforeConsume && eve.type != GameEvent.Type.AfterConsume && eve.type != GameEvent.Type.ConsumeResolveInfo)
		{
			return false;
		}
		if (eve.consumeType == type)
		{
			return true;
		}
		return false;
	}
}
