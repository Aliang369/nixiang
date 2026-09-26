using UnityEngine;

public class zbMiniMapMoveToGridState(zbMiniMapStateMachine fsm, EMiniMapState eState) : zbMiniMapState(fsm, eState)
{
	private float m_elapsed;

	private float m_duration = 0.5f;

	private Vector2 m_startPos;

	private Vector2 m_targetPos;

	public override void OnEnter()
	{
		base.OnEnter();
		OnEnable();
	}

	public override void OnEnable()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_elapsed = 0f;
		int lastX = m_fsm.m_lastX;
		int lastY = m_fsm.m_lastY;
		int nCurPlayerGridX = m_fsm.miniMap.nCurPlayerGridX;
		int nCurPlayerGridY = m_fsm.miniMap.nCurPlayerGridY;
		_ = m_fsm.miniMap.m_gridSpacingX;
		_ = m_fsm.miniMap.m_gridSpacingY;
		m_startPos = m_fsm.miniMap.GetGridPos(lastX, lastY);
		m_targetPos = m_fsm.miniMap.GetGridPos(nCurPlayerGridX, nCurPlayerGridY);
	}

	public override void OnUpdate()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		base.OnUpdate();
		m_elapsed += Time.deltaTime;
		float num = Mathf.Clamp01(m_elapsed / m_duration);
		Vector2 playerPos = Vector2.Lerp(m_startPos, m_targetPos, num);
		m_fsm.miniMap.SetPlayerPos(playerPos);
		if (num >= 1f)
		{
			m_fsm.SetState(EMiniMapState.Idle);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		m_fsm.m_lastX = m_fsm.miniMap.nCurPlayerGridX;
		m_fsm.m_lastY = m_fsm.miniMap.nCurPlayerGridY;
		m_fsm.miniMap.RefreshCurGridPlayerPos();
	}
}
