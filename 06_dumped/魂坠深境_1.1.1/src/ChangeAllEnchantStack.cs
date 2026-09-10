using System.Collections.Generic;

public class ChangeAllEnchantStack : Effect
{
	public ValueGenerator deltaValueGenerator;

	public ItemTargetSelector targetSelector;

	public HitNFXConfig hitEffect;

	public TransformNFXConfig transformVFXEffect;

	public bool negativeOnly;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = targetSelector?.GetTargetItems(eve);
		if (list == null)
		{
			return NodeCastResult.Failure;
		}
		int delta = ((deltaValueGenerator == null) ? 1 : deltaValueGenerator.GenerateValue(eve).Value);
		Item mItem = eve.CasterItem;
		foreach (Item target in list)
		{
			Effect.EnqueueAction(eve, new ActionAction(delegate
			{
				foreach (Enchant enchant in target.Enchants)
				{
					if (!negativeOnly || enchant.def.type == EnchantDef.EnchantType.Negative)
					{
						transformVFXEffect?.Play(eve, eve.CasterPivot, target);
						Effect.EnqueueAction(eve, new ChangeEnchantAction(target, mItem, enchant.def, delta, popup: true, eve.spellInfo));
						hitEffect?.Play(eve, target);
					}
				}
			}));
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { deltaValueGenerator, targetSelector };
	}
}
