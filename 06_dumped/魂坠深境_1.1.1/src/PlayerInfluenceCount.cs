public class PlayerInfluenceCount : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		if (eve.gameState?.playerState?.bft.influences == null)
		{
			return null;
		}
		return eve.gameState.playerState.bft.influences.Count;
	}
}
