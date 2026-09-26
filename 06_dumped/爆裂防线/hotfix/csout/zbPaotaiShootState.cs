using System.Collections.Generic;
using UnityEngine;

public class zbPaotaiShootState(zbPaotaiStateMachine fsm, EPaotaiState eState) : zbPaotaiState(fsm, eState)
{
	private float shootInterval = 0.33f;

	private float shootTimer;

	private static readonly float attackRangeSqr = 144f;

	private void ShootBullet(Vector3 targetPositon)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		m_fsm.paotai.LookAt(targetPositon);
		zbHomeMgr.Instance.paotaiMgr.bulletPool.Spawn().Init(Vector2.op_Implicit(m_fsm.paotai.GetGunpointPosition()), Vector2.op_Implicit(targetPositon));
		m_fsm.paotai.SetAnimation("shoot", loop: false);
	}

	public override void OnEnter()
	{
		base.OnEnter();
		OnEnable();
	}

	public override void OnEnable()
	{
		base.OnEnable();
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void OnUpdate()
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		base.OnUpdate();
		shootTimer += Time.deltaTime;
		if (!(shootTimer >= shootInterval))
		{
			return;
		}
		shootTimer = 0f;
		List<zbZombie> sortedZombiesInAttackRange = zbHomeMgr.Instance.zombieMgr.GetSortedZombiesInAttackRange(attackRangeSqr);
		if (sortedZombiesInAttackRange != null && sortedZombiesInAttackRange.Count != 0)
		{
			zbZombie zbZombie2 = null;
			if (sortedZombiesInAttackRange.Count == 1)
			{
				zbZombie2 = sortedZombiesInAttackRange[0];
			}
			else
			{
				float value = Rand.value;
				zbZombie2 = ((value < 0.33f) ? sortedZombiesInAttackRange[0] : ((value < 0.9f && sortedZombiesInAttackRange.Count >= 2) ? sortedZombiesInAttackRange[1] : ((sortedZombiesInAttackRange.Count < 3) ? sortedZombiesInAttackRange[0] : sortedZombiesInAttackRange[2])));
			}
			if ((Object)(object)zbZombie2 != (Object)null)
			{
				ShootBullet(zbZombie2.GetBoundCenter());
			}
		}
	}
}
