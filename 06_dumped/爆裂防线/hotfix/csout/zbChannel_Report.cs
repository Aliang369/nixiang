using System.Collections.Generic;
using Newtonsoft.Json;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using zbType;

public class zbChannel_Report
{
	public static void ReportGameData(string type)
	{
		int appChannel = (int)zbLoginUtil.appChannel;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("channelid", appChannel.ToString());
		dictionary.Add("deviceid", zbLoginUtil.deviceId);
		dictionary.Add("device", zbAccountMgr.device_zb);
		dictionary.Add("os", zbAccountMgr.os_zb);
		dictionary.Add("osvers", zbAccountMgr.os_vers_zb);
		dictionary.Add("log_type", type);
		string postDataSign = zbLoginUtil.GetPostDataSign(dictionary);
		dictionary.Add("sign", postDataSign);
		zbHttpMsgHandler.Instance.SendPostRequest(zbNetUrl.reportServer_url, dictionary, PostMsgId.E_ZB_REPORT);
	}

	public static void ReportDataByChannel(EReportType type, string paramsStr, IList<OrderDetail> orderList = null)
	{
		if (zbLoginUtil.appChannel == EChannelType.Ios || zbLoginUtil.appChannel == EChannelType.TapTap)
		{
			TapTapEventReport(type, paramsStr);
		}
		else if (zbLoginUtil.appChannel == EChannelType.Dyunion)
		{
			DouyinEventReport(type, paramsStr, orderList);
		}
		else if (zbLoginUtil.appChannel == EChannelType.AliGames)
		{
			if (type == EReportType.E_ENTER_GAME)
			{
				AligamesReport(type, paramsStr);
			}
		}
		else if (zbLoginUtil.appChannel == EChannelType.Vivo)
		{
			if (type == EReportType.E_ENTER_GAME || type == EReportType.E_LOGIN)
			{
				VivoReport(type, paramsStr);
			}
		}
		else if (zbLoginUtil.appChannel == EChannelType.Oppo && type == EReportType.E_ENTER_GAME)
		{
			OppoReport(type, paramsStr);
		}
	}

	private static void TapTapEventReport(EReportType type, string paramsStr)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)type;
		string eventType = num.ToString();
		string platform = ((object)Application.platform/*cast due to constrained. prefix*/).ToString();
		string eventName = "";
		string amount = "0";
		switch (type)
		{
		case EReportType.E_CUSTOM_EVENT:
			eventName = paramsStr;
			break;
		case EReportType.E_ENTER_GAME:
			eventName = "enter_game";
			break;
		case EReportType.E_LOGIN:
			eventName = "login";
			break;
		case EReportType.E_PURCHASE:
			amount = paramsStr;
			eventName = "purchase";
			break;
		}
		string eventParams = JsonConvert.SerializeObject((object)new
		{
			channel = zbLoginUtil.appChannel,
			platform = platform,
			accountId = zbLoginUtil.loginAccountId,
			guid = zbLoginUtil.guid,
			amount = amount,
			eventName = eventName
		});
		zbSdkHelper.ReportGameData(eventType, eventParams);
	}

	private static void DouyinEventReport(EReportType type, string paramsStr, IList<OrderDetail> orderList = null)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)type;
		string eventType = num.ToString();
		string platform = ((object)Application.platform/*cast due to constrained. prefix*/).ToString();
		string eventName = "";
		string amount = "0";
		object orderList2 = null;
		switch (type)
		{
		case EReportType.E_CUSTOM_EVENT:
			eventName = paramsStr;
			break;
		case EReportType.E_ENTER_GAME:
			eventName = "enter_game";
			break;
		case EReportType.E_LOGIN:
			eventName = "login";
			break;
		case EReportType.E_PURCHASE:
		{
			amount = paramsStr;
			eventName = "purchase";
			if (orderList == null || orderList.Count <= 0)
			{
				break;
			}
			List<object> list = new List<object>();
			for (int i = 0; i < orderList.Count; i++)
			{
				OrderDetail orderDetail = orderList[i];
				if (orderDetail != null)
				{
					list.Add(new
					{
						orderId = (orderDetail.OrderId ?? ""),
						amount = orderDetail.Amount
					});
				}
			}
			orderList2 = list;
			break;
		}
		}
		string eventParams = JsonConvert.SerializeObject((object)new
		{
			channel = zbLoginUtil.appChannel,
			platform = platform,
			accountId = zbLoginUtil.loginAccountId,
			guid = zbLoginUtil.guid,
			amount = amount,
			eventName = eventName,
			orderList = orderList2
		});
		zbSdkHelper.ReportGameData(eventType, eventParams);
	}

	private static void AligamesReport(EReportType type, string paramsStr)
	{
		int num = (int)type;
		string eventType = num.ToString();
		long roleCTime = zbServerData.sGameData.RoleCommonInfo?.CreateTm ?? 0;
		string eventParams = JsonConvert.SerializeObject((object)new
		{
			roleId = zbSelf.Instance.GetRoleId(),
			roleName = zbSelf.Instance.GetRoleName(),
			roleLevel = zbSelf.Instance.GetLevel(),
			roleCTime = roleCTime,
			zoneId = zbSelf.Instance.GetServerId(),
			zoneName = zbSelf.Instance.GetServerId()
		});
		zbSdkHelper.ReportGameData(eventType, eventParams);
	}

	private static void VivoReport(EReportType type, string paramsStr)
	{
		int num = (int)type;
		string eventType = num.ToString();
		string roleId = "";
		string roleName = "";
		string roleLevel = "";
		string zoneId = "";
		string zoneName = "";
		if (type == EReportType.E_ENTER_GAME)
		{
			roleId = zbSelf.Instance.GetRoleId().ToString();
			roleName = zbSelf.Instance.GetRoleName();
			roleLevel = zbSelf.Instance.GetLevel().ToString();
			zoneId = zbSelf.Instance.GetServerId().ToString();
			zoneName = zbSelf.Instance.GetServerId().ToString();
		}
		string eventParams = JsonConvert.SerializeObject((object)new { roleId, roleName, roleLevel, zoneId, zoneName });
		zbSdkHelper.ReportGameData(eventType, eventParams);
	}

	private static void OppoReport(EReportType type, string paramsStr)
	{
		int num = (int)type;
		string eventType = num.ToString();
		string roleId = "";
		string roleName = "";
		string roleLevel = "";
		string realmId = "";
		string realmName = "";
		string chapter = "1";
		if (type == EReportType.E_ENTER_GAME)
		{
			roleId = zbSelf.Instance.GetRoleId().ToString();
			roleName = zbSelf.Instance.GetRoleName();
			roleLevel = zbSelf.Instance.GetLevel().ToString();
			realmId = zbSelf.Instance.GetServerId().ToString();
			realmName = zbSelf.Instance.GetServerId().ToString();
			GameData sGameData = zbServerData.sGameData;
			chapter = ((sGameData.MainQuestInfo != null) ? sGameData.MainQuestInfo.MainLevel : 0).ToString();
		}
		string eventParams = JsonConvert.SerializeObject((object)new { roleId, roleName, roleLevel, realmId, realmName, chapter });
		zbSdkHelper.ReportGameData(eventType, eventParams);
	}
}
