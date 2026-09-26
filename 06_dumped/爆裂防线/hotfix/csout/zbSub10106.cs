using System;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;

public class zbSub10106 : zbSkillBase
{
	[NonSerialized]
	private float laserWidth = 0.1f;

	private LineRenderer lineRenderer;

	[NonSerialized]
	public SkeletonAnimation m_skeletonAnimation2;

	protected override void OnInited()
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		lineRenderer = ((Component)this).gameObject.GetComponent<LineRenderer>();
		if ((Object)(object)lineRenderer == (Object)null)
		{
			lineRenderer = ((Component)this).gameObject.GetComponentInChildren<LineRenderer>();
		}
		lineRenderer.startWidth = laserWidth;
		lineRenderer.endWidth = laserWidth;
		lineRenderer.positionCount = 2;
		zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if ((Object)(object)monsterById == (Object)null || ((Component)this).transform.position == Vector3.zero)
		{
			ReturnToPool();
			return;
		}
		Vector2 val = Vector2.op_Implicit(((Component)this).transform.position);
		Vector2 val2 = Vector2.op_Implicit(monsterById.GetCurPosition());
		lineRenderer.SetPosition(0, Vector2.op_Implicit(val));
		lineRenderer.SetPosition(1, Vector2.op_Implicit(val2));
		if ((Object)(object)m_skeletonAnimation != (Object)null)
		{
			((Component)m_skeletonAnimation).transform.position = Vector2.op_Implicit(val2);
		}
		GameObject val3 = zbUtil.FindGameObjectRecursive(((Component)this).transform, "TargetAnimation2");
		if ((Object)(object)val3 != (Object)null)
		{
			m_skeletonAnimation2 = val3.GetComponent<SkeletonAnimation>();
		}
		if ((Object)(object)m_skeletonAnimation2 != (Object)null)
		{
			((Component)m_skeletonAnimation2).transform.position = Vector2.op_Implicit(val);
		}
		CalcMonsterDamage(EAttrSkillType.Damage, monsterById);
		ReturnToPool(0.25f);
	}
}
