using System.Collections.Generic;

public class CastFireControllerBacklashFireball : FireControllerAttackEffect
{
	public int baseDamage = 16;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		FireControllerExileEffect.DealDamageToOpponent(eve, FireControllerExileEffect.UseConfiguredOrDefault(baseDamage, 16));
		Item caster = eve.CasterItem;
		ItemContainer container = caster?.owner;
		ItemDefinition itemDef = FireControllerExileEffect.GetItemDef(eve, "a47f2520cd3271e4d84166493bb5170c");
		if (caster == null || container == null || itemDef == null)
		{
			return NodeCastResult.Success;
		}
		Item item = new Item(itemDef);
		item.Upgrade(eve.gameState);
		FireControllerExileEffect.SpawnGeneratedItem(eve, item, container, () => GetRelativeIndex(caster, container, 0));
		Item item2 = new Item(itemDef);
		item2.Upgrade(eve.gameState);
		FireControllerExileEffect.SpawnGeneratedItem(eve, item2, container, () => GetRelativeIndex(caster, container, 1));
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		NodeDisplayInfo nodeDisplayInfo = FireControllerExileEffect.DamageDisplay(eve, FireControllerExileEffect.UseConfiguredOrDefault(baseDamage, 16));
		Item item = FireControllerExileEffect.CreateDisplayItem(eve, "a47f2520cd3271e4d84166493bb5170c", 0, 1);
		if (item != null)
		{
			nodeDisplayInfo.refItems = new NodeDisplayInfo.Element<List<Item>>(new List<Item> { item });
			nodeDisplayInfo.addItemInfo = new NodeDisplayInfo.Element<AddSpawnItemsInfo>(FireControllerExileEffect.AddItemDisplay(eve, item, 2));
		}
		return nodeDisplayInfo;
	}

	private static int GetRelativeIndex(Item caster, ItemContainer container, int offset)
	{
		if (caster?.owner != container || !caster.ownerSlotIndex.HasValue)
		{
			return -1;
		}
		return caster.ownerSlotIndex.Value + offset;
	}
}
