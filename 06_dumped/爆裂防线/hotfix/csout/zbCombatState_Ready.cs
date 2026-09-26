using System.Collections;
using UnityEngine;

public class zbCombatState_Ready : zbCombatState
{
	public zbCombatState_Ready(zbCombatStateMachine fsm, ECombatState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		((MonoBehaviour)zbUIMgr.Instance.GetWndById(EWndId.Combat)).StartCoroutine(GotFightState());
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
	}

	private IEnumerator GotFightState()
	{
		yield return (object)new WaitForSeconds(0.5f);
		m_fsm.SetState(ECombatState.Fight);
	}
}
