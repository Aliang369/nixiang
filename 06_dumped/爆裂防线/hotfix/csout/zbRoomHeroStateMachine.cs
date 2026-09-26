using System.Collections.Generic;

public class zbRoomHeroStateMachine
{
	public zbRoomHero roomHero;

	private ERoomHeroState m_eState;

	private ERoomHeroState m_eNextState;

	private Dictionary<ERoomHeroState, zbRoomHeroState> m_kStates = new Dictionary<ERoomHeroState, zbRoomHeroState>();

	public void Init(zbRoomHero _roomHero)
	{
		roomHero = _roomHero;
		m_eState = ERoomHeroState.Unknown;
		m_kStates[ERoomHeroState.Plane] = new zbRoomHeroPlaneState(this, ERoomHeroState.Plane);
		m_kStates[ERoomHeroState.MoveToTop] = new zbRoomHeroMoveToTopState(this, ERoomHeroState.MoveToTop);
		m_kStates[ERoomHeroState.MoveToRoom] = new zbRoomHeroMoveToRoomState(this, ERoomHeroState.MoveToRoom);
		m_kStates[ERoomHeroState.Idle] = new zbRoomHeroIdleState(this, ERoomHeroState.Idle);
	}

	public void OnEnable()
	{
		GetState()?.OnEnable();
	}

	public void SetState(ERoomHeroState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public ERoomHeroState GetStateId()
	{
		return m_eState;
	}

	public zbRoomHeroState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbRoomHeroState GetStateById(ERoomHeroState eState)
	{
		if (m_kStates.ContainsKey(eState))
		{
			return m_kStates[eState];
		}
		return null;
	}

	public void Update()
	{
		zbRoomHeroState state = GetState();
		zbRoomHeroState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = ERoomHeroState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
