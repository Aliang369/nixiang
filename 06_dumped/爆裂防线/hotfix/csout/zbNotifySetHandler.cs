using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;

public class zbNotifySetHandler
{
	private PushSetInfo m_pushSetInfo;

	public void Init()
	{
	}

	public void UpdatePushSetInfo(PushSetInfo info)
	{
		if (info != null)
		{
			m_pushSetInfo = info;
		}
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Push_Set_Ack);
	}

	public bool IsPushTypeClose(EPushType type)
	{
		if (m_pushSetInfo == null)
		{
			return false;
		}
		return m_pushSetInfo.CloseList.Contains(type);
	}

	public void SetPushTypeClose(EPushType type, bool isClose)
	{
		if (m_pushSetInfo == null)
		{
			return;
		}
		if (isClose)
		{
			if (!m_pushSetInfo.CloseList.Contains(type))
			{
				m_pushSetInfo.CloseList.Add(type);
			}
		}
		else
		{
			m_pushSetInfo.CloseList.Remove(type);
		}
		zbWsMsgHandler.Instance.PushSet_Close_Req(((IEnumerable<EPushType>)m_pushSetInfo.CloseList).ToArray());
	}
}
