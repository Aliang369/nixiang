using System.Collections.Generic;

public class zbGameStateMachine
{
	private static zbGameStateMachine sInstance;

	private EGameState m_eState;

	private EGameState m_eNextState;

	private Dictionary<EGameState, zbBaseState> m_kStates = new Dictionary<EGameState, zbBaseState>();

	public static zbGameStateMachine Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbGameStateMachine();
			}
			return sInstance;
		}
	}

	public void Init()
	{
		m_kStates[EGameState.Login] = new zbLoginState(EGameState.Login);
		m_kStates[EGameState.Main] = new zbMainState(EGameState.Main);
	}

	public void Free()
	{
	}

	public void SetInitState(EGameState eState)
	{
		if (m_eState == EGameState.Unknown)
		{
			m_eState = eState;
			m_kStates[m_eState].OnEnter();
		}
	}

	public void SetState(EGameState eState, bool immediately = false)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
			if (immediately)
			{
				Update();
			}
		}
	}

	public EGameState GetStateId()
	{
		return m_eState;
	}

	public zbBaseState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbBaseState GetStateById(EGameState eState)
	{
		if (m_kStates.ContainsKey(eState))
		{
			return m_kStates[eState];
		}
		return null;
	}

	public void Update()
	{
		zbBaseState state = GetState();
		zbBaseState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = EGameState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}

	public bool IsFighting()
	{
		if (Instance.GetStateId() == EGameState.Main)
		{
			return zbUIMgr.Instance.IsShow(EWndId.Combat);
		}
		return false;
	}
}
