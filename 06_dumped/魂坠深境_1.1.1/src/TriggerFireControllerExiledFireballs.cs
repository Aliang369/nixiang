using System.Collections.Generic;
using System.Linq;

public class TriggerFireControllerExiledFireballs : FireControllerExileEffect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> tableItems = FireControllerExileEffect.GetTableItems(eve);
		Item casterItem = eve.CasterItem;
		if (casterItem != null && casterItem.upgraded)
		{
			tableItems.AddRange(FireControllerExileEffect.GetExilePileItems(eve));
		}
		foreach (Item item in tableItems.Where(FireControllerExileEffect.IsFireballFamily))
		{
			FireControllerExileEffect.TriggerItemSpell(eve, item);
		}
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			outputBool = new NodeDisplayInfo.Element<bool?>(eve.CasterItem?.upgraded ?? false)
		};
	}
}
