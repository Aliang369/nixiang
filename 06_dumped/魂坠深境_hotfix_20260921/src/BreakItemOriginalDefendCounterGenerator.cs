public class BreakItemOriginalDefendCounterGenerator : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		return eve?.breakItemOriginalDefendCounter;
	}
}
