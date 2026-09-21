using System.Collections.Generic;
using System.Linq;
using GameRandom;

public class CurrentLocationDisorderInfluenceDefGenerator : InfluenceDefGenerator
{
	public override List<InfluenceDef> GenerateInfluenceDefs(GameEvent eve)
	{
		PathLocation pathLocation = eve?.gameState?.dungeonMapManager?.currentLocation;
		InfluenceDef influenceDef = pathLocation?.disorderCriticalInfluence;
		if (influenceDef == null && pathLocation != null)
		{
			List<InfluenceDef> list = ScriptableObjectSingleton<GameStatisticData>.instance?.nightmareTable?.Columns?.OfType<InfluenceDef>().ToList();
			if (!list.IsNullOrEmpty())
			{
				influenceDef = (pathLocation.disorderCriticalInfluence = list.SelectRandom(eve.gameState.randomManager.NextDouble));
			}
		}
		if (!(influenceDef == null))
		{
			return new List<InfluenceDef> { influenceDef };
		}
		return null;
	}
}
