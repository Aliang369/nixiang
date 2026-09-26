using Proto.Enums;
using Proto.Types;
using zbType;

public class zbChannel_Login
{
	public static bool bfirstLoginGame;

	public static void OnLoginError()
	{
		zbSdkHelper.SetLoginState(state: false);
	}

	public static void OnSdkUserLogin(bool useTicket = true)
	{
		if (zbChannelUtil.IsNeedUseZbLogin())
		{
			if (!useTicket || !zbLoginUtil.SdkLocalTicketLogin())
			{
				zbUIMgr.Instance.ShowAccount();
			}
		}
		else if (zbChannelUtil.IsNeedShowAcSdkWnd())
		{
			if (!useTicket || !zbLoginUtil.SdkLocalTicketLogin())
			{
				zbUIMgr.Instance.ShowSdkAccount();
			}
		}
		else
		{
			zbSdkHelper.LoginGame();
		}
	}

	public static void OnUserLoginReq()
	{
		string userId = zbSdkHelper.GetUserId();
		string userToken = zbSdkHelper.GetUserToken();
		EChannelType loginChannel = (EChannelType)zbSdkHelper.GetLoginChannel();
		string loginToken = userId + ";" + userToken;
		UserLoginReq userLoginReq = new UserLoginReq();
		userLoginReq.LoginChannel = loginChannel;
		userLoginReq.LoginToken = loginToken;
		userLoginReq.AppChannel = zbLoginUtil.appChannel;
		userLoginReq.Lang = zbLanguage.Instance.GetLanguageType();
		userLoginReq.Platform = zbLoginUtil.platForm;
		userLoginReq.DeviceId = zbLoginUtil.deviceId;
		userLoginReq.Region = zbLanguage.Instance.GetRegionName();
		zbHttpMsgHandler.Instance.User_Login_Req(userLoginReq);
	}

	public static void OnUserLoginAck()
	{
		zbSdkHelper.SetLoginState(state: true);
		if (bfirstLoginGame)
		{
			bfirstLoginGame = false;
			OnUserReadyReq();
		}
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Login_Ack);
		if (string.IsNullOrEmpty(zbGameSave.Instance.GetString(zbLoginUtil.KEY_FIRST_REG)))
		{
			zbChannel_Report.ReportGameData("3");
			zbGameSave.Instance.SetString(zbLoginUtil.KEY_FIRST_REG, "1");
		}
		zbChannel_Report.ReportDataByChannel(EReportType.E_LOGIN, "");
	}

	public static void OnUserReadyReq()
	{
		if (zbLoginUtil.guid == -1 || !zbSdkHelper.GetLoginState())
		{
			OnSdkUserLogin();
		}
		else
		{
			IdentityCheck();
		}
	}

	private static void IdentityCheck()
	{
		if (zbChannelUtil.IsNeedIdentityCheck())
		{
			zbSdkHelper.IdentityCheck(zbLoginUtil.loginAccountId);
		}
		else
		{
			SendReadyReq();
		}
	}

	public static void OnIdentitySuccess()
	{
		SendReadyReq();
	}

	private static void SendReadyReq()
	{
		UserReadyReq userReadyReq = new UserReadyReq();
		userReadyReq.Platform = zbLoginUtil.platForm;
		userReadyReq.AppChannel = zbLoginUtil.appChannel;
		userReadyReq.Guid = zbLoginUtil.guid;
		userReadyReq.Ticket = zbLoginUtil.ticket;
		userReadyReq.Lang = zbLanguage.Instance.GetLanguageType();
		zbHttpMsgHandler.Instance.User_Ready_Req(userReadyReq);
	}

	public static void LoginGameServer()
	{
		zbWsMsgHandler.Instance.Connect();
		zbWsMsgHandler.Instance.ShowTipConnect();
	}

	public static bool IsOpenTSCheck()
	{
		return false;
	}
}
