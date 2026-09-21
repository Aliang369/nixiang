using System.Collections.Generic;
using System.Linq;
using GameRandom;

public class RandomMoveEnchantInItems : Effect
{
	public ItemTargetSelector itemTargetSelector;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		IEnumerable<Item> enumerable = itemTargetSelector.GetTargetItems(eve).OrEmptyIfNull();
		List<(Item, Enchant)> list = new List<(Item, Enchant)>();
		foreach (Item item3 in enumerable)
		{
			foreach (Enchant enchant in item3.Enchants)
			{
				list.Add((item3, enchant));
			}
		}
		if (list.IsNullOrEmpty())
		{
			return NodeCastResult.Failure;
		}
		(Item, Enchant) pair = list.SelectRandom(eve.gameState.randomManager.NextDouble);
		List<Item> list2 = enumerable.Where((Item it) => pair.Item1 != it && (pair.Item2.def.addConstraint?.CheckItemAccept(it) ?? true)).ToList();
		if (!list2.IsNullOrEmpty())
		{
			Enchant item = pair.Item2;
			Item item2 = pair.Item1;
			Item targetItem = list2.SelectRandom(eve.gameState.randomManager.NextDouble);
			ChangeEnchantAction action = new ChangeEnchantAction(item2, null, item.def, -(int)item.value);
			Effect.EnqueueAction(eve, action);
			ChangeEnchantAction action2 = new ChangeEnchantAction(targetItem, null, item.def, item.value);
			Effect.EnqueueAction(eve, action2);
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { itemTargetSelector };
	}
}
