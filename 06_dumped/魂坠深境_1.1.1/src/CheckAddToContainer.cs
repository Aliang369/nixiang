public class CheckAddToContainer : CheckNode
{
	public GameEvent.ContainerType containerType;

	public GameEvent.EventBFTType bftType;

	public override bool? Check(GameEvent eve)
	{
		if (eve.addItemInfo == null)
		{
			return false;
		}
		ItemContainer ownerContainer = eve.GetOwnerContainer(bftType, containerType);
		return eve.addItemInfo.targetContainer == ownerContainer;
	}
}
