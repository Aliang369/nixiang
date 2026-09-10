public class mEnchantValueGenerator : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		return eve.CasterEnchant?.value ?? ((ProtectedInt)0);
	}
}
