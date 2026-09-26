using System.Collections.Generic;
using UnityEngine;

public class zbMechaSkill10055 : zbMechaSkillBase
{
	private float m_damageInterval;

	private float m_damageIntervalPre;

	protected override void OnInited()
	{
		base.OnInited();
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		m_damageIntervalPre = mechaSkillRecord.param[0];
		m_damageInterval = m_damageIntervalPre;
	}

	protected override void Update()
	{
		base.Update();
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			ShootFormulaMonster();
		}
	}

	private void ShootFormulaMonster()
	{
		foreach (KeyValuePair<int, zbMonster> monster in combatData.monsters.monsterList)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, monster.Value, base.damage, mechaSkillRecord.atkType);
		}
	}
}
