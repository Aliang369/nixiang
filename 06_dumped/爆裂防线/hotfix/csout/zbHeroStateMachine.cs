using System.Collections.Generic;

public class zbHeroStateMachine
{
	public zbAttackerBase hero;

	private EHeroState m_eState;

	private EHeroState m_eNextState;

	private Dictionary<EHeroState, zbHeroState> m_kStates = new Dictionary<EHeroState, zbHeroState>();

	public void Init(zbAttackerBase obj)
	{
		hero = obj;
		m_eState = EHeroState.Unknown;
		m_kStates[EHeroState.Show] = new zbHeroShowState(this, EHeroState.Show);
		m_kStates[EHeroState.Skill] = new zbHeroSkillState(this, EHeroState.Skill);
	}

	public void SetState(EHeroState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public EHeroState GetStateId()
	{
		return m_eState;
	}

	public zbHeroState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbHeroState GetStateById(EHeroState eState)
	{
		m_kStates.TryGetValue(eState, out var value);
		return value;
	}

	public void Update()
	{
		zbHeroState state = GetState();
		zbHeroState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = EHeroState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
