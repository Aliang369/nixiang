using UnityEngine;

public class ContainerFixedSlotCountGenerator : ValueGenerator
{
	[SerializeField]
	private GameEvent.EventBFTType bftType;

	[SerializeField]
	private GameEvent.ContainerType containerType;

	public override int? GenerateValue(GameEvent eve)
	{
		return eve.GetOwnerContainer(bftType, containerType)?.GetFinalFixedSlotCount();
	}
}
