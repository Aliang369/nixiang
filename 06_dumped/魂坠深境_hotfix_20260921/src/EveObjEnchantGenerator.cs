using System.Collections.Generic;

public class EveObjEnchantGenerator : EnchantDefGenerator
{
	public GameEvent.EveObjType type;

	public override List<EnchantDef> GenerateEnchants(GameEvent eve)
	{
		Enchant eveObj = eve.GetEveObj<Enchant>(type);
		if (eveObj == null)
		{
			return null;
		}
		return new List<EnchantDef> { eveObj.def };
	}
}
