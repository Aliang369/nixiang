public class mCounterValueGenerator : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		return eve.CasterItem?.counter ?? ((ProtectedInt)0);
	}
}
