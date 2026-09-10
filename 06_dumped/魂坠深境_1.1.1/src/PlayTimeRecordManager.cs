using System.Diagnostics;

public class PlayTimeRecordManager : GlobalGameManager, ISaveCallback
{
	[SaveField(null, null)]
	public long accumulativeTime;

	[SaveField(null, null)]
	public bool inited;

	private Stopwatch stopwatch;

	public void BeforeSave()
	{
		accumulativeTime = GetPlayTime();
		if (stopwatch != null)
		{
			stopwatch.Restart();
		}
	}

	public void AfterLoad()
	{
	}

	public override void Init(ApplicationStateInfo applicationState)
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

	public long GetPlayTime()
	{
		return accumulativeTime + (stopwatch?.ElapsedTicks ?? 0);
	}
}
