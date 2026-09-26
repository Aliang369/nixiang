using UnityEngine;

public class zbLegionHeroWalk : zbLegionHeroState
{
	private float m_targetX;

	private bool m_isWalking;

	public zbLegionHeroWalk(zbLegionHeroMachine fsm)
		: base(fsm)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		legionHero.PlayAnimation("walk");
		SetTargetPos();
	}

	private void SetTargetPos()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		m_targetX = legionHero.GetRandPosX();
		m_isWalking = true;
		legionHero.SetFacing(m_targetX < ((Component)legionHero).transform.position.x);
	}

	public override void OnExit()
	{
		base.OnExit();
		m_isWalking = false;
	}

	public override void OnUpdate()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		base.OnUpdate();
		if (m_isWalking)
		{
			Vector3 position = ((Component)legionHero).transform.position;
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(m_targetX, position.y, position.z);
			float num = legionHero.moveSpeed * Time.deltaTime;
			Vector3 val2 = Vector3.MoveTowards(position, val, num);
			((Component)legionHero).transform.position = val2;
			if (Mathf.Abs(val2.x - m_targetX) <= 0.001f)
			{
				DecideNextState();
			}
		}
	}

	private void DecideNextState()
	{
		m_isWalking = false;
		if (Random.value < 0.1f)
		{
			SetTargetPos();
		}
		else
		{
			m_fsm.SetState(ELegionHeroState.Idle);
		}
	}
}
