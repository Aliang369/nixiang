using System.Collections.Generic;

public class MulAllRuneStack : Effect
{
	public ValueGenerator deltaPercentGenerator;

	public ItemTargetSelector targetSelector;

	public HitNFXConfig hitEffect;

	public TransformNFXConfig transformVFXEffect;

	public bool showChangedItems;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = targetSelector?.GetTargetItems(eve);
		if (list == null)
		{
			return NodeCastResult.Failure;
		}
		int delta = ((deltaPercentGenerator == null) ? 1 : deltaPercentGenerator.GenerateValue(eve).Value);
		Item mItem = eve.CasterItem;
		List<Item> list2 = new List<Item>();
		foreach (Item target in list)
		{
			if (target == null)
			{
				continue;
			}
			foreach (Rune rune in target.Runes)
			{
				if (rune.def.stacking && rune.value * delta / 100 != 0)
				{
					list2.Add(target);
					break;
				}
			}
			Effect.EnqueueAction(eve, new ActionAction(delegate
			{
				foreach (Rune rune2 in target.Runes)
				{
					if (rune2.def.stacking)
					{
						int num = rune2.value * delta / 100;
						if (rune2.def.stacking && num != 0)
						{
							transformVFXEffect?.Play(eve, eve.CasterPivot, target);
							Effect.EnqueueAction(eve, new ChangeRuneAction(target, rune2.def, num, mItem));
							hitEffect?.Play(eve, target);
						}
					}
				}
			}));
		}
		if (showChangedItems)
		{
			Effect.EnqueueAction(eve, new NotifyItemModificationsAction(list2));
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { deltaPercentGenerator, targetSelector };
	}
}
