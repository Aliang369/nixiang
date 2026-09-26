using System.Collections.Generic;
using UnityEngine;

public class zbMechaSkill10021 : zbMechaSkillBase
{
	private zbMechaHuo01 mechaHuo01;

	private HashSet<int> m_monstersInRange = new HashSet<int>();

	private float m_damageInterval;

	private float m_damageIntervalPre;

	protected override void OnInited()
	{
		m_sortType = ESortLayerType.BurnLayer;
		zbSortMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
		base.OnInited();
		ignoreMonsters.Clear();
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		mechaHuo01 = (zbMechaHuo01)hero;
		float num = mechaSkillRecord.param[1] / 100f;
		((Component)this).transform.localScale = new Vector3(num, num, 1f);
		mechaHuo01.skill1ZoomRange = num;
		m_damageIntervalPre = 0f;
		m_damageInterval = mechaSkillRecord.param[0];
		ReturnToPool(mechaSkillRecord.param[3]);
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

	protected override void OnDisable()
	{
		base.OnDisable();
		m_monstersInRange.Clear();
	}

	protected override void Update()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateMonsterCollision();
		}
		if (mechaHuo01.skill1CanZoom)
		{
			((Component)this).transform.localScale = new Vector3(mechaHuo01.skill1ZoomRange, mechaHuo01.skill1ZoomRange, 1f);
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
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, monsterById, base.damage, mechaSkillRecord.atkType);
		}
	}
}
