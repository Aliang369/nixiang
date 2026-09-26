using System;
using Proto.Enums;
using UnityEngine;

public class zbFootholdIndex1201 : zbFootholdSkillBase
{
	public stFootholdIndex1201 footholdIndex1201Data;

	[NonSerialized]
	private zbBoundsCircle2D boundsCircle2D;

	private bool canFight;

	private float m_damageIntervalPre;

	private float m_damageDurationPre;

	protected override void OnInited()
	{
		m_sortType = ESortLayerType.BurnLayer;
		base.OnInited();
		ignoreMonsters.Clear();
		m_skeletonAnimation.AnimationState.SetAnimation(0, "close", true);
		ReturnToPool(99999f);
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex1201Data = (stFootholdIndex1201)_footholdIndex;
		canFight = false;
		((Component)this).transform.localScale = Vector3.one;
		boundsCircle2D.center.x = skillTF.position.x;
		boundsCircle2D.center.y = skillTF.position.y;
		boundsCircle2D.radius = footholdIndex1201Data.damageRange / 2f;
		skillAttrList[EAttrSkillType.SlowDown] = footholdIndex1201Data.slowDownPrecent;
	}

	public void TriggerDamage()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localScale = Vector3.one * footholdIndex1201Data.damageRange / 2f;
		m_skeletonAnimation.AnimationState.SetAnimation(0, "closetoopen", false);
		m_skeletonAnimation.AnimationState.AddAnimation(0, "open", true, 0f);
		m_damageDurationPre = 0f;
		canFight = true;
	}

	protected override void Update()
	{
		base.Update();
		if (canFight)
		{
			m_damageIntervalPre += Time.deltaTime;
			if (m_damageIntervalPre >= footholdIndex1201Data.damageInterval)
			{
				m_damageIntervalPre -= footholdIndex1201Data.damageInterval;
				UpdateMonsterCollision();
			}
			m_damageDurationPre += Time.deltaTime;
			if (m_damageDurationPre >= footholdIndex1201Data.durationTime)
			{
				canFight = false;
				m_skeletonAnimation.AnimationState.SetAnimation(0, "opentoclose", false);
				m_skeletonAnimation.AnimationState.AddAnimation(0, "close", true, 0f);
			}
		}
	}

	private void UpdateMonsterCollision()
	{
		base.m_combatMonsterGrid.QueryList(in boundsCircle2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			ShootFormulaMonster(overlapResult);
		}
	}

	private void ShootFormulaMonster(zbMonster targetMonster)
	{
		zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, targetMonster, footholdIndex1201Data.damagePercent * (float)(long)targetMonster.maxHp, footholdEffectRecord.atkType, ignoreAttackTypeAdd: true);
		zbCombatSystem.Instance.buffSystem.CreateBuff(this, skillAttrList, targetMonster.GetUUID(), 1, footholdIndex1201Data.slowDownTime);
	}
}
