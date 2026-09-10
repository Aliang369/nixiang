public class CheckWorldEventObjDef : CheckNode
{
	public GameEvent.EveObjType type;

	public WorldEvent worldEvent;

	public override bool? Check(GameEvent eve)
	{
		WorldEvent eveObj = eve.GetEveObj<WorldEvent>(type);
		return eveObj != null && worldEvent != null && eveObj.FullID == worldEvent.FullID;
	}
}
