using System.Collections.Generic;

public class zbFloorBuildingSubFsm
{
	public zbFloor floor;

	private EFloorBuildingSubState m_eState;

	private EFloorBuildingSubState m_eNextState;

	private Dictionary<EFloorBuildingSubState, zbFloorBuildingSubState> m_kStates = new Dictionary<EFloorBuildingSubState, zbFloorBuildingSubState>();

	public void Init(zbFloor _floor)
	{
		floor = _floor;
		m_eState = EFloorBuildingSubState.Unknown;
		m_kStates[EFloorBuildingSubState.In] = new zbFloorBuildingSubInState(this, EFloorBuildingSubState.In);
		m_kStates[EFloorBuildingSubState.Loop] = new zbFloorBuildingSubLoopState(this, EFloorBuildingSubState.Loop);
		m_kStates[EFloorBuildingSubState.Out] = new zbFloorBuildingSubOutState(this, EFloorBuildingSubState.Out);
		m_kStates[EFloorBuildingSubState.Exit] = new zbFloorBuildingSubExitState(this, EFloorBuildingSubState.Exit);
	}

	public void SetState(EFloorBuildingSubState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public EFloorBuildingSubState GetStateId()
	{
		return m_eState;
	}

	public zbFloorBuildingSubState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbFloorBuildingSubState GetStateById(EFloorBuildingSubState eState)
	{
		if (m_kStates.ContainsKey(eState))
		{
			return m_kStates[eState];
		}
		return null;
	}

	public void OnEnable()
	{
		zbFloorBuildingSubState state = GetState();
		EFloorBuildingSubState stateId = GetStateId();
		if (state != null && stateId != EFloorBuildingSubState.Exit)
		{
			state.OnEnable();
		}
		else
		{
			SetState(EFloorBuildingSubState.In);
		}
	}

	public void Update()
	{
		zbFloorBuildingSubState state = GetState();
		zbFloorBuildingSubState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = EFloorBuildingSubState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
