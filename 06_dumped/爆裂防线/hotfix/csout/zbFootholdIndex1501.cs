using System;
using Proto.Enums;
using UnityEngine;

public class zbFootholdIndex1501 : zbFootholdSkillBase
{
	public stFootholdIndex1501 footholdIndex1501Data;

	[NonSerialized]
	private zbBounds2D bounds2D;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private bool canMove;

	protected override void OnInited()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		m_sortType = ESortLayerType.Zhuangjiache;
		base.OnInited();
		ignoreMonsters.Clear();
		canMove = false;
		((Component)this).transform.position = new Vector3(((Component)this).transform.position.x, -20f + combatData.nodeRootPositonY, ((Component)this).transform.position.z);
		canOutWindow = true;
		ReturnToPool(99999f);
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex1501Data = (stFootholdIndex1501)_footholdIndex;
		skillAttrList[EAttrSkillType.Knockback] = footholdIndex1501Data.carKnockbackPercent;
		skillAttrList[EAttrSkillType.MoveSpeed] = footholdIndex1501Data.moveSpeed;
		m_damageIntervalPre = footholdIndex1501Data.carDamageInterval;
		m_damageInterval = footholdIndex1501Data.carDamageInterval;
	}

	public void TriggerDamage()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		canMove = true;
		((Component)this).transform.position = new Vector3(((Component)this).transform.position.x, -8f + combatData.nodeRootPositonY, ((Component)this).transform.position.z);
	}

	protected override void Update()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (!canMove)
		{
			return;
		}
		skillTF.Translate(m_direction * GetAttrValueByType(EAttrSkillType.MoveSpeed) * Time.deltaTime, (Space)0);
		m_damageIntervalPre += Time.deltaTime;
		if (!(m_damageIntervalPre >= m_damageInterval))
		{
			return;
		}
		m_damageIntervalPre -= m_damageInterval;
		bounds2D.center.x = skillTF.position.x;
		bounds2D.center.y = skillTF.position.y - 0.2f;
		bounds2D.halfSize.x = 1.5f * skillTF.localScale.x / 2f;
		bounds2D.halfSize.y = 2.5f * skillTF.localScale.y / 2f;
		base.m_combatMonsterGrid.QueryList(in bounds2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, overlapResult, footholdIndex1501Data.carDamagePercent * (float)(long)overlapResult.maxHp, footholdEffectRecord.atkType, ignoreAttackTypeAdd: true);
			zbSkillSystem.Instance.CreateSubSkill(10203, hero, this, skillAttrList, Vector3.zero, Vector2.op_Implicit(m_direction), overlapResult.GetUUID());
		}
	}
}
