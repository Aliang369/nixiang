using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;

public class zbChatHandler
{
	public ChatDetail[] worldDetails;

	public ChatDetail[] legionDetails;

	public ChatState[] stateList;

	public RepeatedField<RoleBaseInfo> shielding = new RepeatedField<RoleBaseInfo>();

	public ChatDetail[] curDetails;

	public EChatType curType;

	public void Init()
	{
	}

	public void SetCurrentMsg(ChatDetail[] _details, EChatType type)
	{
		curType = type;
		curDetails = _details;
	}

	public void UpdateCurrentMsg()
	{
		if (curDetails != null && curDetails.Count() > 0)
		{
			switch (curType)
			{
			case EChatType.Eworld:
				worldDetails = curDetails.Concat(worldDetails).ToArray();
				break;
			case EChatType.Elegion:
				legionDetails = curDetails.Concat(legionDetails).ToArray();
				break;
			}
			curDetails = null;
		}
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Chat_Init);
	}

	public void UpdateWorldDetails(ChatDetail[] _details)
	{
		worldDetails = _details;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Chat_Init);
	}

	public void UpdateWorldDetailsByRecive(ChatDetail[] _details)
	{
		if (worldDetails != null && worldDetails.Length != 0)
		{
			worldDetails = _details.Concat(worldDetails).ToArray();
		}
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Chat_Update);
	}

	public void UpdateLegionDetails(ChatDetail[] _details)
	{
		legionDetails = _details;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Chat_Init);
	}

	public void UpdateLegionDetailsByRecive(ChatDetail[] _details)
	{
		if (legionDetails != null && legionDetails.Length != 0)
		{
			legionDetails = _details.Concat(legionDetails).ToArray();
		}
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Chat_Update);
	}

	public void UpdateStateList(ChatState[] _stateList)
	{
		stateList = _stateList;
	}

	public long GetChatCd(EChatType chatType)
	{
		ChatState chatState = zbSelf.Instance.chatHandler.stateList?.FirstOrDefault((ChatState s) => s.ChatType == chatType);
		if (chatState == null || chatState.CD <= 0)
		{
			return 0L;
		}
		long num = zbSelf.Instance.timeHandler.GetServerTime() - chatState.CD;
		if (num < 0)
		{
			num = 0L;
		}
		long num2 = 30 - num;
		if (num2 <= 0)
		{
			return 0L;
		}
		return num2;
	}

	public bool HasRedPoint()
	{
		return false;
	}
}
