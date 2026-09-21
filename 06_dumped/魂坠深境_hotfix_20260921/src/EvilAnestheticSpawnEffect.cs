using System.Collections.Generic;
using System.Linq;
using GameRandom;

public sealed class EvilAnestheticSpawnEffect : Effect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		eve.GetEventBFT(GameEvent.EventBFTType.Owner);
		ItemContainer table = eve.GetOwnerContainer(GameEvent.EventBFTType.Owner, GameEvent.ContainerType.TableContainer);
		List<ItemDefinition> list = eve.gameState?.applicationState?.characterManager?.GetCharacterClassDef(CharacterClassType.Dancer)?.GetPoolItems(eve.gameState.applicationState);
		if (table == null || list == null)
		{
			return NodeCastResult.Success;
		}
		foreach (Item item in table.items.Where((Item item) => item != eve.CasterItem && item.HasSpell))
		{
			Effect.EnqueueAction(eve, new ChangeCoolDownAction(item, eve.CasterItem, 1));
		}
		foreach (ItemDefinition item2 in list.Where((ItemDefinition d) => d != null).ToList().SelectRandomCountUnique(3, eve.gameState.randomManager.NextDouble))
		{
			Effect.EnqueueAction(eve, new SpawnItemAction(new Item(item2), table, withAnim: true, eve.CasterItem, () => table.ItemsCount));
		}
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(3)
		};
	}
}
