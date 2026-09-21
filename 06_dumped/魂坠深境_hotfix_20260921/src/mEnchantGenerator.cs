using System.Collections.Generic;

public class mEnchantGenerator : EnchantDefGenerator
{
	public override List<EnchantDef> GenerateEnchants(GameEvent eve)
	{
		Enchant casterEnchant = eve.CasterEnchant;
		if (casterEnchant == null)
		{
			return null;
		}
		return new List<EnchantDef> { casterEnchant.def };
	}
}
