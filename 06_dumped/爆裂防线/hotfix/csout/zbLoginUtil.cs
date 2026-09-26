using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Proto.Enums;
using Proto.Messages;
using Proto.Types;
using UnityEngine;
using zbType;

public class zbLoginUtil
{
	public static readonly string KEY_UUID = "device_uuid";

	public static readonly string KEY_TICKET = "ticket";

	public static readonly string KEY_GUID = "guid";

	public static readonly string KEY_LOGIN_ACCOUNTID = "loginaccountid";

	public static readonly string KEY_LOGINCHANNEL = "loginchannel";

	public static readonly string KEY_APPCHANNEL = "appchannel";

	public static readonly string KEY_USERID = "userId";

	public static readonly string KEY_TOKEN = "token";

	public static readonly string KEY_FIRST_START_GAME = "firststartgame";

	public static readonly string KEY_FIRST_REG = "firstreg";

	public static readonly string KEY_LOGIN_SERVERID = "loginserverid";

	public static readonly string KEY_PRIVACY_AGREEMENT_ACCEPTED = "privacy_agreement_accepted";

	public static EChannelType loginChannel;

	public static EChannelType appChannel;

	public static EPlatformType platForm;

	public static string loginAccountId;

	public static int ageRange = -1;

	public static string ticket;

	public static long guid = -1L;

	public static string deviceId;

	public static string appId = "100019";

	public static string appSecret = "778294891a94d10c2eac5d20c07cd08d";

	public static int loginServerId = -1;

	public static int selectServerId = -1;

	public static NoticeListAck noticeListAckData = null;

	public static ServerListAck serverListAckData = null;

	public static void Init()
	{
		platForm = zbNetUrl.playformType;
		if (zbGameSave.Instance.HasKey(KEY_UUID))
		{
			deviceId = zbGameSave.Instance.GetString(KEY_UUID);
		}
		else
		{
			deviceId = GenerateRandomString(32);
			zbGameSave.Instance.SetString(KEY_UUID, deviceId);
		}
		zbSdkHelper.InitParams();
		appChannel = (EChannelType)zbSdkHelper.GetChannel();
		loginChannel = (EChannelType)zbGameSave.Instance.GetInt(KEY_LOGINCHANNEL);
		if (loginChannel == EChannelType.EmptyValue)
		{
			loginChannel = (Application.isMobilePlatform ? EChannelType.Visitor : EChannelType.Zb);
		}
		loginServerId = zbGameSave.Instance.GetInt(KEY_LOGIN_SERVERID, -1);
	}

	public static ServerDetail GetServerDetailById(int serverId)
	{
		if (serverListAckData != null)
		{
			foreach (ServerDetail server in serverListAckData.ServerList)
			{
				if (server.ServerId == serverId)
				{
					return server;
				}
			}
		}
		return null;
	}

	public static void SaveUserLoginAckInfo()
	{
		if (appChannel != EChannelType.ZbRobot)
		{
			zbGameSave.Instance.SetString(KEY_TICKET, ticket);
			zbGameSave.Instance.SetLong(KEY_GUID, guid);
			zbGameSave.Instance.SetInt(KEY_LOGINCHANNEL, (int)loginChannel);
			zbGameSave.Instance.SetString(KEY_LOGIN_ACCOUNTID, loginAccountId);
			string userToken = zbSdkHelper.GetUserToken();
			string userId = zbSdkHelper.GetUserId();
			zbGameSave.Instance.SetString(KEY_USERID, userId);
			zbGameSave.Instance.SetString(KEY_TOKEN, userToken);
		}
	}

	public static string GetPostDataSign(Dictionary<string, string> datas, string appSecret = null)
	{
		if (datas == null || datas.Count == 0)
		{
			return string.Empty;
		}
		List<string> list = datas.Keys.ToList();
		list.Sort();
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		foreach (string item in list)
		{
			if (!flag)
			{
				stringBuilder.Append("#");
			}
			flag = false;
			string text = Uri.EscapeDataString(datas[item]);
			stringBuilder.Append(item + "=" + text);
		}
		if (!string.IsNullOrEmpty(appSecret))
		{
			stringBuilder.Append("#" + appSecret);
		}
		else
		{
			stringBuilder.Append("#" + zbLoginUtil.appSecret);
		}
		return GetMd5Hash(stringBuilder.ToString()).ToString();
	}

	public static string PacketSign(Packet packet, string secret)
	{
		return GetMd5Hash("MsgId=" + packet.MsgId + "#Ver=" + packet.Ver + "#Tm=" + packet.Tm + "#Seq=" + packet.Seq + "#Data=" + packet.Data.ToBase64() + "#" + secret);
	}

