using System.Collections.Generic;
using System.Linq;

public class ItemCastTimesInThisRound : ValueGenerator
{
	public ItemTargetSelector itemTargetSelector;

	public override int? GenerateValue(GameEvent eve)
	{
		if (eve.gameState == null)
		{
			return null;
		}
		Item item = itemTargetSelector.GetSingleItem(eve);
		if (item == null)
		{
			return null;
		}
		return eve.gameState.recordManager.castedItemsThisRound.Count((Item it) => it == item);
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { itemTargetSelector };
	}
}
