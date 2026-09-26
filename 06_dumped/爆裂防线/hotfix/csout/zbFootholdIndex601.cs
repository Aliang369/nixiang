using System;
using Proto.Enums;
using UnityEngine;

public class zbFootholdIndex601 : zbFootholdSkillBase
{
	public stFootholdIndex601 footholdIndex601Data;

	[NonSerialized]
	private zbBounds2D bounds2D;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	public Vector3 m_moveUp;

	private bool canFight;

	protected override void OnInited()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		m_sortType = ESortLayerType.Projectile;
		base.OnInited();
		canOutWindow = true;
		m_direction = Vector3.up;
		((Component)this).transform.position = new Vector3(((Component)this).transform.position.x, -5f, ((Component)this).transform.position.z);
		ReturnToPool(99999f);
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex601Data = (stFootholdIndex601)_footholdIndex;
		skillAttrList[EAttrSkillType.Knockback] = footholdIndex601Data.knockbackPercent;
		skillAttrList[EAttrSkillType.MoveSpeed] = 500f;
		m_damageIntervalPre = footholdIndex601Data.damageInterval;
		m_damageInterval = footholdIndex601Data.damageInterval;
	}

	public void TriggerDamage()
	{
		canFight = true;
		m_damageIntervalPre = 0f;
	}

	protected override void Update()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (!canFight)
		{
			return;
		}
		skillTF.Translate(m_moveUp * GetAttrValueByType(EAttrSkillType.MoveSpeed) * Time.deltaTime, (Space)0);
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			bounds2D.center.x = skillTF.position.x;
			bounds2D.center.y = skillTF.position.y;
			bounds2D.halfSize.x = 4.5f;
			bounds2D.halfSize.y = 1f;
			base.m_combatMonsterGrid.QueryList(in bounds2D, ignoreMonsters, overlapResultList);
			foreach (zbMonster overlapResult in overlapResultList)
			{
				zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, overlapResult, footholdIndex601Data.damagePercent * (float)(long)overlapResult.maxHp, footholdEffectRecord.atkType, ignoreAttackTypeAdd: true);
				zbSkillSystem.Instance.CreateSubSkill(10203, hero, this, skillAttrList, Vector3.zero, Vector2.op_Implicit(m_moveUp), overlapResult.GetUUID());
			}
		}
		if (m_moveUp.x < 0f)
		{
			if (skillTF.position.x - combatData.nodeRootPositonX < -18.6f)
			{
				canFight = false;
			}
		}
		else if (skillTF.position.x - combatData.nodeRootPositonX > 18.6f)
		{
			canFight = false;
		}
	}

	public zbFootholdIndex601()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		m_moveUp = Vector3.right;
		base._002Ector();
	}
}
