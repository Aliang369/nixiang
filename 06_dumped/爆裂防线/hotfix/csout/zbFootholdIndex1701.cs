using System;
using UnityEngine;

public class zbFootholdIndex1701 : zbFootholdSkillBase
{
	public stFootholdIndex1701 footholdIndex1701Data;

	[NonSerialized]
	private zbBounds2D bounds2D;

	private float m_damageIntervalPre;

	private bool canFight;

	private float m_durationTimePre;

	protected override void OnInited()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		m_sortType = ESortLayerType.BackgroundUp;
		base.OnInited();
		canOutWindow = true;
		m_direction = Vector3.up;
		ReturnToPool(99999f);
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex1701Data = (stFootholdIndex1701)_footholdIndex;
		m_damageIntervalPre = footholdIndex1701Data.damageInterval;
		m_durationTimePre = 0f;
		m_skeletonAnimation.AnimationState.SetAnimation(0, "act", true);
		m_skeletonAnimation.timeScale = 0f;
		bounds2D.center.x = skillTF.position.x;
		bounds2D.center.y = skillTF.position.y;
		bounds2D.halfSize.x = 1.2f;
		bounds2D.halfSize.y = 8.4f;
	}

	public void TriggerDamage()
	{
		canFight = true;
		m_damageIntervalPre = footholdIndex1701Data.damageInterval;
		m_durationTimePre = 0f;
		m_skeletonAnimation.timeScale = footholdIndex1701Data.upSpeed;
	}

	protected override void Update()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (!canFight)
		{
			return;
		}
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= footholdIndex1701Data.damageInterval)
		{
			m_damageIntervalPre -= footholdIndex1701Data.damageInterval;
			base.m_combatMonsterGrid.QueryList(in bounds2D, ignoreMonsters, overlapResultList);
			foreach (zbMonster overlapResult in overlapResultList)
			{
				if (overlapResult.monsterRecord.moveType == 1)
				{
					zbExtraDirection zbExtraDirection2 = new zbExtraDirection();
					zbExtraDirection2.direction = Vector3.up;
					zbExtraDirection2.duration = footholdIndex1701Data.damageInterval;
					zbExtraDirection2.speed = footholdIndex1701Data.upSpeed;
					overlapResult.AddExtraDirectionList(zbExtraDirection2);
				}
			}
		}
		m_durationTimePre += Time.deltaTime;
		if (m_durationTimePre >= footholdIndex1701Data.durationTime)
		{
			canFight = false;
			m_skeletonAnimation.timeScale = 0f;
		}
	}
}
