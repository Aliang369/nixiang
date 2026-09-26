using System;
using System.Collections;
using Spine;
using UnityEngine;

public class zbFootholdIndex701 : zbFootholdSkillBase
{
	public stFootholdIndex701 footholdIndex701Data;

	[NonSerialized]
	private zbBoundsCircle2D boundsCircle2D;

	protected override void OnInited()
	{
		base.OnInited();
		ignoreMonsters.Clear();
		((MonoBehaviour)this).StartCoroutine(ShootMonster());
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex701Data = (stFootholdIndex701)_footholdIndex;
		((Component)this).transform.localScale = Vector3.one * footholdIndex701Data.damageRange;
	}

	public IEnumerator ShootMonster()
	{
		TrackEntry enter = m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false);
		ReturnToPool(enter.AnimationEnd);
		yield return (object)new WaitForSeconds(2f / 3f);
		UpdateMonsterCollision();
		yield return (object)new WaitForSeconds(2f / 3f);
		m_sortType = ESortLayerType.BurnLayer;
		zbSortMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
		yield return (object)new WaitForSeconds(enter.AnimationEnd - 1.3333334f);
	}

	private void UpdateMonsterCollision()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		boundsCircle2D.center.x = skillTF.position.x;
		boundsCircle2D.center.y = skillTF.position.y;
		boundsCircle2D.radius = footholdIndex701Data.damageRange / 2f;
		base.m_combatMonsterGrid.QueryList(in boundsCircle2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			ShootFormulaMonster(overlapResult);
		}
	}

	private void ShootFormulaMonster(zbMonster targetMonster)
	{
		zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, targetMonster, footholdIndex701Data.damagePercent * (float)(long)targetMonster.maxHp, footholdEffectRecord.atkType, ignoreAttackTypeAdd: true);
		zbCombatSystem.Instance.buffSystem.CreateBuff(this, skillAttrList, targetMonster.GetUUID(), 4, footholdIndex701Data.dazeTime);
	}
}
