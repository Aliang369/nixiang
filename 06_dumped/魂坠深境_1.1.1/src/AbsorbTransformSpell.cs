using System.Collections.Generic;

public class AbsorbTransformSpell : Effect
{
	public ItemTargetSelector sourceItemSelector;

	public ItemTargetSelector targetItemSelector;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = sourceItemSelector?.GetTargetItems(eve);
		List<Item> list2 = targetItemSelector?.GetTargetItems(eve);
		if (list == null || list.Count == 0 || list2 == null || list2.Count == 0)
		{
			return NodeCastResult.Failure;
		}
		Item item = list[0];
		if (item == null)
		{
			return NodeCastResult.Failure;
		}
		ItemSpell originalSpell = item.GetOriginalSpell();
		if (originalSpell == null)
		{
			return NodeCastResult.Failure;
		}
		string description = TextDataPreset.FormatItemSpellDescription(eve.gameState.applicationState, item, out var _, null, isBattleDisplay: true);
		bool flag = false;
		foreach (Item item2 in list2)
		{
			if (item2 != null)
			{
				item2.SetRuntimeTransformTemplate(originalSpell, description, item);
				flag = true;
			}
		}
		if (!flag)
		{
			return NodeCastResult.Failure;
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return IEffectNode.ToList(sourceItemSelector, targetItemSelector);
	}
}
