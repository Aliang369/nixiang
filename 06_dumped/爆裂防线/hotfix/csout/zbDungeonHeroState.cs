using UnityEngine;

public class zbDungeonHeroState
{
	protected EDungeonHeroState m_eState;

	protected zbDungeonHeroStateMachine m_fsm;

	private AudioSource m_runAudioS;

	public float actionTime = 1f;

	public float eventTriggerRange = 0.5f;

	public zbDungeonHeroState(zbDungeonHeroStateMachine fsm, EDungeonHeroState eState)
	{
		m_fsm = fsm;
		m_eState = eState;
	}

	public void PlayMoveAudio()
	{
		if ((Object)(object)m_runAudioS == (Object)null)
		{
			m_runAudioS = zbAudioMgr.Instance.PlayAudio(zbAudioDefine.RM_MOVE, loop: true);
		}
	}

	public void StopMoveAudio()
	{
		if ((Object)(object)m_runAudioS != (Object)null)
		{
			zbAudioMgr.Instance.StopAudio(m_runAudioS);
			m_runAudioS = null;
		}
	}

	public virtual void OnEnter()
	{
	}

	public virtual void OnExit()
	{
	}

	public virtual void OnUpdate()
	{
	}

	public virtual void ProcessMoveInput(Vector2 joystickInput)
	{
	}
}
