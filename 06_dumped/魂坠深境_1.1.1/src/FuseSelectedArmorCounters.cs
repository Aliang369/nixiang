using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FuseSelectedArmorCounters : Effect
{
	[SerializeField]
	private ItemDefGenerator targetDefGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (eve?.SelectedItems == null || eve.SelectedItems.Count == 0)
		{
			return NodeCastResult.Failure;
		}
		ItemDefinition targetDef = targetDefGenerator?.GenerateSingleItemDef(eve);
		List<Item> list = eve.SelectedItems.Where((Item item2) => item2 != null && item2.owner != null && (targetDef == null || item2.GetDef() == targetDef)).ToList();
		if (list.Count == 0)
		{
			return NodeCastResult.Failure;
		}
		Item item = list[0];
		int num = CombatValueLimit.Clamp(((IEnumerable<Item>)list).Sum((Func<Item, long>)((Item item2) => item2.GetCurrentDefendCounter())));
		foreach (Item item2 in list.Skip(1))
		{
			Effect.EnqueueAction(eve, new RemoveItemAction(item2, eve.CasterInfluence));
		}
		Effect.EnqueueAction(eve, new ChangeCounterAction(num - CombatValueLimit.Clamp((int)item.counter), item, eve.CasterInfluence, withPopupText: true));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetDefGenerator };
	}
}
