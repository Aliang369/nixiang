using System;
using Proto.Enums;
using UnityEngine;

public class zbFootholdIndex1101 : zbFootholdSkillBase
{
	public stFootholdIndex1101 footholdIndex1101Data;

	private bool canFight = true;

	private bool canMove = true;

	[NonSerialized]
	private zbBoundsCircle2D boundsCircle2D;

	private float m_damageIntervalPre;

	private float m_damageDurationPre;

	private float m_cdPre;

	private bool isMoveUp = true;

	protected override void OnInited()
	{
		m_sortType = ESortLayerType.BurnLayer;
		base.OnInited();
		ignoreMonsters.Clear();
		canFight = false;
		m_skeletonAnimation.AnimationState.SetAnimation(0, "normal_in", false);
		m_skeletonAnimation.AnimationState.AddAnimation(0, "normal_loop", true, 0f);
		ReturnToPool(99999f);
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex1101Data = (stFootholdIndex1101)_footholdIndex;
		((Component)this).transform.localScale = Vector3.one * footholdIndex1101Data.damageRange;
		canFight = false;
		m_damageIntervalPre = 0f;
		m_damageDurationPre = 0f;
		m_cdPre = footholdIndex1101Data.cdTime;
		isMoveUp = true;
		skillAttrList[EAttrSkillType.SlowDown] = footholdIndex1101Data.slowDownPrecent;
		boundsCircle2D.radius = footholdIndex1101Data.damageRange / 2f;
	}

	protected override void Update()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (canMove)
		{
			if (isMoveUp)
			{
				skillTF.Translate(Vector3.up * 0.5f * Time.deltaTime, (Space)0);
			}
			else
			{
				skillTF.Translate(Vector3.down * 0.5f * Time.deltaTime, (Space)0);
			}
			if (skillTF.position.y >= 3f)
			{
				isMoveUp = false;
				((Component)this).transform.localScale = (Vector3.one + 2f * Vector3.down) * footholdIndex1101Data.damageRange;
			}
			if (skillTF.position.y <= -4f)
			{
				isMoveUp = true;
				((Component)this).transform.localScale = Vector3.one * footholdIndex1101Data.damageRange;
			}
		}
		if (!canFight)
		{
			m_cdPre += Time.deltaTime;
			if (m_cdPre >= footholdIndex1101Data.cdTime)
			{
				boundsCircle2D.center.x = skillTF.position.x;
				boundsCircle2D.center.y = skillTF.position.y;
				base.m_combatMonsterGrid.QueryList(in boundsCircle2D, ignoreMonsters, overlapResultList);
				if (overlapResultList.Count > 0)
				{
					m_skeletonAnimation.AnimationState.SetAnimation(0, "normaltoact", false);
					m_skeletonAnimation.AnimationState.AddAnimation(0, "act_loop", true, 0f);
					((Component)this).transform.localScale = Vector3.one * footholdIndex1101Data.damageRange;
					canFight = true;
					canMove = false;
					m_damageDurationPre = 0f;
				}
			}
		}
		if (!canFight)
		{
			return;
		}
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= footholdIndex1101Data.damageInterval)
		{
			m_damageIntervalPre -= footholdIndex1101Data.damageInterval;
			UpdateMonsterCollision();
		}
		m_damageDurationPre += Time.deltaTime;
		if (m_damageDurationPre >= footholdIndex1101Data.durationTime)
		{
			m_cdPre = 0f;
			canFight = false;
			m_skeletonAnimation.AnimationState.SetAnimation(0, "act_out", false);
			m_skeletonAnimation.AnimationState.AddAnimation(0, "normal_loop", true, 0f);
			canMove = true;
			if (isMoveUp)
			{
				((Component)this).transform.localScale = Vector3.one * footholdIndex1101Data.damageRange;
			}
			else
			{
				((Component)this).transform.localScale = (Vector3.one + 2f * Vector3.down) * footholdIndex1101Data.damageRange;
			}
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
		zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, targetMonster, footholdIndex1101Data.damagePercent * (float)(long)targetMonster.maxHp, footholdEffectRecord.atkType, ignoreAttackTypeAdd: true);
		zbCombatSystem.Instance.buffSystem.CreateBuff(this, skillAttrList, targetMonster.GetUUID(), 1, footholdIndex1101Data.slowDownTime);
	}
}
