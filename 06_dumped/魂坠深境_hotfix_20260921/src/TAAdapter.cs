using System.Collections.Generic;
using Newtonsoft.Json;
using ThinkingAnalytics;
using UnityEngine;

public static class TAAdapter
{
	private static bool CanReport
	{
		get
		{
			if (Application.isEditor)
			{
				return GlobalConfigManager.instance.enableEditorTA;
			}
			return true;
		}
	}

	private static bool DebugLog => GlobalConfigManager.instance.debugLogTA;

	public static void Init()
	{
		ThinkingAnalyticsAPI.StartThinkingAnalytics((Token[])null);
		if (CanReport)
		{
			ThinkingAnalyticsAPI.EnableAutoTrack((AUTO_TRACK_EVENTS)35, (Dictionary<string, object>)null, "");
		}
	}

	public static void Track(string eventName, Dictionary<string, object> properties)
	{
		if (DebugLog)
		{
			LogTrackEvent(eventName, properties);
		}
		if (CanReport)
		{
			ThinkingAnalyticsAPI.Track(eventName, properties, "");
		}
	}

	public static void UserSet(Dictionary<string, object> properties)
	{
		if (DebugLog)
		{
			LogTrackEvent("user_set", properties);
		}
		if (CanReport)
		{
			ThinkingAnalyticsAPI.UserSet(properties, "");
		}
	}

	public static void Login(string playerId)
	{
		if (CanReport)
		{
			ThinkingAnalyticsAPI.Login(playerId, Application.identifier);
		}
	}

	public static string GetDeviceId()
	{
		return ThinkingAnalyticsAPI.GetDeviceId();
	}

	public static string GetDistinctId()
	{
		return ThinkingAnalyticsAPI.GetDistinctId("");
	}

	public static void ReportException(string title, string message, string stackTrace)
	{
		if (BuglyAgent.IsInitialized && CanReport)
		{
			BuglyAgent.ReportException(title, message, stackTrace);
		}
	}

	private static void LogTrackEvent(string eventName, Dictionary<string, object> properties)
	{
		Debug.Log(string.Concat("TA事件【" + eventName + "】", JsonConvert.SerializeObject(properties, new JsonSerializerSettings
		{
			Formatting = Formatting.None
		})));
	}
}
