using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbFootholdIndex201 : zbFootholdSkillBase
{
	private HashSet<int> m_monstersInRange = new HashSet<int>();

	private bool canFight;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private float m_damageDurationPre;

	public stFootholdIndex201 footholdIndex201Data;

	protected override void OnInited()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		m_sortType = ESortLayerType.BurnLayer;
		base.OnInited();
		ignoreMonsters.Clear();
		m_skeletonAnimation.AnimationState.SetAnimation(0, "normal", true);
		((Component)this).transform.localScale = Vector3.one * 0.75f * Rand.Range(0.8f, 1f);
		((Component)this).transform.localRotation = Quaternion.Euler(0f, 0f, Rand.Range(-30f, 30f));
		ReturnToPool(99999f);
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex201Data = (stFootholdIndex201)_footholdIndex;
		skillAttrList[EAttrSkillType.SlowDown] = footholdIndex201Data.slowDownPrecent;
		skillAttrList[EAttrSkillType.SlowTime] = 0f;
	}

	public void TriggerDamage()
	{
		canFight = true;
		m_damageDurationPre = 0f;
		m_skeletonAnimation.AnimationState.SetAnimation(0, "in", false);
		m_skeletonAnimation.AnimationState.AddAnimation(0, "loop", true, 0f);
	}

	protected override void Update()
	{
		base.Update();
		if (canFight)
		{
			m_damageIntervalPre += Time.deltaTime;
			if (m_damageIntervalPre >= m_damageInterval)
			{
				m_damageIntervalPre -= m_damageInterval;
				UpdateMonsterCollision();
			}
			m_damageDurationPre += Time.deltaTime;
			if (m_damageDurationPre >= footholdIndex201Data.durationTime)
			{
				canFight = false;
				m_skeletonAnimation.AnimationState.SetAnimation(0, "out", false);
				m_skeletonAnimation.AnimationState.AddAnimation(0, "normal", true, 0f);
			}
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
		if (!canFight)
		{
			return;
		}
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
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, monsterById, footholdIndex201Data.damagePercent * (float)(long)monsterById.maxHp, footholdEffectRecord.atkType, ignoreAttackTypeAdd: true);
			if (footholdIndex201Data.freezeTime > 0f)
			{
				zbCombatSystem.Instance.buffSystem.CreateBuff(this, skillAttrList, monsterById.GetUUID(), 2, footholdIndex201Data.freezeTime);
			}
			if (footholdIndex201Data.slowDownTime > 0f)
			{
				zbCombatSystem.Instance.buffSystem.CreateBuff(this, skillAttrList, monsterById.GetUUID(), 1, footholdIndex201Data.slowDownTime);
			}
		}
	}
}
