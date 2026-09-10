using System.Collections.Generic;
using System.Linq;

public class ExileTargetAndSpawnFireBoltAtOriginalIndex : FireControllerExileEffect
{
	public ItemTargetSelector targetSelector;

	public ItemDefinition itemDef;

	public int spawnCount = 1;

	public int upgradeLevel;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = targetSelector?.GetTargetItems(eve) ?? ((eve.TargetItem == null) ? new List<Item>() : new List<Item> { eve.TargetItem });
		if (itemDef == null || list == null)
		{
			return NodeCastResult.Success;
		}
		foreach (Item item2 in list.Where((Item item2) => item2?.owner != null).ToList())
		{
			ItemContainer owner = item2.owner;
			int index = owner.GetRawIndex(item2);
			Effect.EnqueueAction(eve, new ExileItemAction(item2, eve.CasterItem));
			for (int num = 0; num < spawnCount; num++)
			{
				Item item = new Item(itemDef);
				for (int num2 = 0; num2 < upgradeLevel; num2++)
				{
					item.Upgrade(eve.gameState);
				}
				FireControllerExileEffect.SpawnGeneratedItem(eve, item, owner, () => index);
			}
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector };
	}
}
