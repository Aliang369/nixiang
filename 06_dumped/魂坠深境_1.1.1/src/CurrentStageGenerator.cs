public class CurrentStageGenerator : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		return eve.gameState?.contractState?.stage ?? 1;
	}
}
