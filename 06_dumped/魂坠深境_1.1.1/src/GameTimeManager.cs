using System.Diagnostics;

public class GameTimeManager : InGameManager, ISaveCallback
{
	[SaveField(null, null)]
	public long accumulativeTime;

	[SaveField(null, null)]
	public bool inited;

	private Stopwatch stopwatch;

	public void BeforeSave()
	{
		accumulativeTime = GetTime();
		if (stopwatch != null)
		{
			stopwatch.Restart();
		}
	}

	public void AfterLoad()
	{
	}

	public override void Init(GameStateInfo gameState)
	{
		stopwatch = new Stopwatch();
		stopwatch.Start();
		if (!inited)
		{
			accumulativeTime = 0L;
			inited = true;
		}
	}

	public void Stop()
	{
		stopwatch.Stop();
	}

	public long GetTime()
	{
		return accumulativeTime + (stopwatch?.ElapsedTicks ?? 0);
	}
}
