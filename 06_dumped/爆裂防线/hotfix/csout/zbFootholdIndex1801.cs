using System;
using UnityEngine;

public class zbFootholdIndex1801 : zbFootholdSkillBase
{
	public stFootholdIndex1801 footholdIndex1801Data;

	[NonSerialized]
	private zbBounds2D bounds2D;

	private float m_delayTimePre;

	private float m_durationTimePre;

	private bool canFight;

	protected override void OnInited()
	{
		m_sortType = ESortLayerType.BurnLayer;
		base.OnInited();
		canOutWindow = true;
		ignoreMonsters.Clear();
		canFight = false;
		ReturnToPool(99999f);
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex1801Data = (stFootholdIndex1801)_footholdIndex;
	}

	public void TriggerDamage(int index)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		int num = index % 7;
		int num2 = index / 7;
		float num3 = -3 + num;
		float num4 = -5.1f + (float)num2;
		((Component)this).transform.position = new Vector3(num3 + combatData.nodeRootPositonX, num4 + combatData.nodeRootPositonY, ((Component)this).transform.position.z);
		m_skeletonAnimation.AnimationState.SetAnimation(0, "open", true);
		bounds2D.center.x = skillTF.position.x;
		bounds2D.center.y = skillTF.position.y;
		bounds2D.halfSize.x = 0.5f;
		bounds2D.halfSize.y = 0.5f;
		m_delayTimePre = 0f;
		m_durationTimePre = 0f;
		canFight = true;
		zbSortMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
	}

	protected override void Update()
	{
		base.Update();
		if (canFight)
		{
			m_delayTimePre += Time.deltaTime;
			if (m_delayTimePre >= footholdIndex1801Data.delayTime)
			{
				m_delayTimePre = -9999f;
				m_skeletonAnimation.AnimationState.SetAnimation(0, "in", false);
				m_skeletonAnimation.AnimationState.AddAnimation(0, "open", true, 0f);
				UpdateMonsterCollision();
			}
			m_durationTimePre += Time.deltaTime;
			if (m_durationTimePre >= footholdIndex1801Data.durationTime + footholdIndex1801Data.delayTime)
			{
				m_durationTimePre = -9999f;
				m_skeletonAnimation.AnimationState.SetAnimation(0, "close", true);
				canFight = false;
			}
		}
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
		zbCombatSystem.Instance.buffSystem.CreateBuff(this, skillAttrList, targetMonster.GetUUID(), footholdIndex1801Data.buffId, footholdIndex1801Data.buffTime, 0, footholdIndex1801Data.moveRange);
	}
}
