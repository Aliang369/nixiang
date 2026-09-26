using System.Collections.Generic;
using UnityEngine;

public class zbFootholdIndex2201_2 : zbFootholdSkillBase
{
	private HashSet<int> m_monstersInRange = new HashSet<int>();

	public stFootholdIndex2201 footholdIndex2201_2Data;

	private float m_damageIntervalPre;

	protected override void OnInited()
	{
		m_sortType = ESortLayerType.Projectile;
		base.OnInited();
		canOutWindow = true;
		ReturnToPool(99999f);
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex2201_2Data = (stFootholdIndex2201)_footholdIndex;
		ReturnToPool(footholdIndex2201_2Data.durationTime);
	}

	protected override void Update()
	{
		base.Update();
		float num = 360f / footholdIndex2201_2Data.durationTime;
		((Component)this).transform.Rotate(0f, 0f, (0f - num) * Time.deltaTime);
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= footholdIndex2201_2Data.damageInterval)
		{
			m_damageIntervalPre -= footholdIndex2201_2Data.damageInterval;
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
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, monsterById, footholdIndex2201_2Data.damagePercent * (float)(long)monsterById.maxHp, footholdEffectRecord.atkType, ignoreAttackTypeAdd: true);
		}
	}
}
