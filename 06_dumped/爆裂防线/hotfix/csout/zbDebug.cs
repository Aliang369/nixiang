using System.Diagnostics;
using UnityEngine;

public static class zbDebug
{
	[Conditional("UNITY_EDITOR")]
	public static void Log(object msg)
	{
		Debug.Log(msg);
	}

	[Conditional("UNITY_EDITOR")]
	public static void LogWarning(object msg)
	{
		Debug.LogWarning(msg);
	}

	[Conditional("UNITY_EDITOR")]
	public static void LogError(object msg)
	{
		Debug.LogError(msg);
	}
}
