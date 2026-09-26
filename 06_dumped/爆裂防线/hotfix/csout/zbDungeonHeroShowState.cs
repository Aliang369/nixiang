using System.Collections;
using UnityEngine;

public class zbDungeonHeroShowState : zbDungeonHeroState
{
	public zbDungeonHeroShowState(zbDungeonHeroStateMachine fsm, EDungeonHeroState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		((MonoBehaviour)m_fsm.hero).StartCoroutine(PlayIdleAnimation());
	}

	private IEnumerator PlayIdleAnimation()
	{
		m_fsm.hero.SetAnimation("show", loop: false);
		yield return (object)new WaitForSeconds(0.5f);
		m_fsm.SetState(EDungeonHeroState.Idle);
	}
}
