using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class zbPerfLog
{
	private static bool s_enabled = false;

	private const string kTag = "[zbPerf]";

	private static readonly Dictionary<string, long> s_counters = new Dictionary<string, long>(StringComparer.Ordinal);

	private const float kFlushInterval = 5f;

	private static float s_accumTime;

	private static int s_frameCount;

	private static float s_frameTimeMin = float.MaxValue;

	private static float s_frameTimeMax;

	private static float s_frameTimeSum;

	public static bool Enabled => s_enabled;

	public static Func<string> SnapshotProvider { get; set; }

	public static void SetEnabled(bool value)
	{
		s_enabled = value;
	}

	public static void Log(string msg)
	{
		if (s_enabled)
		{
			Debug.Log((object)("[zbPerf]" + msg));
		}
	}

	public static void LogWarning(string msg)
	{
		if (s_enabled)
		{
			Debug.LogWarning((object)("[zbPerf]" + msg));
		}
	}

	public static void LogError(string msg)
	{
		Debug.LogError((object)("[zbPerf]" + msg));
	}

	public static void Inc(string key, long delta = 1L)
	{
		if (s_enabled)
		{
			s_counters.TryGetValue(key, out var value);
			s_counters[key] = value + delta;
		}
	}

	public static void Set(string key, long value)
	{
		if (s_enabled)
		{
			s_counters[key] = value;
		}
	}

	public static void TickFrame()
	{
		if (!s_enabled)
		{
			return;
		}
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		s_accumTime += unscaledDeltaTime;
		s_frameCount++;
		s_frameTimeSum += unscaledDeltaTime;
		if (unscaledDeltaTime < s_frameTimeMin)
		{
			s_frameTimeMin = unscaledDeltaTime;
		}
		if (unscaledDeltaTime > s_frameTimeMax)
		{
			s_frameTimeMax = unscaledDeltaTime;
		}
		if (s_accumTime < 5f)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder(256);
		stringBuilder.Append("周期汇总(").Append(s_accumTime.ToString("0.0")).Append("s): ")
			.Append("fps=")
			.Append(((float)s_frameCount / s_accumTime).ToString("0.0"))
			.Append(" 帧时间ms avg=")
			.Append((s_frameTimeSum / (float)s_frameCount * 1000f).ToString("0.0"))
			.Append(" min=")
			.Append((s_frameTimeMin * 1000f).ToString("0.0"))
			.Append(" max=")
			.Append((s_frameTimeMax * 1000f).ToString("0.0"));
		if (s_counters.Count > 0)
		{
			stringBuilder.Append(" |");
			foreach (KeyValuePair<string, long> s_counter in s_counters)
			{
				stringBuilder.Append(' ').Append(s_counter.Key).Append('=')
					.Append(s_counter.Value);
			}
		}
		string value = ((SnapshotProvider != null) ? SnapshotProvider() : null);
		if (!string.IsNullOrEmpty(value))
		{
			stringBuilder.Append(" | ").Append(value);
		}
		Debug.Log((object)("[zbPerf]" + stringBuilder.ToString()));
		s_accumTime = 0f;
		s_frameCount = 0;
		s_frameTimeSum = 0f;
		s_frameTimeMin = float.MaxValue;
		s_frameTimeMax = 0f;
		s_counters.Clear();
	}

	public static void Reset()
	{
		s_counters.Clear();
		s_accumTime = 0f;
		s_frameCount = 0;
		s_frameTimeSum = 0f;
		s_frameTimeMin = float.MaxValue;
		s_frameTimeMax = 0f;
	}
}
