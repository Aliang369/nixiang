using System;
using Spine;
using UnityEngine;

public class zbFootholdIndex1601 : zbFootholdSkillBase
{
	public stFootholdIndex1601 footholdIndex1601Data;

	[NonSerialized]
	private zbBoundsCircle2D boundsCircle2D;

	private float m_damageIntervalPre;

	private float m_damageDurationPre;

	private float m_growTime;

	private int currState;

	protected override void OnInited()
	{
		base.OnInited();
		ignoreMonsters.Clear();
		ReturnToPool(99999f);
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex1601Data = (stFootholdIndex1601)_footholdIndex;
		((Component)this).transform.localScale = Vector3.one;
		currState = 0;
		m_growTime = 0f;
		m_damageIntervalPre = 0f;
		m_damageDurationPre = 0f;
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "in", false);
		m_skeletonAnimation.timeScale = val.AnimationEnd / footholdIndex1601Data.growTime;
		if (combatData.dataIndex == 2)
		{
			((Component)this).transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
			boundsCircle2D.center.y = skillTF.position.y - 0.25f;
		}
		else
		{
			((Component)this).transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			boundsCircle2D.center.y = skillTF.position.y + 0.25f;
		}
		boundsCircle2D.center.x = skillTF.position.x;
		boundsCircle2D.radius = footholdIndex1601Data.damageRange / 2f;
	}

	protected override void Update()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (currState == 1)
		{
			m_growTime += Time.deltaTime;
			if (m_growTime >= footholdIndex1601Data.daizhiTime)
			{
				m_growTime = 0f;
				currState = 3;
			}
			base.m_combatMonsterGrid.QueryList(in boundsCircle2D, ignoreMonsters, overlapResultList);
			if (overlapResultList.Count > 0)
			{
				((Component)this).transform.localScale = Vector3.one * footholdIndex1601Data.damageRange;
				m_skeletonAnimation.AnimationState.SetAnimation(0, "out", false);
				m_skeletonAnimation.AnimationState.AddAnimation(0, "out2", true, 0f);
				currState = 3;
			}
		}
		if (currState == 3)
		{
			m_damageIntervalPre += Time.deltaTime;
			if (m_damageIntervalPre >= footholdIndex1601Data.damageInterval)
			{
				m_damageIntervalPre -= footholdIndex1601Data.damageInterval;
				UpdateMonsterCollision();
			}
			m_damageDurationPre += Time.deltaTime;
			if (m_damageDurationPre >= footholdIndex1601Data.durationTime)
			{
				currState = 4;
				ReturnToPool();
			}
		}
		if (currState == 0)
		{
			m_growTime += Time.deltaTime;
			if (m_growTime >= footholdIndex1601Data.growTime)
			{
				m_growTime = 0f;
				TriggerGrowEnd();
			}
		}
	}

	private void TriggerGrowEnd()
	{
		m_skeletonAnimation.AnimationState.SetAnimation(0, "loop", true);
		m_skeletonAnimation.timeScale = 1f;
		currState = 1;
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
		zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, targetMonster, footholdIndex1601Data.damagePercent * (float)(long)targetMonster.maxHp, footholdEffectRecord.atkType, ignoreAttackTypeAdd: true);
		if (targetMonster.monsterRecord.atkType == 2)
		{
			zbCombatSystem.Instance.buffSystem.CreateBuff(this, skillAttrList, targetMonster.GetUUID(), footholdIndex1601Data.buffId, footholdIndex1601Data.hitRateTime, 0, footholdIndex1601Data.hitRatePercent);
		}
	}
}
