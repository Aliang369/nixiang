using System;
using UnityEngine;

public class zbFootholdIndex501 : zbFootholdSkillBase
{
	public stFootholdIndex501 footholdIndex501Data;

	[NonSerialized]
	private zbBounds2D bounds2D;

	protected override void OnInited()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		m_sortType = ESortLayerType.BurnLayer;
		base.OnInited();
		ignoreMonsters.Clear();
		if (combatData.dataIndex == 2)
		{
			((Component)this).transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
			bounds2D.center.y = skillTF.position.y - 0.25f;
		}
		else
		{
			((Component)this).transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			bounds2D.center.y = skillTF.position.y + 0.25f;
		}
		bounds2D.center.x = skillTF.position.x;
		bounds2D.halfSize.x = 0.7f;
		bounds2D.halfSize.y = 0.5f;
		m_skeletonAnimation.AnimationState.SetAnimation(0, "loop", true);
		ReturnToPool(99999f);
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex501Data = (stFootholdIndex501)_footholdIndex;
	}

	public void TriggerDamage()
	{
		m_skeletonAnimation.AnimationState.SetAnimation(0, "act", false);
		UpdateMonsterCollision();
	}

	private void UpdateMonsterCollision()
	{
		base.m_combatMonsterGrid.QueryList(in bounds2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			ShootFormulaMonster(overlapResult);
		}
	}

	private void ShootFormulaMonster(zbMonster targetMonster)
	{
		zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, targetMonster, footholdIndex501Data.damagePercent * (float)(long)targetMonster.maxHp, footholdEffectRecord.atkType, ignoreAttackTypeAdd: true);
		zbCombatSystem.Instance.buffSystem.CreateBuff(this, skillAttrList, targetMonster.GetUUID(), 4, footholdIndex501Data.dazeTime);
	}
}
