using System;
using System.Collections;
using Proto.Enums;
using UnityEngine;

public class zbHeroPao : zbHeroMain
{
	public override void FireToTargetPosition(int targetMonsterId, Vector3 targetPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Vector3 gunpointPosition = GetGunpointPosition();
		Vector3 val = targetPosition - gunpointPosition;
		Vector2 directionToTarget = Vector2.op_Implicit(((Vector3)(ref val)).normalized);
		int num = (int)GetAttrValueByType(EAttrSkillType.Repeater);
		for (int i = 0; i < num + 1; i++)
		{
			float delayTime = (float)i * 0.1f;
			((MonoBehaviour)this).StartCoroutine(ShootRepeater(directionToTarget, gunpointPosition, delayTime));
		}
		zbAudioMgr.Instance.PlayAudio("audio/combat/pao.ogg");
	}

	public IEnumerator ShootRepeater(Vector2 directionToTarget, Vector3 offsetPosition, float delayTime)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		yield return (object)new WaitForSeconds(delayTime);
		int num = (int)GetAttrValueByType(EAttrSkillType.Volley);
		for (int i = 0; i < num + 1; i++)
		{
			int num2 = (int)Math.Ceiling((float)i / 2f);
			float num3 = spreadAngle * (float)num2;
			if (i % 2 == 1)
			{
				num3 = (0f - spreadAngle) * (float)num2;
			}
			Vector2 direction = Vector2.op_Implicit(Quaternion.Euler(0f, 0f, num3) * Vector2.op_Implicit(directionToTarget));
			zbSkillSystem.Instance.CreateSkill(this, offsetPosition, Vector2.up).SetDirection(direction);
		}
	}
}
