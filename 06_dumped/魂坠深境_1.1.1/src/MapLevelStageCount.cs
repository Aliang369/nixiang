public class MapLevelStageCount : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		if (eve?.gameState?.contractState == null)
		{
			return null;
		}
		return eve.gameState.contractState.stage;
	}
}
