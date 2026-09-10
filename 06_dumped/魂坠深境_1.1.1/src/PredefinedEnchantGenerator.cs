using System.Collections.Generic;

public class PredefinedEnchantGenerator : EnchantDefGenerator
{
	public EnchantDef enchantDef;

	public override List<EnchantDef> GenerateEnchants(GameEvent eve)
	{
		if (enchantDef == null)
		{
			return null;
		}
		return new List<EnchantDef> { enchantDef };
	}
}
