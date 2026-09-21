public class CheckDamageSourceEnchant : CheckNode
{
	public EnchantDef enchantDef;

	public override bool? Check(GameEvent eve)
	{
		return enchantDef != null && eve.damagecommand?.source is Enchant enchant && enchant.def == enchantDef;
	}
}
