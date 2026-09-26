using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbFootholdIndex2001 : zbFootholdSkillBase
{
	private HashSet<int> m_monstersInRange = new HashSet<int>();

	public stFootholdIndex2001 footholdIndex2001Data;

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
		footholdIndex2001Data = (stFootholdIndex2001)_footholdIndex;
		skillAttrList[EAttrSkillType.Knockback] = footholdIndex2001Data.knockbackPercent;
		skillAttrList[EAttrSkillType.SlowDown] = footholdIndex2001Data.slowDownPrecent;
	}

	protected override void Update()
	{
		base.Update();
		float num = 360f / footholdIndex2001Data.rotationTime;
		((Component)this).transform.Rotate(0f, 0f, (0f - num) * Time.deltaTime);
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= footholdIndex2001Data.damageInterval)
		{
			m_damageIntervalPre -= footholdIndex2001Data.damageInterval;
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
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			zbSkillSystem.Instance.CreateSubSkill(10203, hero, this, skillAttrList, Vector3.zero, Vector2.up, monsterById.GetUUID());
			zbCombatSystem.Instance.buffSystem.CreateBuff(this, skillAttrList, monsterById.GetUUID(), 1, footholdIndex2001Data.slowDownTime);
		}
	}
}
