using System;
using System.Collections.Generic;

public sealed class RoutineQuestProgressBuffer
{
	public const float MergeDelaySeconds = 0.3f;

	private static readonly float[] RetryDelays = new float[4] { 2f, 5f, 15f, 30f };

	private readonly Dictionary<string, int> pendingProgress = new Dictionary<string, int>();

	private readonly Dictionary<string, int> inFlightProgress = new Dictionary<string, int>();

	private float nextFlushTime;

	private int retryStep;

	private bool retryScheduled;

	private bool requestInFlight;

	public int Generation { get; private set; }

	public int PendingCount => pendingProgress.Count;

	public bool RequestInFlight => requestInFlight;

	public float NextFlushTime => nextFlushTime;

	public IReadOnlyDictionary<string, int> PendingProgress => pendingProgress;

	public IReadOnlyDictionary<string, int> InFlightProgress => inFlightProgress;

	public int Queue(string questGuid, int incomingProgress, int confirmedProgress, int maxProgress, float now)
	{
		if (string.IsNullOrEmpty(questGuid) || incomingProgress <= 0)
		{
			return 0;
		}
		long num = (long)confirmedProgress + (long)GetProgress(pendingProgress, questGuid) + GetProgress(inFlightProgress, questGuid);
		long val = Math.Max(0L, maxProgress - num);
		int num2 = (int)Math.Min(incomingProgress, val);
		if (num2 <= 0)
		{
			return 0;
		}
		pendingProgress[questGuid] = GetProgress(pendingProgress, questGuid) + num2;
		if (retryScheduled)
		{
			retryStep = 0;
			nextFlushTime = now + RetryDelays[0];
		}
		else
		{
			nextFlushTime = now + 0.3f;
		}
		return num2;
	}

	public bool CanFlush(float now)
	{
		if (!requestInFlight && pendingProgress.Count > 0)
		{
			return now >= nextFlushTime;
		}
		return false;
	}

	public Dictionary<string, int> BeginFlush(Func<string, int, int> validateProgress)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		if (requestInFlight)
		{
			return dictionary;
		}
		foreach (KeyValuePair<string, int> item in pendingProgress)
		{
			int num = validateProgress?.Invoke(item.Key, item.Value) ?? item.Value;
			if (num > 0)
			{
				dictionary[item.Key] = num;
			}
		}
		pendingProgress.Clear();
		inFlightProgress.Clear();
		foreach (KeyValuePair<string, int> item2 in dictionary)
		{
			inFlightProgress[item2.Key] = item2.Value;
		}
		requestInFlight = dictionary.Count > 0;
		if (!requestInFlight)
		{
			retryStep = 0;
			retryScheduled = false;
			nextFlushTime = 0f;
		}
		return dictionary;
	}

	public bool CompleteSuccess(int generation, float now)
	{
		if (generation != Generation)
		{
			return false;
		}
		inFlightProgress.Clear();
		requestInFlight = false;
		retryStep = 0;
		retryScheduled = false;
		nextFlushTime = ((pendingProgress.Count > 0) ? (now + 0.3f) : 0f);
		return true;
	}

	public (int step, float delaySeconds)? Fail(int generation, float now)
	{
		if (generation != Generation)
		{
			return null;
		}
		foreach (KeyValuePair<string, int> item in inFlightProgress)
		{
			pendingProgress[item.Key] = GetProgress(pendingProgress, item.Key) + item.Value;
		}
		inFlightProgress.Clear();
		requestInFlight = false;
		retryScheduled = true;
		int num = retryStep;
		float num2 = RetryDelays[Math.Min(num, RetryDelays.Length - 1)];
		retryStep = Math.Min(retryStep + 1, RetryDelays.Length);
		nextFlushTime = now + num2;
		return (num + 1, num2);
	}

	public void Clear()
	{
		pendingProgress.Clear();
		inFlightProgress.Clear();
		nextFlushTime = 0f;
		retryStep = 0;
		retryScheduled = false;
		requestInFlight = false;
		Generation++;
	}

	private static int GetProgress(IReadOnlyDictionary<string, int> progress, string questGuid)
	{
		if (!progress.TryGetValue(questGuid, out var value))
		{
			return 0;
		}
		return value;
	}
}
