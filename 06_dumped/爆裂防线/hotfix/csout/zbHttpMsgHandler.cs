using System;
using System.Collections.Generic;
using System.IO;
using Best.HTTP;
using Best.HTTP.Request.Upload;
using Google.Protobuf;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Messages;
using Proto.Types;
using UnityEngine;
using zbType;

public class zbHttpMsgHandler : MonoBehaviour
{
	private bool m_openNoticeWnd;

	public static zbHttpMsgHandler Instance;

	public int m_SequenceId = 1;

	private Dictionary<Message, Action<ByteString>> m_messageCallbacks = new Dictionary<Message, Action<ByteString>>();

	private PostMsgId m_lastPostMsgId;

	private void InitMsgHandle()
	{
		RegisterCallback(Message.ErrorAck, Error_Ack);
		RegisterCallback(Message.UserVisitorInitAck, User_VisitorInit_Ack);
		RegisterCallback(Message.UserVisitorBindAck, User_VisitorBind_Ack);
		RegisterCallback(Message.NoticeListAck, Notice_List_Ack);
		RegisterCallback(Message.UserLoginAck, User_Login_Ack);
		RegisterCallback(Message.UserReadyAck, User_Ready_Ack);
		RegisterCallback(Message.ServerListAck, Server_List_Ack);
		RegisterCallback(Message.UserSetLangAck, User_SetLang_Ack);
	}

	public void User_VisitorInit_Req()
	{
		Instance.PackMsg(null, Message.UserVisitorInitReq);
	}

	public void User_VisitorInit_Ack(ByteString response)
	{
		UserVisitorInitAck userVisitorInitAck = UserVisitorInitAck.Parser.ParseFrom(response);
		if (userVisitorInitAck != null)
		{
			zbSdkHelper.SetLoginUserData(userVisitorInitAck.AccountId, userVisitorInitAck.Token, 60000);
		}
	}

	public void User_VisitorBind_Req(UserVisitorBindReq req)
	{
		Instance.PackMsg((IMessage)(object)req, Message.UserVisitorBindReq);
	}

	public void User_VisitorBind_Ack(ByteString response)
	{
		UserVisitorBindAck userVisitorBindAck = UserVisitorBindAck.Parser.ParseFrom(response);
		if (userVisitorBindAck != null)
		{
			zbLoginUtil.loginChannel = userVisitorBindAck.LoginChannel;
			zbLoginUtil.guid = userVisitorBindAck.Guid;
			zbLoginUtil.ticket = userVisitorBindAck.Ticket;
			zbLoginUtil.SaveUserLoginAckInfo();
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10427"));
			zbGameEventMgr.Instance.SendEvent(EGameEvent.VisitorBind_Ack, userVisitorBindAck);
		}
	}

	public void User_Login_Req(UserLoginReq req)
	{
		Instance.PackMsg((IMessage)(object)req, Message.UserLoginReq);
	}

	public void User_Login_Ack(ByteString response)
	{
		UserLoginAck userLoginAck = UserLoginAck.Parser.ParseFrom(response);
		if (userLoginAck != null)
		{
			zbLoginUtil.loginChannel = userLoginAck.LoginChannel;
			zbLoginUtil.guid = userLoginAck.Guid;
			zbLoginUtil.ticket = userLoginAck.Ticket;
			zbLoginUtil.loginAccountId = userLoginAck.LoginAccountID;
			zbLoginUtil.SaveUserLoginAckInfo();
			zbChannel_Login.OnUserLoginAck();
		}
	}

	public void User_Ready_Req(UserReadyReq req)
	{
		Instance.PackMsg((IMessage)(object)req, Message.UserReadyReq);
	}

	public void User_Ready_Ack(ByteString response)
	{
		UserReadyAck userReadyAck = UserReadyAck.Parser.ParseFrom(response);
		if (userReadyAck != null)
		{
			zbServerData.sGameEnv = userReadyAck.GameEnv;
			if (zbLoginUtil.selectServerId == -1)
			{
				zbLoginUtil.selectServerId = userReadyAck.ServerId;
			}
			zbLoginUtil.loginServerId = zbLoginUtil.selectServerId;
			zbGameSave.Instance.SetInt(zbLoginUtil.KEY_LOGIN_SERVERID, zbLoginUtil.loginServerId);
			zbWsMsgHandler.Instance.m_wsUrl = "ws://" + userReadyAck.Gate.Host + ":" + userReadyAck.Gate.Port + "/gate";
			if (!string.IsNullOrEmpty(userReadyAck.Gate.Url))
			{
				zbWsMsgHandler.Instance.m_wsUrl = userReadyAck.Gate.Url;
			}
			zbChannel_Login.LoginGameServer();
		}
	}

