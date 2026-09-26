using System;
using Proto.Enums;

public class zbCheatContext
{
	private static bool s_reported;

	public static void ReportCheat(float OriginValue, float ModifyValue, string modifyValueName)
	{
		zbCombatSystem system = zbCombatSystem.Instance;
		if (system != null && !s_reported)
		{
			s_reported = true;
			try
			{
				string text = system.battleInfo?.BattleBaseInfo?.UUID ?? "unknown";
				string text2 = system.GetBattleType().ToString();
				string resVersion = zbSdkHelper.GetResVersion();
				string reportDesc = "UUID:" + text + ", battleFuncType=" + text2 + ", " + modifyValueName + ":" + ModifyValue + ":" + OriginValue;
				zbWsMsgHandler.Instance?.Abnormal_ClientReport_Req(EFuncType.MainQuest, OriginValue, ModifyValue, resVersion, reportDesc);
			}
			catch (Exception)
			{
			}
			SafeInvoke(system.Pause, "Pause");
			SafeInvoke(delegate
			{
				zbAudioMgr.Instance.StopAllAudio();
			}, "StopAllAudio");
			SafeInvoke(delegate
			{
				system.combatRecorder.ClearRecord();
			}, "ClearRecord");
			SafeInvoke(delegate
			{
				zbUIMgr.Instance.DestroyWnd(EWndId.Combat);
			}, "DestroyWnd.Combat");
			SafeInvoke(delegate
			{
				zbUIMgr.Instance.DestroyWnd(EWndId.CombatPause);
			}, "DestroyWnd.CombatPause");
			SafeInvoke(delegate
			{
				zbUIMgr.Instance.ShowMainLevel();
			}, "ShowMainLevel");
			SafeInvoke(delegate
			{
				zbWsMsgHandler.Instance.Game_LoginOut_Req();
			}, "Game_LoginOut_Req");
		}
	}

	private static void SafeInvoke(Action action, string stepName)
	{
		try
		{
			action?.Invoke();
		}
		catch (Exception)
		{
		}
	}
}
