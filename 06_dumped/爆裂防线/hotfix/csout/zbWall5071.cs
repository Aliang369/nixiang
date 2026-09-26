using System.Collections.Generic;
using UnityEngine;

public class zbWall5071 : zbWallSkillBase
{
	private HashSet<int> m_monstersInRange = new HashSet<int>();

	private float m_damageInterval;

	private float m_damageIntervalPre;

	public stWallSkillIndex5071 wallSkillIndex5071Data;

	protected override void OnInited()
	{
		base.OnInited();
		m_monstersInRange.Clear();
		ReturnToPool(20f);
	}

	public override void SetWallAtr(float _damage, zbWallSkillCsvRecord _wallSkillRecord)
	{
		base.SetWallAtr(_damage, _wallSkillRecord);
		m_damageIntervalPre = wallSkillIndex5071Data.damageInterval;
		m_damageInterval = wallSkillIndex5071Data.damageInterval;
		UpdateMonsterCollision();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_monstersInRange.Clear();
	}

	protected override void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		((Component)this).transform.Translate(Vector3.up * (float)wallSkillIndex5071Data.moveSpeed * Time.deltaTime, (Space)0);
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateMonsterCollision();
		}
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null)
		{
			m_monstersInRange.Add(component.GetUUID());
		}
	}

	private void OnTriggerExit2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null)
		{
			m_monstersInRange.Remove(component.GetUUID());
		}
	}

	private void UpdateMonsterCollision()
	{
		foreach (int item in m_monstersInRange)
		{
			ShootFormulaMonster(item);
		}
	}

	private void ShootFormulaMonster(int monsterUUID)
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, monsterById, base.damage, wallSkillRecord.atkType);
		}
	}
}
