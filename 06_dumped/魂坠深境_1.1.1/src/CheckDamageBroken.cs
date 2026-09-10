public class CheckDamageBroken : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		if (eve.damage == null)
		{
			return null;
		}
		if (eve.damage.targetItem != null && eve.damage.subCounter > 0 && eve.damage.subCounter == (int)eve.damage.targetItem.counter)
		{
			return true;
		}
		return false;
	}
}
