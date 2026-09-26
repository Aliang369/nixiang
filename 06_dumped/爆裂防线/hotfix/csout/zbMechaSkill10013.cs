using System;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbMechaSkill10013 : zbMechaSkillBase
{
	[NonSerialized]
	public Bone gunpoint;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	[NonSerialized]
	public GameObject goRiseEffect_1;

	protected override void OnInited()
	{
		base.OnInited();
		if ((Object)(object)m_skeletonAnimation != (Object)null)
		{
			gunpoint = ((SkeletonRenderer)m_skeletonAnimation).skeleton.FindBone("gunpoint");
		}
		goRiseEffect_1 = ((Component)((Component)this).transform.Find("goRiseEffect_1")).gameObject;
		needDurationPre = 0f;
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		m_damageIntervalPre = mechaSkillRecord.param[1];
		m_damageInterval = m_damageIntervalPre;
	}

	private void UpdateLaser()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if (!((Object)(object)monsterById == (Object)null))
		{
			Vector2 val = Vector2.op_Implicit(monsterById.GetCurPosition());
			Vector3 val2 = ((Component)m_skeletonAnimation).transform.InverseTransformPoint(Vector2.op_Implicit(val));
			SkeletonExtensions.SetLocalPosition(gunpoint, val2);
		}
	}

	protected override void Update()
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			ShootFormulaMonster();
		}
		zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if ((Object)(object)monsterById != (Object)null && monsterById.isCanSelected())
		{
			UpdateLaser();
		}
		else
		{
			zbMonster closestMonsterByVector = combatData.monsters.GetClosestMonsterByVector(goRiseEffect_1.transform.position);
			if ((Object)(object)closestMonsterByVector != (Object)null)
			{
				targetMonsterUUID = closestMonsterByVector.GetUUID();
				UpdateLaser();
			}
		}
		needDurationPre += Time.deltaTime;
		if (needDurationPre >= mechaSkillRecord.param[2])
		{
			needDurationPre = 0f;
			DelayDestroySelf();
		}
	}

	private void DelayDestroySelf()
	{
		ReturnToPool();
	}

	private void ShootFormulaMonster()
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, monsterById, base.damage, mechaSkillRecord.atkType);
		}
	}
}
