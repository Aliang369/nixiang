using System;
using System.Linq;
using System.Threading.Tasks;
using ChillyRoom.RealName.Utils;
using ChillyRoom.Services.ContainerOfSoul;
using Generated.ChillyRoomSdkClient;
using UnityEngine;

public class ForceUpdateManager
{
	private const int CheckIntervalSeconds = 300;

	private static readonly TimeSpan MaintenancePreviewTime = TimeSpan.FromMinutes(30.0);

	private ForceUpdatePolicyResult cachedPolicy;

	private bool inited;

	private bool checkLoopStarted;

	private bool maintenancePreviewShown;

	private TextDataPreset.ForceUpdateManagerText textData => ScriptableObjectSingleton<TextDataPreset>.instance.forceUpdateManagerText;

	public void Init()
	{
		if (!inited)
		{
			inited = true;
		}
	}

	public void Dispose()
	{
		if (inited)
		{
			checkLoopStarted = false;
			inited = false;
		}
	}

	public async void StartCheckLoop()
	{
		if (inited && !checkLoopStarted && CSCommon.enableNetwork)
		{
			checkLoopStarted = true;
			while (inited)
			{
				await RefreshPolicy();
				await Task.Delay(300000);
			}
		}
	}

	private void CheckShouldBlock()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if ((int)CSCommon.channelType == 0)
		{
			return;
		}
		if (cachedPolicy == null || cachedPolicy.TargetVersion.IsNullOrEmpty())
		{
			ShowUnavailableVersionInfoView();
			return;
		}
		DateTimeOffset currentTime = ServerTimePoller.Instance.GetCurrentTime();
		if (ShouldBlock(cachedPolicy, GetClientVersion(), currentTime))
		{
			ShowBlockView(cachedPolicy, currentTime);
		}
		else if (ShouldShowMaintenancePreview(cachedPolicy, currentTime))
		{
			ShowMaintenancePreviewView(cachedPolicy);
		}
	}

	private static bool ShouldBlock(ForceUpdatePolicyResult policy, string clientVersion, DateTimeOffset serverTime)
	{
		if (policy == null)
		{
			return false;
		}
		if (IsInMaintenanceWindow(policy, serverTime))
		{
			return true;
		}
		if (!IsAfterAvailableAt(policy, serverTime))
		{
			return false;
		}
		if (string.IsNullOrWhiteSpace(policy.TargetVersion))
		{
			return false;
		}
		return CompareVersion(clientVersion, policy.TargetVersion) < 0;
	}

	private static bool IsInMaintenanceWindow(ForceUpdatePolicyResult policy, DateTimeOffset serverTime)
	{
		if (policy.EffectiveAtUtc != default(DateTime) && policy.AvailableAtUtc != default(DateTime) && serverTime >= policy.EffectiveAtUtc)
		{
			return serverTime < policy.AvailableAtUtc;
		}
		return false;
	}

	private bool ShouldShowMaintenancePreview(ForceUpdatePolicyResult policy, DateTimeOffset serverTime)
	{
		if (maintenancePreviewShown || policy == null || policy.EffectiveAtUtc == default(DateTime))
		{
			return false;
		}
		DateTimeOffset dateTimeOffset = new DateTimeOffset(policy.EffectiveAtUtc).Add(-MaintenancePreviewTime);
		if (serverTime >= dateTimeOffset)
		{
			return serverTime < policy.EffectiveAtUtc;
		}
		return false;
	}

	private static bool IsAfterAvailableAt(ForceUpdatePolicyResult policy, DateTimeOffset serverTime)
	{
		if (policy.AvailableAtUtc != default(DateTime))
		{
			return serverTime >= policy.AvailableAtUtc;
		}
		if (policy.EffectiveAtUtc != default(DateTime))
		{
			return serverTime >= policy.EffectiveAtUtc;
		}
		return true;
	}

	private static string GetClientVersion()
	{
		return AppVersionDisplay.GetDisplayVersion();
	}

	private static int CompareVersion(string left, string right)
	{
		int[] array = ParseVersionParts(left);
		int[] array2 = ParseVersionParts(right);
		int num = Math.Max(array.Length, array2.Length);
		for (int i = 0; i < num; i++)
		{
			int num2 = ((i < array.Length) ? array[i] : 0);
			int num3 = ((i < array2.Length) ? array2[i] : 0);
			if (num2 != num3)
			{
				return num2.CompareTo(num3);
			}
		}
		return 0;
	}

	private static int[] ParseVersionParts(string version)
	{
		if (string.IsNullOrWhiteSpace(version))
		{
			return Array.Empty<int>();
		}
		return (from part in version.Split('.')
			select int.TryParse(part, out var result) ? result : 0).ToArray();
	}

	private void ShowBlockView(ForceUpdatePolicyResult policy, DateTimeOffset serverTime)
	{
		Bootstrap.instance.bootstrapExtra.blockPanel.Show(GetBlockTitle(policy, serverTime), null, Application.Quit, null, textData.term_exit);
	}

	private void ShowUnavailableVersionInfoView()
	{
		Bootstrap.instance.bootstrapExtra.blockPanel.Show(textData.term_unavailable_version_info, null, Application.Quit, null, textData.term_exit);
	}

	private void ShowMaintenancePreviewView(ForceUpdatePolicyResult policy)
	{
		maintenancePreviewShown = true;
		Bootstrap.instance.bootstrapExtra.blockPanel.Show(string.Format(textData.term_maintenance_update_preview, FormatLocalTime(policy.EffectiveAtUtc)), null, Bootstrap.instance.bootstrapExtra.blockPanel.Hide, null, textData.term_close);
	}

	private string GetBlockTitle(ForceUpdatePolicyResult policy, DateTimeOffset serverTime)
	{
		if (IsInMaintenanceWindow(policy, serverTime))
		{
			return string.Format(textData.term_maintenance_update_until, FormatLocalTime(policy.AvailableAtUtc));
		}
		return textData.term_found_new_version_restart_update;
	}

	private static string FormatLocalTime(DateTimeOffset time)
	{
		return time.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
	}

	private async Task RefreshPolicy()
	{
		if (CSCommon.enableNetwork)
		{
			try
			{
				TraceLog.Debug("[ForceUpdateManager] RefreshPolicy()");
				cachedPolicy = null;
				cachedPolicy = await (ChillyRoomService.GetSdk()?.ForceUpdate.FetchForceUpdatePolicyAsync());
			}
			catch (Exception arg)
			{
				TraceLog.Error($"[ForceUpdateManager] RefreshPolicy() failed, keep cached policy. Exception: {arg}");
			}
			CheckShouldBlock();
		}
	}
}