	public static string GetMd5Hash(string input)
	{
		using MD5 mD = MD5.Create();
		byte[] bytes = Encoding.UTF8.GetBytes(input);
		byte[] array = mD.ComputeHash(bytes);
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			stringBuilder.Append(b.ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	public static bool SdkLocalTicketLogin()
	{
		ticket = zbGameSave.Instance.GetString(KEY_TICKET);
		guid = zbGameSave.Instance.GetLong(KEY_GUID, 0L);
		loginAccountId = zbGameSave.Instance.GetString(KEY_LOGIN_ACCOUNTID);
		if (!string.IsNullOrEmpty(ticket) && guid != 0L)
		{
			loginChannel = (EChannelType)zbGameSave.Instance.GetInt(KEY_LOGINCHANNEL);
			zbChannel_Login.OnUserLoginAck();
			return true;
		}
		return false;
	}

	public static string GenerateRandomString(int length)
	{
		string text = "abcdefghijklmnopqrstuvwxyz0123456789";
		char[] array = new char[length];
		for (int i = 0; i < length; i++)
		{
			array[i] = text[Rand.Next(text.Length)];
		}
		return new string(array);
	}

	public static void LoginZBAccount()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("appid", appId);
		dictionary.Add("check_code", "");
		dictionary.Add("account", zbAccountMgr.account_zb);
		dictionary.Add("password", zbAccountMgr.pwd_zb);
		dictionary.Add("os_vers", zbAccountMgr.os_vers_zb);
		dictionary.Add("source", zbAccountMgr.source_zb);
		dictionary.Add("device", zbAccountMgr.device_zb);
		dictionary.Add("device_uuid", deviceId);
		string postDataSign = GetPostDataSign(dictionary);
		dictionary.Add("sign", postDataSign);
		zbHttpMsgHandler.Instance.SendPostRequest(zbNetUrl.loginPost_url, dictionary, PostMsgId.E_ZB_MSG_LOGIN);
	}

	public static void NoticListReq(bool open)
	{
		NoticeListReq noticeListReq = new NoticeListReq();
		noticeListReq.AppChannel = appChannel;
		noticeListReq.Platform = platForm;
		noticeListReq.Lang = zbLanguage.Instance.GetLanguageType();
		zbHttpMsgHandler.Instance.Notice_List_Req(noticeListReq, open);
	}

	public static void ErrMsgHandle(ErrorCodeAck ack)
	{
		if (ack != null)
		{
			if (ack.ReqId == 17211 && ack.ErrorCode == EErrorCode.InCd)
			{
				zbUIMgr.Instance.ShowLegionClearCd();
			}
			if (ack.ReqId == 13001)
			{
				_ = ack.ErrorCode;
				_ = 30233;
			}
			if (ack.ErrorCode == EErrorCode.DiamondNotEnough)
			{
				zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), zbUtil.Language("lang.10767"), OnRechrge, zbUtil.Language("lang.10155"), zbUtil.Language("lang.10287"));
			}
			if (ack.ErrorCode == EErrorCode.LegionHaveJoined)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10070"));
			}
			if (ack.ErrorCode == EErrorCode.VoucherNotEnough)
			{
				zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), zbUtil.Language("lang.10258"), OnVoucher, zbUtil.Language("lang.10155"), zbUtil.Language("lang.10287"));
			}
			if (ack.ErrorCode == EErrorCode.RepeatBattleResult || (ack.ErrorCode == EErrorCode.NotInOpenTime && zbUIMgr.Instance.IsShow(EWndId.Combat)))
			{
				zbUIMgr.Instance.DestroyWnd(EWndId.Combat);
				zbUIMgr.Instance.ShowMainLevel();
				zbCombatSystem.Instance.combatRecorder.ClearRecord();
			}
			if (ack.SyncId > 0)
			{
				zbWsMsgHandler.Instance.SyncMsg((Message)ack.SyncId);
			}
			zbUIMgr.Instance.BubbleErrorCode(ack);
			if (ack.ErrorCode == EErrorCode.SdktokenError || ack.ErrorCode == EErrorCode.SdkserverAbnormal)
			{
				zbChannel_Login.OnLoginError();
			}
			else if (ack.ErrorCode == EErrorCode.UserTicketError)
			{
				zbChannel_Login.OnSdkUserLogin(useTicket: false);
			}
			else if (ack.ErrorCode == EErrorCode.AlreadyLoginGame)
			{
				zbWsMsgHandler.Instance.Game_Kick_Req();
			}
			else if (ack.ErrorCode == EErrorCode.NewVersionFound)
			{
				zbUIMgr.Instance.ShowTipNewVersion();
			}
		}
	}

	private static void OnRechrge(bool toRecharge)
	{
		if (toRecharge)
		{
			zbUIMgr.Instance.ShowVip(2, 1);
		}
	}

	private static void OnVoucher(bool toVoucher)
	{
		if (toVoucher)
		{
			zbUIMgr.Instance.ShowVip(2);
		}
	}
}
