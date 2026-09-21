public class PlayerItemCastTimesInThisRound : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		return eve.gameState?.recordManager?.playerCastedItemsThisRound?.Count;
	}
}
