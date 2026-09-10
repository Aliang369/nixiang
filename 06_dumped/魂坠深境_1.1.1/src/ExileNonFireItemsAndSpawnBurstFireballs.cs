public class ExileNonFireItemsAndSpawnBurstFireballs : FireControllerExileEffect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item targetItem = eve.TargetItem;
		ItemContainer itemContainer = targetItem?.owner;
		ItemDefinition itemDef = FireControllerExileEffect.GetItemDef(eve, "a47f2520cd3271e4d84166493bb5170c");
		if (itemContainer == null || itemDef == null)
		{
			return NodeCastResult.Success;
		}
		int index = itemContainer.GetRawIndex(targetItem);
		int num = ((!FireControllerExileEffect.IsSkinBranch(eve)) ? 1 : 2);
		Item casterItem = eve.CasterItem;
		int num2 = (((casterItem != null && casterItem.upgraded) || FireControllerExileEffect.IsSkinBranch(eve)) ? 1 : 0);
		Effect.EnqueueAction(eve, new ExileItemAction(targetItem, eve.CasterItem));
		for (int i = 0; i < num; i++)
		{
			Item item = new Item(itemDef);
			for (int j = 0; j < num2; j++)
			{
				item.Upgrade(eve.gameState);
			}
			FireControllerExileEffect.SpawnGeneratedItem(eve, item, itemContainer, () => index);
		}
		return NodeCastResult.Success;
	}
}
