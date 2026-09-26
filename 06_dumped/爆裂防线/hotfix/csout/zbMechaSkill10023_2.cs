using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;

public class zbMechaSkill10023_2 : zbMechaSkillBase
{
	private HashSet<int> m_monstersInRange = new HashSet<int>();

	private float m_damageInterval;

	private float m_damageIntervalPre;

	protected override void OnInited()
	{
		m_sortType = ESortLayerType.BurnLayer;
		base.OnInited();
		((MonoBehaviour)this).StartCoroutine(ShootRepeater());
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		float num = mechaSkillRecord.param[2] / 100f;
		((Component)this).transform.localScale = new Vector3(num, num, 1f);
		ignoreMonsters.Clear();
		m_damageIntervalPre = mechaSkillRecord.param[1];
		m_damageInterval = mechaSkillRecord.param[1];
	}

	public IEnumerator ShootRepeater()
	{
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "enter", false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		m_skeletonAnimation.AnimationState.SetAnimation(0, "loop", true);
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
}
