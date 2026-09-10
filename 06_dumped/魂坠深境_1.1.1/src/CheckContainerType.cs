public class CheckContainerType : CheckNode
{
	public ItemContainer.ContainerType type;

	public GameEvent.EveObjType objType;

	public override bool? Check(GameEvent eve)
	{
		ItemContainer eveObj = eve.GetEveObj<ItemContainer>(objType);
		if (eveObj == null)
		{
			return false;
		}
		return eveObj.type == type;
	}
}
