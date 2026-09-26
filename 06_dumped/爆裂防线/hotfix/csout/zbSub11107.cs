using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbSub11107 : zbSkillBase
{
	private LineRenderer rayRenderer;

	private List<int> m_monstersInRange;

	private float targetPositionX;

	private float duration;

	private float timer;

	private Vector2 laserStart;

	private bool turnBack;

	private float maxX = 4f;

	private readonly List<Vector2> _rayPoints = new List<Vector2>();

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private BoxCollider2D boxCollider2D;

	private GameObject boxColliderObject;

	private float laserWidth = 0.2f;

	protected override void OnInited()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		boxColliderObject = zbUtil.FindGameObjectRecursive(((Component)this).transform, "BoxColliderObject");
		boxCollider2D = boxColliderObject.GetComponent<BoxCollider2D>();
		duration = GetAttrValueByType(EAttrSkillType.Duration);
		timer = 0f;
		((Component)this).transform.position = Vector2.op_Implicit(new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY));
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		turnBack = false;
		m_monstersInRange = new List<int>();
		Vector3 gunpointPosition = hero.GetGunpointPosition();
		Vector2 val = Vector2.op_Implicit(zbUtil.GetTargetNormalized(gunpointPosition, new Vector3(Rand.Range(0f - maxX, maxX) + combatData.nodeRootPositonX, 7f + combatData.nodeRootPositonY, 0f)));
		targetPositionX = 0f - maxX + combatData.nodeRootPositonX;
		val = Vector2.op_Implicit(zbUtil.GetTargetNormalized(gunpointPosition, new Vector3(targetPositionX, 7f + combatData.nodeRootPositonY, 0f)));
		rayRenderer = ((Component)this).gameObject.GetComponent<LineRenderer>();
		if ((Object)(object)rayRenderer == (Object)null)
		{
			rayRenderer = ((Component)this).gameObject.GetComponentInChildren<LineRenderer>();
		}
		rayRenderer.startWidth = GetAttrValueByType(EAttrSkillType.DamageRange);
		rayRenderer.endWidth = GetAttrValueByType(EAttrSkillType.DamageRange);
		rayRenderer.positionCount = 2;
		laserStart = Vector2.op_Implicit(gunpointPosition);
		Vector2 val2 = laserStart + val * 20f;
		rayRenderer.SetPosition(0, Vector2.op_Implicit(laserStart));
		rayRenderer.SetPosition(1, Vector2.op_Implicit(val2));
		_rayPoints.Clear();
		_rayPoints.Add(laserStart);
		_rayPoints.Add(laserStart + val * 20f);
		SetBoxCollider2D(laserStart, val2);
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

	protected override void Update()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (!turnBack)
		{
			timer += Time.deltaTime;
			float num = timer / duration;
			targetPositionX = Mathf.Lerp(0f - maxX, maxX, num);
			Vector2 val = Vector2.op_Implicit(zbUtil.GetTargetNormalized(Vector2.op_Implicit(laserStart), new Vector3(targetPositionX, 7f, 0f)));
			Vector2 val2 = laserStart + val * 20f;
			rayRenderer.SetPosition(1, Vector2.op_Implicit(val2));
			_rayPoints.Clear();
			_rayPoints.Add(laserStart);
			_rayPoints.Add(laserStart + val * 20f);
			SetBoxCollider2D(laserStart, val2);
			if (num > 0.999f)
			{
				turnBack = true;
			}
		}
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateMonsterCollision();
		}
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
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			CalcMonsterDamage(EAttrSkillType.Damage, monsterById);
		}
	}
}
