using System;
using System.Collections;
using Spine;
using UnityEngine;

public class zbFootholdIndex1001 : zbFootholdSkillBase
{
	public stFootholdIndex1001 footholdIndex1001Data;

	[NonSerialized]
	private zbBoundsCircle2D boundsCircle2D;

	protected override void OnInited()
	{
		m_sortType = ESortLayerType.Projectile;
		base.OnInited();
		ignoreMonsters.Clear();
		((MonoBehaviour)this).StartCoroutine(ShootMonster());
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex1001Data = (stFootholdIndex1001)_footholdIndex;
		((Component)this).transform.localScale = Vector3.one * footholdIndex1001Data.damageRange;
		if (combatData.dataIndex == 2)
		{
			((Component)this).transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
		}
		else
		{
			((Component)this).transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}
	}

	public IEnumerator ShootMonster()
	{
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false);
		ReturnToPool(val.AnimationEnd);
		yield return (object)new WaitForSeconds(1.1666666f);
		UpdateMonsterCollision();
	}

	private void UpdateMonsterCollision()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		boundsCircle2D.center.x = skillTF.position.x;
		boundsCircle2D.center.y = skillTF.position.y;
		boundsCircle2D.radius = footholdIndex1001Data.damageRange / 2f;
		base.m_combatMonsterGrid.QueryList(in boundsCircle2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			ShootFormulaMonster(overlapResult);
		}
	}

	private void ShootFormulaMonster(zbMonster targetMonster)
	{
		zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, targetMonster, footholdIndex1001Data.damagePercent * (float)(long)targetMonster.maxHp, footholdEffectRecord.atkType, ignoreAttackTypeAdd: true);
		zbCombatSystem.Instance.buffSystem.CreateBuff(this, skillAttrList, targetMonster.GetUUID(), footholdIndex1001Data.burnBuffId, footholdIndex1001Data.burnTime);
	}
}
