using UnityEngine;

public class zbWsHeartStateWait : zbWsHeartStateBase
{
	private float m_lastSendTm;

	public zbWsHeartStateWait(zbWsHeartStateMachine fsm, EHeartState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_lastSendTm = Time.unscaledTime;
		zbUIMgr.Instance.ShowConnectFaild();
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		float unscaledTime = Time.unscaledTime;
		if (!zbUIMgr.Instance.IsShow(EWndId.ConnectFaildTip))
		{
			if (unscaledTime - m_lastSendTm > 5f)
			{
				m_lastSendTm = unscaledTime;
				zbUIMgr.Instance.ShowConnectFaild();
			}
		}
		else
		{
			m_lastSendTm = unscaledTime;
		}
	}

	public override void OnNotifySocketOpen()
	{
		base.OnNotifySocketOpen();
		m_fsm.SetState(EHeartState.Heart);
	}
}
