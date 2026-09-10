public class CurrentStanceValueGenerator : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		return eve.gameState?.stanceManager?.currentStance?.value;
	}
}
