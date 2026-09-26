using UnityEngine;

public class zbMechaSkill10022 : zbMechaSkillBase
{
	private float m_damageInterval;

	private float m_damageIntervalPre;

	private zbMechaHuo01 mechaHuo01;

	protected override void OnInited()
	{
		base.OnInited();
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		mechaHuo01 = (zbMechaHuo01)hero;
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		m_damageIntervalPre = 0f;
		m_damageInterval = mechaSkillRecord.param[0];
		ReturnToPool(mechaSkillRecord.param[1] + 5f);
	}

	protected override void Update()
	{
		base.Update();
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateMonsterCollision();
		}
	}

	private void UpdateMonsterCollision()
	{
		foreach (int item in mechaHuo01.mechaHuo01Sub.m_monstersInRange)
		{
			ShootFormulaMonster(item);
		}
	}

	private void ShootFormulaMonster(int monsterUUID)
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, monsterById, base.damage, mechaSkillRecord.atkType);
		}
	}
}
