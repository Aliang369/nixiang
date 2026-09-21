using System.Collections.Generic;
using System.Linq;

public sealed class DancerCharmNegativeEnchantCountGenerator : ValueGenerator
{
	public ItemTargetSelector targetSelector;

	public override int? GenerateValue(GameEvent eve)
	{
		return DancerCharmReplaceNegativeEnchantsEffect.ResolveTargets(eve, targetSelector)?.Sum((Item item) => (item?.Enchants?.Count((Enchant enchant) => enchant != null && enchant.def?.type == EnchantDef.EnchantType.Negative)).GetValueOrDefault());
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector };
	}
}
