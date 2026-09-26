using System;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbMechaSkill10054 : zbMechaSkillBase
{
	private float m_damageInterval;

	private float m_damageIntervalPre;

	private zbMechaNengliang01 mechaNengliang01;

	private HashSet<int> m_monstersInRange;

	[NonSerialized]
	public GameObject goRiseEffect;

	public int sortIndex;

	private float timer;

	private float duration;

	private Vector2 targetCurrPosition;

	private bool turnBack;

	private bool bCanFight;

	private float vectorX;

	private int subSkillId;

	public Bone gunpoint { get; set; }

	protected override void OnInited()
	{
		base.OnInited();
		mechaNengliang01 = (zbMechaNengliang01)hero;
		timer = 0f;
		turnBack = false;
		bCanFight = false;
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		m_damageIntervalPre = mechaSkillRecord.param[0];
		m_damageInterval = m_damageIntervalPre;
		duration = mechaSkillRecord.param[1];
		subSkillId = (int)mechaSkillRecord.param[2];
	}

	public void Set10054Param(int index, Bone _gunpoint, GameObject _goRiseEffect)
	{
		sortIndex = index;
		gunpoint = _gunpoint;
		goRiseEffect = _goRiseEffect;
		if (sortIndex == 0 || sortIndex == 1)
		{
			vectorX = 0.5f;
		}
		else
		{
			vectorX = -0.5f;
		}
	}

	public void CanFight(bool isFight)
	{
		bCanFight = isFight;
	}

	protected override void Update()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (!bCanFight)
		{
			return;
		}
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateMonsterCollision();
		}
		if (sortIndex == 4)
		{
			return;
		}
		((Component)this).transform.up = zbUtil.GetTargetNormalized(((Component)this).transform.position, goRiseEffect.transform.position);
		Vector3 val;
		if (!turnBack)
		{
			float num = timer / duration * 2f;
			val = Vector3.Lerp(new Vector3(0f, 1f, 0f), new Vector3(vectorX, 0.5f, 0f), num);
			targetCurrPosition = Vector2.op_Implicit(((Vector3)(ref val)).normalized);
			timer += Time.deltaTime;
			HeroboneShoot(Vector2.op_Implicit(targetCurrPosition));
			if (num >= 1f)
			{
				turnBack = true;
				timer = 0f;
			}
		}
		if (turnBack)
		{
			float num2 = timer / duration * 2f;
			val = Vector3.Lerp(new Vector3(vectorX, 0.5f, 0f), new Vector3(0f, 1f, 0f), num2);
			targetCurrPosition = Vector2.op_Implicit(((Vector3)(ref val)).normalized);
			timer += Time.deltaTime;
			HeroboneShoot(Vector2.op_Implicit(targetCurrPosition));
			if (num2 >= 1f)
			{
				turnBack = false;
				timer = 0f;
			}
		}
	}

	private void HeroboneShoot(Vector3 targetPosition)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		SkeletonExtensions.SetLocalPosition(gunpoint, ((Component)this).transform.position + targetPosition * 10f);
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null)
		{
			m_monstersInRange.Add(component.GetUUID());
		}
	}

	private void OnTriggerExit2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null)
		{
			m_monstersInRange.Remove(component.GetUUID());
		}
	}

	private void UpdateMonsterCollision()
	{
		foreach (int item in m_monstersInRange)
		{
			ShootFormulaMonster(item);
		}
	}

	private void ShootFormulaMonster(int monsterUUID)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, monsterById, base.damage, mechaSkillRecord.atkType);
			if (subSkillId > 0)
			{
				zbSkillSystem.Instance.CreateSubSkill(subSkillId, mechaNengliang01, this, skillAttrList, ((Component)monsterById).transform.position, Vector2.up, monsterById.GetUUID());
			}
		}
	}

	public zbMechaSkill10054()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		m_monstersInRange = new HashSet<int>();
		targetCurrPosition = Vector2.zero;
		base._002Ector();
	}
}
