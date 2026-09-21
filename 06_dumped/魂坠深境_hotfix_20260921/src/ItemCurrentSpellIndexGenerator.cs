using UnityEngine;

public class ItemCurrentSpellIndexGenerator : ValueGenerator
{
	[SerializeField]
	private ItemTargetSelector itemTargetSelector;

	public override int? GenerateValue(GameEvent eve)
	{
		return itemTargetSelector.GetSingleItem(eve)?.CurrentSpellIndex;
	}
}
