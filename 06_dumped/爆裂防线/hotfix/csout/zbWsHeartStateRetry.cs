using UnityEngine;

public class zbWsHeartStateRetry : zbWsHeartStateBase
{
	private float m_lastSendTm;

	private int m_tryCount;

	public zbWsHeartStateRetry(zbWsHeartStateMachine fsm, EHeartState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_lastSendTm = Time.unscaledTime;
		m_tryCount = 0;
		zbWsMsgHandler.Instance.ShowTipConnect(bNeedTimeOutClose: true, 0f);
		zbWsMsgHandler.Instance.Connect();
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		float unscaledTime = Time.unscaledTime;
		if (m_tryCount >= 2)
		{
			m_fsm.SetState(EHeartState.Wait);
		}
		else if (unscaledTime - m_lastSendTm > 10f)
		{
			zbWsMsgHandler.Instance.ShowTipConnect(bNeedTimeOutClose: true, 0f);
			zbWsMsgHandler.Instance.Connect();
			m_tryCount++;
			m_lastSendTm = unscaledTime;
		}
	}

	public override void OnNotifySocketOpen()
	{
		base.OnNotifySocketOpen();
		m_fsm.SetState(EHeartState.Heart);
	}
}
