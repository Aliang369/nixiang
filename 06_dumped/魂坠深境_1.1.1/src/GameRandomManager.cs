public class GameRandomManager : InGameManager
{
	[SaveField(null, null)]
	private PredictableRandom random;

	public override void Init(GameStateInfo gameState)
	{
		if (random != null)
		{
			return;
		}
		NewGameConfigData newGameConfigData = gameState.startBootStrapData.newGameConfigData;
		if (newGameConfigData != null)
		{
			NewGameConfigData.GeneratePrepareConfig startGameInfo = newGameConfigData.startGameInfo;
			if (startGameInfo != null && startGameInfo.gameRandomSeed.HasValue)
			{
				random = new PredictableRandom(gameState.startBootStrapData.newGameConfigData.startGameInfo.gameRandomSeed.Value);
				return;
			}
		}
		random = new PredictableRandom();
	}

	public int Next()
	{
		return random.Next();
	}

	public int Next(int min, int max)
	{
		return random.Next(min, max);
	}

	public double NextDouble()
	{
		return random.NextDouble();
	}

	public double NextDouble(double min, double max)
	{
		return random.NextDouble(min, max);
	}

	public float NextFloat()
	{
		return random.NextFloat();
	}

	public float NextFloat(float max)
	{
		return random.NextFloat(max);
	}

	public float NextFloat(float min, float max)
	{
		return random.NextFloat(min, max);
	}
}
