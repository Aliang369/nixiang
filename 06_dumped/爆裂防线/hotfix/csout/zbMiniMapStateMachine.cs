using System.Collections.Generic;

public class zbMiniMapStateMachine
{
	public zbMiniMapCanvas miniMap;

	private EMiniMapState m_eState;

	private EMiniMapState m_eNextState;

	private Dictionary<EMiniMapState, zbMiniMapState> m_kStates = new Dictionary<EMiniMapState, zbMiniMapState>();

	public int m_lastX;

	public int m_lastY;

	public void Init(zbMiniMapCanvas _miniMap)
	{
		miniMap = _miniMap;
		m_eState = EMiniMapState.Unknown;
		m_kStates[EMiniMapState.Idle] = new zbMiniMapIdleState(this, EMiniMapState.Idle);
		m_kStates[EMiniMapState.MoveToGrid] = new zbMiniMapMoveToGridState(this, EMiniMapState.MoveToGrid);
		SetState(EMiniMapState.Idle);
	}

	public void SetState(EMiniMapState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public EMiniMapState GetStateId()
	{
		return m_eState;
	}

	public zbMiniMapState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbMiniMapState GetStateById(EMiniMapState eState)
	{
		if (m_kStates.ContainsKey(eState))
		{
			return m_kStates[eState];
		}
		return null;
	}

	public void OnEnable()
	{
		GetState()?.OnEnable();
	}

	public void Update()
	{
		zbMiniMapState state = GetState();
		zbMiniMapState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = EMiniMapState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
