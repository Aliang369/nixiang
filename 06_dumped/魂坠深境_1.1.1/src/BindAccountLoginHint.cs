using System;
using ChillyRoom.Services.Accounts.Sdk;
using Generated.ChillyRoomSdkClient;
using UnityEngine;

public class BindAccountLoginHint : UIBase
{
	private const string BindEmailSuccessPlayerPrefsKeyPrefix = "BindAccountLoginHint.BindEmailSuccess.";

	public GameButton backButton;

	public GameButton confirmButton;

	private bool bindEmailSuccessInSession;

	public bool NeedBindAccount
	{
		get
		{
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Invalid comparison between Unknown and I4
			if (!GlobalConfigManager.instance.enableNetwork)
			{
				return false;
			}
			try
			{
				SdkState sdkState = UIBase.applicationState.playerInfoManager.sdkState;
				User user = sdkState?.User;
				if (sdkState == null || user == null)
				{
					return false;
				}
				if (bindEmailSuccessInSession)
				{
					return false;
				}
				return (int)sdkState.CurrentLoginType == 11 && user.Phone.IsNullOrEmpty() && user.Email.IsNullOrEmpty();
			}
			catch (Exception ex)
			{
				Debug.LogWarning(ex.Message);
				return false;
			}
		}
	}

	public void ShowIfNeeded()
	{
		TraceLog.Debug($"BindAccountLoginHint ShowIfNeeded, NeedBindAccount={NeedBindAccount}");
		if (NeedBindAccount)
		{
			SetActiveAndDirtyWithFadeAnim(active: true);
		}
	}

	protected override void Init()
	{
		backButton.onClick.AddListener(_OnBackButtonClicked);
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		SdkEvents.OnBindEmail = (Action<string, string>)Delegate.Remove(SdkEvents.OnBindEmail, new Action<string, string>(_OnBindEmail));
		SdkEvents.OnBindEmail = (Action<string, string>)Delegate.Combine(SdkEvents.OnBindEmail, new Action<string, string>(_OnBindEmail));
	}

	protected override void Dispose()
	{
		SdkEvents.OnBindEmail = (Action<string, string>)Delegate.Remove(SdkEvents.OnBindEmail, new Action<string, string>(_OnBindEmail));
	}

	private void _OnBindEmail(string state, string extra)
	{
		TraceLog.Debug("BindAccountLoginHint OnBindEmail, state=" + state + ", extra=" + extra);
		ThinkingAnalyticsExtends.TrackAccountBinding((state == "0") ? 3 : 2);
		if (!(state != "0"))
		{
			bindEmailSuccessInSession = true;
			UIBase.applicationState.playerInfoManager.UpdateUserProfile();
		}
	}

	private void _OnBackButtonClicked()
	{
		SetActiveAndDirtyWithFadeAnim(active: false);
	}

	private void _OnConfirmButtonClicked()
	{
		SetActiveAndDirtyWithFadeAnim(active: false);
		if (GlobalConfigManager.instance.enableNetwork)
		{
			ThinkingAnalyticsExtends.TrackAccountBinding(1);
			ChillyRoomService.GetSdk().Accounts.BindEmail();
		}
	}
}
