using System.Collections.Generic;
using UnityEngine;

public class zbDungeonHeroStateMachine
{
	public zbDungeonHero hero;

	private EDungeonHeroState m_eState;

	private EDungeonHeroState m_eNextState;

	private Dictionary<EDungeonHeroState, zbDungeonHeroState> m_kStates = new Dictionary<EDungeonHeroState, zbDungeonHeroState>();

	public void Init(zbDungeonHero _hero)
	{
		hero = _hero;
		m_eState = EDungeonHeroState.Unknown;
		m_kStates[EDungeonHeroState.Show] = new zbDungeonHeroShowState(this, EDungeonHeroState.Show);
		m_kStates[EDungeonHeroState.Idle] = new zbDungeonHeroIdleState(this, EDungeonHeroState.Idle);
		m_kStates[EDungeonHeroState.Run] = new zbDungeonHeroRunState(this, EDungeonHeroState.Run);
		m_kStates[EDungeonHeroState.OpenDoor] = new zbDungeonHeroOpenDoorState(this, EDungeonHeroState.OpenDoor);
		m_kStates[EDungeonHeroState.Elevaror] = new zbDungeonHeroElevatorState(this, EDungeonHeroState.Elevaror);
		m_kStates[EDungeonHeroState.MoveRoom] = new zbDungeonHeroMoveRoomState(this, EDungeonHeroState.MoveRoom);
		m_kStates[EDungeonHeroState.Event] = new zbDungeonHeroEventState(this, EDungeonHeroState.Event);
		m_kStates[EDungeonHeroState.EventTrigger] = new zbDungeonHeroTiggerEventState(this, EDungeonHeroState.EventTrigger);
		m_kStates[EDungeonHeroState.RiotEvent] = new zbDungeonHeroRiotEventState(this, EDungeonHeroState.RiotEvent);
	}

	public void SetState(EDungeonHeroState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public EDungeonHeroState GetStateId()
	{
		return m_eState;
	}

	public zbDungeonHeroState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbDungeonHeroState GetStateById(EDungeonHeroState eState)
	{
		if (m_kStates.ContainsKey(eState))
		{
			return m_kStates[eState];
		}
		return null;
	}

	public void Update()
	{
		zbDungeonHeroState state = GetState();
		zbDungeonHeroState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = EDungeonHeroState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}

	public void ProcessMoveInput(Vector2 joystickInput)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		GetState()?.ProcessMoveInput(joystickInput);
	}
}
