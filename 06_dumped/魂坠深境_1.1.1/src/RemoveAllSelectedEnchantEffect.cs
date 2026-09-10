using System.Collections.Generic;
using System.Linq;

public sealed class RemoveAllSelectedEnchantEffect : Effect
{
	public ItemTargetSelector targetSelector;

	public EnchantDefGenerator enchantGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item item = targetSelector?.GetSingleItem(eve);
		EnchantDef enchantDef = enchantGenerator?.GenerateEnchants(eve)?.FirstOrDefault();
		Enchant enchant = item?.Enchants?.FirstOrDefault((Enchant candidate) => candidate.def == enchantDef);
		if (item == null || enchant == null || (int)enchant.value <= 0)
		{
			return NodeCastResult.Success;
		}
		Effect.EnqueueAction(eve, new ChangeEnchantAction(item, eve.CasterItem, enchantDef, -(int)enchant.value, popup: true, eve.spellInfo));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector, enchantGenerator };
	}
}
