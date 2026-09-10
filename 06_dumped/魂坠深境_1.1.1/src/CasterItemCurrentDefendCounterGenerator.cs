public class CasterItemCurrentDefendCounterGenerator : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		return eve?.CasterItem?.GetCurrentDefendCounter();
	}
}
