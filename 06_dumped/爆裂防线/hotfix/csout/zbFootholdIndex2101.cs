using System;
using Spine;
using UnityEngine;

public class zbFootholdIndex2101 : zbFootholdSkillBase
{
	public stFootholdIndex2101 footholdIndex2101Data;

	[NonSerialized]
	public zbBoundsCircle2D boundsCircle2D;

	private float m_damageIntervalPre;

	private float m_damageDurationPre;

	protected override void OnInited()
	{
		m_sortType = ESortLayerType.BurnLayer;
		base.OnInited();
		ignoreMonsters.Clear();
		ReturnToPool(99999f);
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex2101Data = (stFootholdIndex2101)_footholdIndex;
		((Component)this).transform.localScale = Vector3.one * footholdIndex2101Data.damageRange;
		boundsCircle2D.center.x = skillTF.position.x;
		boundsCircle2D.center.y = skillTF.position.y;
		boundsCircle2D.radius = footholdIndex2101Data.damageRange / 2f;
		m_damageDurationPre = 0f;
		m_damageIntervalPre = 0f;
		m_skeletonAnimation.AnimationState.SetAnimation(0, "in", false);
		m_skeletonAnimation.AnimationState.AddAnimation(0, "loop", true, 0f);
	}

	protected override void Update()
	{
		base.Update();
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= footholdIndex2101Data.damageInterval)
		{
			m_damageIntervalPre -= footholdIndex2101Data.damageInterval;
			UpdateMonsterCollision();
		}
		m_damageDurationPre += Time.deltaTime;
		if (m_damageDurationPre >= footholdIndex2101Data.durationTime)
		{
			m_damageDurationPre = -9999f;
			TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "out", false);
			ReturnToPool(val.AnimationEnd);
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
		zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, targetMonster, footholdIndex2101Data.damagePercent * (float)(long)targetMonster.maxHp, footholdEffectRecord.atkType, ignoreAttackTypeAdd: true);
		zbCombatSystem.Instance.buffSystem.CreateBuff(this, skillAttrList, targetMonster.GetUUID(), footholdIndex2101Data.buffId, footholdIndex2101Data.buffTime);
	}
}
