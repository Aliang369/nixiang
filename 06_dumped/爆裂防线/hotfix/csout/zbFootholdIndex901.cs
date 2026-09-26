using System;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbFootholdIndex901 : zbFootholdSkillBase
{
	public stFootholdIndex901 footholdIndex901Data;

	[NonSerialized]
	private zbBoundsCircle2D boundsCircle2D;

	private float m_damageIntervalPre;

	private float m_damageDurationPre;

	private float m_growTime;

	private int currState;

	protected override void OnInited()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		m_sortType = ESortLayerType.Zhuangjiache;
		base.OnInited();
		ignoreMonsters.Clear();
		if (!ignoreAnimation)
		{
			int num = Rand.Range(1, 4);
			string spinePath = "animation/foothold/scene_point_mushroomwood_" + num + "/scene_point_mushroomwood_" + num + "_SkeletonData.asset";
			zbUtil.RefreshSpine(m_skeletonAnimation, spinePath);
			((SkeletonRenderer)m_skeletonAnimation).updateWhenInvisible = (UpdateMode)0;
		}
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
		footholdIndex901Data = (stFootholdIndex901)_footholdIndex;
		((Component)this).transform.localScale = Vector3.one;
		currState = 0;
		m_growTime = 0f;
		m_damageIntervalPre = 0f;
		m_damageDurationPre = 0f;
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "in", false);
		m_skeletonAnimation.timeScale = val.AnimationEnd / footholdIndex901Data.growTime;
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
		boundsCircle2D.radius = footholdIndex901Data.damageRange / 2f;
	}

	protected override void Update()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (currState == 1)
		{
			m_growTime += Time.deltaTime;
			if (m_growTime >= footholdIndex901Data.daizhiTime)
			{
				m_growTime = 0f;
				currState = 3;
			}
			base.m_combatMonsterGrid.QueryList(in boundsCircle2D, ignoreMonsters, overlapResultList);
			if (overlapResultList.Count > 0)
			{
				((Component)this).transform.localScale = Vector3.one * footholdIndex901Data.damageRange;
				m_skeletonAnimation.AnimationState.SetAnimation(0, "out", false);
				m_skeletonAnimation.AnimationState.AddAnimation(0, "out2", true, 0f);
				currState = 3;
			}
		}
		if (currState == 3)
		{
			m_damageIntervalPre += Time.deltaTime;
			if (m_damageIntervalPre >= footholdIndex901Data.damageInterval)
			{
				m_damageIntervalPre -= footholdIndex901Data.damageInterval;
				UpdateMonsterCollision();
			}
			m_damageDurationPre += Time.deltaTime;
			if (m_damageDurationPre >= footholdIndex901Data.durationTime)
			{
				currState = 4;
				ReturnToPool();
			}
		}
		if (currState == 0)
		{
			m_growTime += Time.deltaTime;
			if (m_growTime >= footholdIndex901Data.growTime)
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
		zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, targetMonster, footholdIndex901Data.damagePercent * (float)(long)targetMonster.maxHp, footholdEffectRecord.atkType, ignoreAttackTypeAdd: true);
	}
}
