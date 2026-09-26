using System.Collections.Generic;

public class zbWsHeartStateMachine
{
	private static zbWsHeartStateMachine sInstance;

	private EHeartState m_eState;

	private EHeartState m_eNextState;

	private Dictionary<EHeartState, zbWsHeartStateBase> m_kStates = new Dictionary<EHeartState, zbWsHeartStateBase>();

	public static zbWsHeartStateMachine Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbWsHeartStateMachine();
			}
			return sInstance;
		}
	}

	public void Init()
	{
		m_kStates[EHeartState.Heart] = new zbWsHeartStateHeart(this, EHeartState.Heart);
		m_kStates[EHeartState.Retry] = new zbWsHeartStateRetry(this, EHeartState.Retry);
		m_kStates[EHeartState.Wait] = new zbWsHeartStateWait(this, EHeartState.Wait);
		SetState(EHeartState.Heart);
	}

	public void Free()
	{
	}

	public void SetState(EHeartState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public EHeartState GetStateId()
	{
		return m_eState;
	}

	public zbWsHeartStateBase GetState()
	{
		return GetStateById(m_eState);
	}

	public zbWsHeartStateBase GetStateById(EHeartState eState)
	{
		if (m_kStates.ContainsKey(eState))
		{
			return m_kStates[eState];
		}
		return null;
	}

	public void Update()
	{
		zbWsHeartStateBase state = GetState();
		zbWsHeartStateBase stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = EHeartState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
