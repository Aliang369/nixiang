using System.Collections.Generic;

public class MultiplyAllEnchantStack : Effect
{
	public ValueGenerator percentGenerator;

	public ItemTargetSelector targetSelector;

	public HitNFXConfig hitEffect;

	public TransformNFXConfig transformVFXEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = targetSelector?.GetTargetItems(eve);
		if (list == null)
		{
			return NodeCastResult.Failure;
		}
		int percent = ((percentGenerator == null) ? 1 : percentGenerator.GenerateValue(eve).Value);
		Item mItem = eve.CasterItem;
		foreach (Item target in list)
		{
			Effect.EnqueueAction(eve, new ActionAction(delegate
			{
				foreach (Enchant enchant in target.Enchants)
				{
					if (enchant.def.stacking)
					{
						transformVFXEffect?.Play(eve, eve.CasterPivot, target);
						Effect.EnqueueAction(eve, new ChangeEnchantAction(target, mItem, enchant.def, percent * (int)enchant.value / 100, popup: true, eve.spellInfo));
						hitEffect?.Play(eve, target);
					}
				}
			}));
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { percentGenerator, targetSelector };
	}
}
