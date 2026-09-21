using CoreKit.Config;
using Generated.ChillyRoomSdkClient;
using UnityEngine;

public class ChillyRoomBuglyIntegration
{
	public static void Init()
	{
		BuglyAgent.ConfigDebugMode(Debug.isDebugBuild);
		string name = SdkConfigManager.gameConfig.Distro.Name;
		string displayVersion = AppVersionDisplay.GetDisplayVersion();
		string text = TryGetUserID();
		if (TraceLog.EnabledLog)
		{
			TraceLog.Debug("[ChillyRoomBuglyIntegration] channel=" + name + ", version=" + displayVersion + ", user=" + text);
		}
		BuglyAgent.ConfigDefault(name, displayVersion, text, 0L);
		BuglyAgent.InitWithAppId("34cd2f94e0");
		BuglyAgent.EnableExceptionHandler();
		BuglyAgent.SetDeviceModel(SystemInfo.deviceModel);
	}

	public static void OnLogin(string playerId)
	{
		if (BuglyAgent.IsInitialized)
		{
			if (TraceLog.EnabledLog)
			{
				TraceLog.Debug("[ChillyRoomBuglyIntegration] playerId=" + playerId);
			}
			BuglyAgent.SetUserId(playerId);
		}
	}

	private static string TryGetUserID()
	{
		try
		{
			return ChillyRoomService.GetSdk().Accounts.StateManager.State.User.PlayerId.ToString();
		}
		catch
		{
			return "unknown";
		}
	}
}
