using System;

public class GameTimeInSeconds : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		if (eve.gameState?.gameTimer == null)
		{
			return null;
		}
		return (int)new TimeSpan(eve.gameState.gameTimer.GetTime()).TotalSeconds;
	}
}
