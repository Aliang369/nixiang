public class CurrentStanceMaxValueGenerator : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		return eve.gameState?.stanceManager?.stanceMaxValue;
	}
}
