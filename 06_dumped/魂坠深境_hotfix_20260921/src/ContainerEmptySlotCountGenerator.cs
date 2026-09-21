using System.Linq;
using UnityEngine;

public class ContainerEmptySlotCountGenerator : ValueGenerator
{
	[SerializeField]
	private GameEvent.EventBFTType bftType;

	[SerializeField]
	private GameEvent.ContainerType containerType;

	public override int? GenerateValue(GameEvent eve)
	{
		return eve.GetOwnerContainer(bftType, containerType)?.fixedSlots.Count((Item item) => item == null);
	}
}
