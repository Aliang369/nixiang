using System.Collections.Generic;

public class zbMusicStateMachine
{
	private static zbMusicStateMachine sInstance;

	private EMusicState m_eState;

	private EMusicState m_eNextState;

	private Dictionary<EMusicState, zbMusicState> m_kStates = new Dictionary<EMusicState, zbMusicState>();

	public static zbMusicStateMachine Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbMusicStateMachine();
			}
			return sInstance;
		}
	}

	public void Init()
	{
		m_kStates[EMusicState.Login] = new zbLoginMusicState(EMusicState.Login);
		m_kStates[EMusicState.Main] = new zbMainMusicState(EMusicState.Main);
		m_kStates[EMusicState.Combat] = new zbCombatMusicState(EMusicState.Combat);
		m_kStates[EMusicState.Dungeon] = new zbDungeonMusicState(EMusicState.Dungeon);
		m_kStates[EMusicState.JourneyMovie] = new zbJourneyMovieMusicState(EMusicState.JourneyMovie);
	}

	public void Free()
	{
	}

	public void SetState(EMusicState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public EMusicState GetStateId()
	{
		return m_eState;
	}

	public zbMusicState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbMusicState GetStateById(EMusicState eState)
	{
		if (m_kStates.ContainsKey(eState))
		{
			return m_kStates[eState];
		}
		return null;
	}

	public void Update()
	{
		zbMusicState state = GetState();
		zbMusicState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = EMusicState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
