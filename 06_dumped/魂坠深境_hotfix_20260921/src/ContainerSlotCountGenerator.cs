using UnityEngine;

public class ContainerSlotCountGenerator : ValueGenerator
{
	[SerializeField]
	private GameEvent.EventBFTType bftType;

	[SerializeField]
	private GameEvent.ContainerType containerType;

	public override int? GenerateValue(GameEvent eve)
	{
		return eve.GetOwnerContainer(bftType, containerType)?.ItemsCount;
	}
}
