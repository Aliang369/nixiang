using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbSub11101 : zbSkillBase
{
	private List<int> m_monstersInRange = new List<int>();

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private BoxCollider2D boxCollider2D;

	private GameObject boxColliderObject;

	private float laserWidth = 0.2f;

	protected override void OnInited()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		((Component)this).transform.position = Vector2.op_Implicit(new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY));
		LineRenderer val = ((Component)this).gameObject.GetComponent<LineRenderer>();
		if ((Object)(object)val == (Object)null)
		{
			val = ((Component)this).gameObject.GetComponentInChildren<LineRenderer>();
		}
		boxColliderObject = zbUtil.FindGameObjectRecursive(((Component)this).transform, "BoxColliderObject");
		boxCollider2D = boxColliderObject.GetComponent<BoxCollider2D>();
		laserWidth = GetAttrValueByType(EAttrSkillType.DamageRange);
		val.startWidth = laserWidth;
		val.endWidth = laserWidth;
		val.positionCount = 2;
		Vector3 gunpointPosition = hero.GetGunpointPosition();
		Vector2 val2 = Vector2.op_Implicit(gunpointPosition);
		zbMonster randomMonster = combatData.monsters.GetRandomMonster();
		Vector2 val3 = ((!((Object)(object)randomMonster != (Object)null)) ? Vector2.op_Implicit(zbUtil.GetTargetNormalized(gunpointPosition, new Vector3(Rand.Range(-6f, 6f) + combatData.nodeRootPositonX, 7f + combatData.nodeRootPositonY, 0f))) : Vector2.op_Implicit(zbUtil.GetTargetNormalized(gunpointPosition, randomMonster.GetCurPosition())));
		Vector2 val4 = val2 + val3 * 20f;
		val.SetPosition(0, Vector2.op_Implicit(val2));
		val.SetPosition(1, Vector2.op_Implicit(val4));
		SetBoxCollider2D(val2, val4);
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_monstersInRange.Clear();
		ReturnToPool(GetAttrValueByType(EAttrSkillType.Duration));
	}

	public void SetBoxCollider2D(Vector2 start, Vector2 end)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = end - start;
		float magnitude = ((Vector2)(ref val)).magnitude;
		boxColliderObject.transform.position = Vector2.op_Implicit((start + end) / 2f);
		boxColliderObject.transform.up = Vector2.op_Implicit(((Vector2)(ref val)).normalized);
		boxCollider2D.size = new Vector2(laserWidth, magnitude);
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

	protected override void Update()
	{
		base.Update();
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateMonsterCollision();
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
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			CalcMonsterDamage(EAttrSkillType.Damage, monsterById);
		}
	}
}
