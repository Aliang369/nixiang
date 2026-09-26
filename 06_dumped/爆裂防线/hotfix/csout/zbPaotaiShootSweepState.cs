using System;
using UnityEngine;

public class zbPaotaiShootSweepState(zbPaotaiStateMachine fsm, EPaotaiState eState) : zbPaotaiState(fsm, eState)
{
	private float shootInterval = 0.03f;

	private float shootTimer;

	private int totalShots = 50;

	private float startAngle = -115f;

	private float endAngle = -165f;

	private float elapsedTime;

	private float totalDuration;

	public override void OnEnter()
	{
		base.OnEnter();
		shootTimer = 0f;
		elapsedTime = 0f;
		totalDuration = shootInterval * (float)totalShots;
	}

	public override void OnUpdate()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		base.OnUpdate();
		elapsedTime += Time.deltaTime;
		shootTimer += Time.deltaTime;
		if (Mathf.FloorToInt(elapsedTime / totalDuration) >= 1)
		{
			m_fsm.SetState(EPaotaiState.Idle);
			return;
		}
		float num = Mathf.PingPong(Mathf.Clamp01(elapsedTime / totalDuration) * 2f, 1f);
		float num2 = Mathf.Lerp(startAngle, endAngle, num) * ((float)Math.PI / 180f);
		Vector3 val = new Vector3(Mathf.Cos(num2), Mathf.Sin(num2), 0f);
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector3 gunpointPosition = m_fsm.paotai.GetGunpointPosition();
		Vector3 val2 = gunpointPosition + normalized * 5f;
		m_fsm.paotai.LookAt(val2);
		if (shootTimer >= shootInterval)
		{
			shootTimer = 0f;
			FireBullet(gunpointPosition, val2);
		}
	}

	private void FireBullet(Vector3 start, Vector3 target)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		zbPaotaiBullet obj = zbHomeMgr.Instance.paotaiMgr.bulletPool.Spawn();
		obj.Init(Vector2.op_Implicit(start), Vector2.op_Implicit(target));
		obj.SetPenetrate(canPenetrate: true);
		m_fsm.paotai.SetAnimation("shoot", loop: false);
	}
}
