using System.Collections.Generic;

public class PlayerCurrentInfluence : InfluenceDefGenerator
{
	public override List<InfluenceDef> GenerateInfluenceDefs(GameEvent eve)
	{
		if (eve.gameState?.playerState?.bft.influences == null)
		{
			return null;
		}
		return eve.gameState.playerState.bft.influences.ConvertAll((Influence inf) => inf.def);
	}
}
