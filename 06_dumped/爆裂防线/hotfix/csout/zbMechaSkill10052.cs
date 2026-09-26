using System;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbMechaSkill10052 : zbMechaSkillBase
{
	private float m_damageInterval;

	private float m_damageIntervalPre;

	[NonSerialized]
	public Bone gunpoint;

	[NonSerialized]
	public GameObject goRiseEffect;

	private zbMechaNengliang01 mechaNengliang01;

	private int subSkillId;

	protected override void OnInited()
	{
		base.OnInited();
		mechaNengliang01 = (zbMechaNengliang01)hero;
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		m_damageIntervalPre = mechaSkillRecord.param[0];
		m_damageInterval = m_damageIntervalPre;
		subSkillId = (int)mechaSkillRecord.param[2];
	}

	private void UpdateLaser()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if (!((Object)(object)monsterById == (Object)null))
		{
			Vector2 val = Vector2.op_Implicit(monsterById.GetCurPosition());
			Vector3 val2 = ((Component)mechaNengliang01.anim.skeletonAnimation).transform.InverseTransformPoint(Vector2.op_Implicit(val));
			SkeletonExtensions.SetLocalPosition(gunpoint, val2);
		}
	}

	protected override void Update()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
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
			return;
		}
		zbMonster closestMonsterByVector = combatData.monsters.GetClosestMonsterByVector(goRiseEffect.transform.position);
		if ((Object)(object)closestMonsterByVector != (Object)null)
		{
			targetMonsterUUID = closestMonsterByVector.GetUUID();
			UpdateLaser();
		}
	}

	private void ShootFormulaMonster()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, monsterById, base.damage, mechaSkillRecord.atkType);
			if (subSkillId > 0)
			{
				zbSkillSystem.Instance.CreateSubSkill(subSkillId, mechaNengliang01, this, skillAttrList, ((Component)monsterById).transform.position, Vector2.up, monsterById.GetUUID());
			}
		}
	}
}
