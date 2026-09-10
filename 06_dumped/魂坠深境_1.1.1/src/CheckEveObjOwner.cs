public class CheckEveObjOwner : CheckNode
{
	public GameEvent.EventBFTType ownerType;

	public GameEvent.EveObjType objType;

	public override bool? Check(GameEvent eve)
	{
		IBftOwnable eveObj = eve.GetEveObj<IBftOwnable>(objType);
		if (eveObj == null)
		{
			return false;
		}
		BattleFieldTransform eventBFT = eve.GetEventBFT(ownerType);
		if (eventBFT != null)
		{
			return eveObj.OwnerBft == eventBFT;
		}
		return false;
	}
}
