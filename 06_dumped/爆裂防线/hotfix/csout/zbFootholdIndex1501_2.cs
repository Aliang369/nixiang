using System;
using Proto.Enums;
using Spine;
using UnityEngine;

public class zbFootholdIndex1501_2 : zbFootholdSkillBase
{
	public stFootholdIndex1501 footholdIndex1501Data;

	[NonSerialized]
	private zbBoundsCircle2D boundsCircle2D;

	private float hitTime;

	protected override void OnInited()
	{
		m_sortType = ESortLayerType.Projectile;
		base.OnInited();
		ignoreMonsters.Clear();
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex1501Data = (stFootholdIndex1501)_footholdIndex;
		if (combatData.dataIndex == 2)
		{
			((Component)this).transform.localScale = (Vector3.one + Vector3.down * 2f) * footholdIndex1501Data.damageRange;
		}
		else
		{
			((Component)this).transform.localScale = Vector3.one * footholdIndex1501Data.damageRange;
		}
		skillAttrList[EAttrSkillType.Knockback] = footholdIndex1501Data.carKnockbackPercent;
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false);
		ReturnToPool(val.AnimationEnd);
		hitTime = 0f;
	}

	protected override void Update()
	{
		base.Update();
		hitTime += Time.deltaTime;
		if (hitTime >= 1.1666666f)
		{
			UpdateMonsterCollision();
			m_sortType = ESortLayerType.BurnLayer;
			zbSortMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
			hitTime = -9999f;
		}
	}

	private void UpdateMonsterCollision()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		boundsCircle2D.center.x = skillTF.position.x;
		boundsCircle2D.center.y = skillTF.position.y;
		boundsCircle2D.radius = footholdIndex1501Data.damageRange / 2f;
		base.m_combatMonsterGrid.QueryList(in boundsCircle2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, overlapResult, footholdIndex1501Data.stoneDamagePercent * (float)(long)overlapResult.maxHp, footholdEffectRecord.atkType, ignoreAttackTypeAdd: true);
			zbSkillSystem.Instance.CreateSubSkill(10203, hero, this, skillAttrList, Vector3.zero, Vector2.op_Implicit(m_direction), overlapResult.GetUUID());
		}
	}
}
