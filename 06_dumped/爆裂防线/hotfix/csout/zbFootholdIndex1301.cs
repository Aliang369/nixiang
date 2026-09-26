using System;
using UnityEngine;

public class zbFootholdIndex1301 : zbFootholdSkillBase
{
	public stFootholdIndex1301 footholdIndex1301Data;

	[NonSerialized]
	private zbBoundsCircle2D boundsCircle2D;

	[NonSerialized]
	private zbBounds2D bounds2D;

	private int fightState;

	private float m_damageEatPre;

	private float scalef = 0.9f;

	protected override void OnInited()
	{
		base.OnInited();
		ignoreMonsters.Clear();
		ReturnToPool(99999f);
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex1301Data = (stFootholdIndex1301)_footholdIndex;
		fightState = 0;
		m_damageEatPre = 0f;
		m_skeletonAnimation.AnimationState.SetAnimation(0, "skin2", true);
	}

	public void SetScaleFlip(bool scaleFlip)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		num = ((combatData.dataIndex == 2) ? (-2) : 0);
		if (!scaleFlip)
		{
			((Component)this).transform.localScale = (Vector3.one + Vector3.up * (float)num) * scalef;
			bounds2D.center.x = skillTF.position.x + 0.5f * scalef;
		}
		else
		{
			((Component)this).transform.localScale = (Vector3.one + Vector3.left * 2f + Vector3.up * (float)num) * scalef;
			bounds2D.center.x = skillTF.position.x - scalef;
		}
		if (combatData.dataIndex == 2)
		{
			bounds2D.center.y = skillTF.position.y - 1f * scalef;
			bounds2D.halfSize.x = 1f;
			bounds2D.halfSize.y = 0f;
		}
		else
		{
			bounds2D.center.y = skillTF.position.y + 1f * scalef;
			bounds2D.halfSize.x = 1f;
			bounds2D.halfSize.y = 0f;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (fightState == 0)
		{
			UpdateMonsterCollision();
		}
		if (fightState == 1)
		{
			m_damageEatPre += Time.deltaTime;
			if (m_damageEatPre >= footholdIndex1301Data.eatTime)
			{
				m_damageEatPre = -99999f;
				EndEat();
			}
		}
	}

	private void UpdateMonsterCollision()
	{
		base.m_combatMonsterGrid.QueryList(in bounds2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			if (overlapResult.GetMass() <= (float)footholdIndex1301Data.massLimit)
			{
				TriggerEat(overlapResult);
				break;
			}
		}
	}

	public void TriggerEat(zbMonster targetMonster)
	{
		fightState = 1;
		m_damageEatPre = 0f;
		m_skeletonAnimation.AnimationState.SetAnimation(0, "skin2_actin", false);
		m_skeletonAnimation.AnimationState.AddAnimation(0, "skin2_actkeep", true, 0f);
		targetMonster.SetCurHp(0L);
	}

	public void EndEat()
	{
		fightState = 0;
		m_skeletonAnimation.AnimationState.SetAnimation(0, "skin2_actout", false);
		m_skeletonAnimation.AnimationState.AddAnimation(0, "skin2", true, 0f);
	}
}
