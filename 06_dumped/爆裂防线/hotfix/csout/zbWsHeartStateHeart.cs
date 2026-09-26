using UnityEngine;

public class zbWsHeartStateHeart : zbWsHeartStateBase
{
	private float m_lastSendTm;

	private bool m_bSending;

	private int m_missCount;

	public zbWsHeartStateHeart(zbWsHeartStateMachine fsm, EHeartState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_lastSendTm = Time.unscaledTime;
		m_bSending = false;
		m_missCount = 0;
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (zbSelf.Instance.IsLoading())
		{
			return;
		}
		float unscaledTime = Time.unscaledTime;
		if (m_bSending && unscaledTime - m_lastSendTm > 5f)
		{
			m_bSending = false;
			m_missCount++;
			if (m_missCount >= 2)
			{
				m_fsm.SetState(EHeartState.Retry);
			}
		}
		else if (!m_bSending && unscaledTime - m_lastSendTm > 8f)
		{
			zbWsMsgHandler.Instance.HeartBeat_Req();
			m_bSending = true;
			m_lastSendTm = unscaledTime;
		}
	}

	public override void OnNotifyHeartAck()
	{
		base.OnNotifyHeartAck();
		m_bSending = false;
		m_missCount = 0;
	}
}
