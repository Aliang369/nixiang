using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbMechaSkill10065 : zbMechaSkillBase
{
	private Vector3 targetP;

	private float bulletSpeed = 1f;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private float duration;

	private zbBoundsCircle2D boundsCircle2D;

	private int excludeUUID;

	protected static readonly List<Vector3> targetVector3Up;

	protected static readonly List<Vector3> targetVector3Bm;

	private int subSkillId;

	protected override void OnInited()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		needDurationPre = 0f;
		targetP = Vector2.op_Implicit(new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY));
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		bulletSpeed = mechaSkillRecord.param[2] / 100f;
		float num = mechaSkillRecord.param[3] / 100f;
		((Component)this).transform.localScale = new Vector3(num, num, 1f);
		conitnueNext();
		m_damageIntervalPre = mechaSkillRecord.param[0];
		m_damageInterval = m_damageIntervalPre;
		duration = mechaSkillRecord.param[1];
		skillAttrList[EAttrSkillType.Tow] = mechaSkillRecord.param[4];
		subSkillId = (int)mechaSkillRecord.param[5];
	}

	protected void conitnueNext()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		zbMonster randomMonster = combatData.monsters.GetRandomMonster(excludeUUID);
		if ((Object)(object)randomMonster != (Object)null)
		{
			targetP = ((Component)randomMonster).transform.position;
			excludeUUID = randomMonster.GetUUID();
			return;
		}
		if (((Component)this).transform.position.y >= 0f)
		{
			targetP = targetVector3Bm[Rand.Range(0, 3)];
		}
		else
		{
			targetP = targetVector3Up[Rand.Range(0, 3)];
		}
		excludeUUID = 0;
	}

	private void DelayDestroySelf()
	{
		ReturnToPool();
	}

	protected override void Update()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		needDurationPre += Time.deltaTime;
		if (needDurationPre >= duration)
		{
			needDurationPre = 0f;
			DelayDestroySelf();
		}
		if (zbUtil.Reached(Vector2.op_Implicit(((Component)this).transform.position), Vector2.op_Implicit(targetP)))
		{
			conitnueNext();
			return;
		}
		((Component)this).transform.position = Vector2.op_Implicit(Vector2.MoveTowards(Vector2.op_Implicit(((Component)this).transform.position), Vector2.op_Implicit(targetP), bulletSpeed * Time.deltaTime));
		boundsCircle2D.center.x = ((Component)this).transform.position.x;
		boundsCircle2D.center.y = ((Component)this).transform.position.y;
		boundsCircle2D.radius = ((Component)this).transform.localScale.x / 2f;
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateMonsterCollision();
		}
	}

	private void UpdateMonsterCollision()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		combatData.monsters.monsterGrid.QueryList(in boundsCircle2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, overlapResult, base.damage, mechaSkillRecord.atkType);
			zbSkillSystem.Instance.CreateSubSkill(11302, hero, this, skillAttrList, ((Component)this).transform.position, Vector2.up, overlapResult.GetUUID());
			if (subSkillId > 0)
			{
				zbSkillSystem.Instance.CreateSubSkill(subSkillId, hero, this, skillAttrList, ((Component)overlapResult).transform.position, Vector2.up, overlapResult.GetUUID());
			}
		}
	}

	static zbMechaSkill10065()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		targetVector3Up = new List<Vector3>
		{
			new Vector3(-2f, 5f),
			new Vector3(2f, 5f),
			new Vector3(0f, 5f)
		};
		targetVector3Bm = new List<Vector3>
		{
			new Vector3(-2f, -5f),
			new Vector3(2f, -5f),
			new Vector3(0f, -5f)
		};
	}
}
