using System.Collections.Generic;
using UnityEngine;

public class zbMechaSkill10051 : zbMechaSkillBase
{
	private BoxCollider2D boxCollider;

	private Vector2 boxColliderSize;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private zbMechaNengliang01 mechaNengliang01;

	private HashSet<int> m_monstersInRange;

	protected override void OnInited()
	{
		base.OnInited();
		mechaNengliang01 = (zbMechaNengliang01)hero;
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		boxColliderSize.x = mechaSkillRecord.param[0] / 100f;
		boxCollider = ((Component)this).GetComponent<BoxCollider2D>();
		boxCollider.size = boxColliderSize;
		m_damageIntervalPre = mechaSkillRecord.param[1];
		m_damageInterval = m_damageIntervalPre;
	}

	protected override void Update()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateMonsterCollision();
		}
		((Component)this).transform.up = zbUtil.GetTargetNormalized(mechaNengliang01.goRiseEffect_1.transform.position, mechaNengliang01.goRiseEffect_2.transform.position);
		((Component)this).transform.position = mechaNengliang01.goRiseEffect_1.transform.position;
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
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, monsterById, base.damage, mechaSkillRecord.atkType);
		}
	}

	public zbMechaSkill10051()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		boxColliderSize = new Vector2(1f, 12f);
		m_monstersInRange = new HashSet<int>();
		base._002Ector();
	}
}
