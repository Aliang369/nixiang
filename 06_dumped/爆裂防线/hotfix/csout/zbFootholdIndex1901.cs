using System;
using Proto.Enums;
using Spine;
using UnityEngine;

public class zbFootholdIndex1901 : zbFootholdSkillBase
{
	public stFootholdIndex1901 footholdIndex1901Data;

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
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex1901Data = (stFootholdIndex1901)_footholdIndex;
		((Component)this).transform.localScale = Vector3.one * footholdIndex1901Data.damageRange;
		skillAttrList[EAttrSkillType.Tow] = footholdIndex1901Data.towPercent;
		boundsCircle2D.center.x = skillTF.position.x;
		boundsCircle2D.center.y = skillTF.position.y;
		boundsCircle2D.radius = footholdIndex1901Data.damageRange / 2f;
		m_damageDurationPre = 0f;
		m_damageIntervalPre = 0f;
		m_skeletonAnimation.AnimationState.SetAnimation(0, "in", false);
		m_skeletonAnimation.AnimationState.AddAnimation(0, "loop", true, 0f);
	}

	protected override void Update()
	{
		base.Update();
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= footholdIndex1901Data.damageInterval)
		{
			m_damageIntervalPre -= footholdIndex1901Data.damageInterval;
			UpdateMonsterCollision();
		}
		m_damageDurationPre += Time.deltaTime;
		if (m_damageDurationPre >= footholdIndex1901Data.durationTime)
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
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, targetMonster, footholdIndex1901Data.damagePercent * (float)(long)targetMonster.maxHp, footholdEffectRecord.atkType, ignoreAttackTypeAdd: true);
		zbSkillSystem.Instance.CreateSubSkill(11302, hero, this, skillAttrList, ((Component)this).transform.position, Vector2.up, targetMonster.GetUUID());
	}
}
