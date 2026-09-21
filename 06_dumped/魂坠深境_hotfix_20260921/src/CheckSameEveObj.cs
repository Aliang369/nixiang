public class CheckSameEveObj : CheckNode
{
	public GameEvent.EveObjType a;

	public GameEvent.EveObjType b;

	public override bool? Check(GameEvent eve)
	{
		object eveObj = eve.GetEveObj(a);
		object eveObj2 = eve.GetEveObj(b);
		return eveObj == eveObj2;
	}
}
