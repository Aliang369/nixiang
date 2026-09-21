public class CheckOwner : CheckNode
{
	public GameEvent.EventBFTType ownerType;

	public GameEvent.EventItemType target;

	public override bool? Check(GameEvent eve)
	{
		Item eventItem = eve.GetEventItem(target);
		if (eventItem == null)
		{
			return false;
		}
		BattleFieldTransform eventBFT = eve.GetEventBFT(ownerType);
		if (eventBFT != null)
		{
			return (eventItem.OwnerBft == eventBFT) ? true : false;
		}
		return false;
	}
}
