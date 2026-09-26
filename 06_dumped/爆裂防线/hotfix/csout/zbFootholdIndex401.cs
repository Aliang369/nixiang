using System;
using Proto.Enums;
using UnityEngine;

public class zbFootholdIndex401 : zbFootholdSkillBase
{
	public stFootholdIndex401 footholdIndex401Data;

	[NonSerialized]
	private zbBounds2D bounds2D;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private bool canMove;

	protected override void OnInited()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		m_direction = Vector3.up;
		canMove = false;
		((Component)this).transform.position = new Vector3(((Component)this).transform.position.x, -5f + combatData.nodeRootPositonY, ((Component)this).transform.position.z);
		m_skeletonAnimation.AnimationState.SetAnimation(0, "in", false);
		m_skeletonAnimation.AnimationState.AddAnimation(0, "animation", true, 0f);
		ReturnToPool(99999f);
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex401Data = (stFootholdIndex401)_footholdIndex;
		((Component)this).transform.localScale = Vector3.one * footholdIndex401Data.damageRange;
		skillAttrList[EAttrSkillType.Knockback] = footholdIndex401Data.knockbackPercent;
		skillAttrList[EAttrSkillType.MoveSpeed] = footholdIndex401Data.moveSpeed;
		m_damageIntervalPre = footholdIndex401Data.damageInterval;
		m_damageInterval = footholdIndex401Data.damageInterval;
	}

	protected override void Update()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (canMove)
		{
			skillTF.Translate(m_direction * GetAttrValueByType(EAttrSkillType.MoveSpeed) * Time.deltaTime, (Space)0);
		}
		m_damageIntervalPre += Time.deltaTime;
		if (!(m_damageIntervalPre >= m_damageInterval))
		{
			return;
		}
		m_damageIntervalPre -= m_damageInterval;
		bounds2D.center.x = skillTF.position.x;
		bounds2D.center.y = skillTF.position.y;
		bounds2D.halfSize.x = 1.4f * skillTF.localScale.x / 2f;
		bounds2D.halfSize.y = 2.5f * skillTF.localScale.y / 2f;
		base.m_combatMonsterGrid.QueryList(in bounds2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, overlapResult, footholdIndex401Data.damagePercent * (float)(long)overlapResult.maxHp, footholdEffectRecord.atkType, ignoreAttackTypeAdd: true);
			zbSkillSystem.Instance.CreateSubSkill(10203, hero, this, skillAttrList, Vector3.zero, Vector2.op_Implicit(m_direction), overlapResult.GetUUID());
			canMove = true;
		}
	}
}