	public void Notice_List_Req(NoticeListReq req, bool open = false)
	{
		m_openNoticeWnd = open;
		Instance.PackMsg((IMessage)(object)req, Message.NoticeListReq);
	}

	public void Notice_List_Ack(ByteString response)
	{
		NoticeListAck noticeListAck = NoticeListAck.Parser.ParseFrom(response);
		if (noticeListAck == null)
		{
			return;
		}
		foreach (NoticeDetail notice in noticeListAck.NoticeList)
		{
			if (notice != null)
			{
				zbNewNoticeHandler.AddNewNotice(notice.Id);
			}
		}
		zbLoginUtil.noticeListAckData = noticeListAck;
		if (m_openNoticeWnd)
		{
			zbUIMgr.Instance.ShowHub();
		}
	}

	public void Server_List_Req()
	{
		ServerListReq serverListReq = new ServerListReq();
		serverListReq.AppChannel = zbLoginUtil.appChannel;
		serverListReq.Platform = zbLoginUtil.platForm;
		serverListReq.Guid = zbLoginUtil.guid;
		Instance.PackMsg((IMessage)(object)serverListReq, Message.ServerListReq);
	}

	public void Server_List_Ack(ByteString response)
	{
		ServerListAck serverListAck = ServerListAck.Parser.ParseFrom(response);
		if (serverListAck != null)
		{
			zbLoginUtil.serverListAckData = serverListAck;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Server_List_Ack, serverListAck);
		}
	}

	private void Error_Ack(ByteString response)
	{
		ErrorCodeAck errorCodeAck = ErrorCodeAck.Parser.ParseFrom(response);
		if (errorCodeAck != null)
		{
			zbLoginUtil.ErrMsgHandle(errorCodeAck);
		}
	}

	public void User_SetLang_Req(ELanguageType eLanguage)
	{
		UserSetLangReq userSetLangReq = new UserSetLangReq();
		userSetLangReq.Lang = eLanguage;
		Instance.PackMsg((IMessage)(object)userSetLangReq, Message.UserSetLangReq);
	}

	public void User_SetLang_Ack(ByteString response)
	{
		UserSetLangAck userSetLangAck = UserSetLangAck.Parser.ParseFrom(response);
		if (userSetLangAck != null)
		{
			zbGameEventMgr.Instance.SendEvent(EGameEvent.User_SetLang_Ack, userSetLangAck);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Server_List_Ack);
		}
	}

	private void Awake()
	{
		if ((Object)(object)Instance == (Object)null)
		{
			Instance = this;
			Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		}
		else
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void OnEnable()
	{
		m_SequenceId = 1;
		InitMsgHandle();
	}

	public void RegisterCallback(Message msg, Action<ByteString> callback)
	{
		if (!m_messageCallbacks.ContainsKey(msg))
		{
			m_messageCallbacks.Add(msg, callback);
		}
	}

	public void UnregisterCallback(Message msg)
	{
		if (m_messageCallbacks.ContainsKey(msg))
		{
			m_messageCallbacks.Remove(msg);
		}
	}

	public int GetSequenceId()
	{
		m_SequenceId++;
		return m_SequenceId;
	}

	public void PackMsg(IMessage req, Message messageId)
	{
		if (!string.IsNullOrEmpty(zbNetUrl.apiServer_url))
		{
			Packet packet = new Packet();
			packet.MsgId = (int)messageId;
			packet.Seq = GetSequenceId();
			packet.Ver = 20260921;
			packet.Tm = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			if (req == null)
			{
				packet.Data = ByteString.Empty;
			}
			else
			{
				packet.Data = MessageExtensions.ToByteString(req);
			}
			packet.Sign = zbLoginUtil.PacketSign(packet, zbLoginUtil.appSecret);
			SendPacketPostRequest(zbNetUrl.apiServer_url, packet);
		}
	}

	private void SendPacketPostRequest(string url, Packet packet)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		HTTPRequest obj = HTTPRequest.CreatePost(url, new OnRequestFinishedDelegate(HandlePacketResponse));
		obj.UploadSettings.UploadStream = new MemoryStream(MessageExtensions.ToByteArray((IMessage)(object)packet));
		obj.Send();
		ShowTipConnect();
	}

	public void SendPostRequest(string url, Dictionary<string, string> postData, PostMsgId id)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		m_lastPostMsgId = id;
		HTTPRequest obj = HTTPRequest.CreatePost(url, new OnRequestFinishedDelegate(HandlePostResponse));
		obj.UploadSettings.UploadStream = (Stream)(object)new JSonDataStream<Dictionary<string, string>>(postData);
		obj.Send();
		ShowTipConnect();
	}

	private void HandlePacketResponse(HTTPRequest req, HTTPResponse resp)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		HideTipConnect();
		HTTPRequestStates state = req.State;
		if ((int)state != 3)
		{
			if ((int)state == 4)
			{
				zbUIMgr.Instance.BubbleInfo("Response Error:" + resp.Message);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo("Response state: " + ((object)req.State/*cast due to constrained. prefix*/).ToString());
			}
		}
		else if (resp.IsSuccess)
		{
			string dataAsText = resp.DataAsText;
			Packet packet = JsonParser.Default.Parse<Packet>(dataAsText);
			if (packet != null && m_messageCallbacks.TryGetValue((Message)packet.MsgId, out var value))
			{
				value?.Invoke(packet.Data);
			}
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo($"PostResponse Status:{resp.StatusCode}");
		}
	}

	private void HandlePostResponse(HTTPRequest req, HTTPResponse resp)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		HideTipConnect();
		HTTPRequestStates state = req.State;
		if ((int)state != 3)
		{
			if ((int)state == 4)
			{
				zbUIMgr.Instance.BubbleInfo("PostResponse Error:" + resp.Message);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo("PostResponse state: " + ((object)req.State/*cast due to constrained. prefix*/).ToString());
			}
		}
		else if (resp.IsSuccess)
		{
			JObject val = JObject.Parse(resp.DataAsText);
			if (val == null)
			{
				return;
			}
			JToken obj = val["status"];
			int num = ((obj != null) ? obj.ToObject<int>() : (-1));
			string text = ((object)val["message"])?.ToString() ?? "";
			if (num == 0)
			{
				string text2 = ((object)val["data"])?.ToString() ?? "";
				if (string.IsNullOrEmpty(text2))
				{
					return;
				}
				JObject val2 = JObject.Parse(text2);
				if (val2 != null)
				{
					if (m_lastPostMsgId == PostMsgId.E_ZB_MSG_VISITOR)
					{
						zbAccountMgr.account_zb = ((object)val2["fast_name"])?.ToString() ?? "";
						zbAccountMgr.userid_zb = ((object)val2["userid"])?.ToString() ?? "";
						zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Account_VisitorReg);
					}
					else if (m_lastPostMsgId == PostMsgId.E_ZB_MSG_REG)
					{
						zbUIMgr.Instance.BubbleInfo("Register Success!");
						zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Account_Register);
					}
					else if (m_lastPostMsgId == PostMsgId.E_ZB_MSG_LOGIN)
					{
						string token = ((object)val2["token"])?.ToString() ?? "";
						zbSdkHelper.SetLoginUserData(((object)val2["userid"])?.ToString() ?? "", token, 60001);
						zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Account_Login);
					}
				}
			}
			else if (!string.IsNullOrEmpty(text))
			{
				zbUIMgr.Instance.BubbleInfo(text);
			}
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo($"PostResponse Status:{resp.StatusCode}");
		}
	}

	private void ShowTipConnect()
	{
		zbUIMgr.Instance.ShowTipConnect();
	}

	private void HideTipConnect()
	{
		zbTipConnectWnd zbTipConnectWnd2 = zbUIMgr.Instance.GetWndById(EWndId.TipConnect) as zbTipConnectWnd;
		if ((Object)(object)zbTipConnectWnd2 != (Object)null)
		{
			zbTipConnectWnd2.SetResponsed(responsed: true);
		}
	}
}
