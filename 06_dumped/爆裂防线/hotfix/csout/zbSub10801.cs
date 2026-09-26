using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbSub10801 : zbSkillBase
{
	[NonSerialized]
	private int bounceCount;

	[NonSerialized]
	private EWallTapDir lastWallTapDir;

	private List<int> m_monstersInRange;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private zbBoundsCircle2D boundsCircle2D;

	protected override void OnInited()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			m_direction = zbUtil.GetTargetNormalized(((Component)this).transform.position, monsterById.GetCurPosition());
		}
		else
		{
			Vector2 insideUnitCircle = Rand.insideUnitCircle;
			m_direction = Vector2.op_Implicit(((Vector2)(ref insideUnitCircle)).normalized);
		}
		m_monstersInRange = new List<int>();
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		ReturnToPool(GetAttrValueByType(EAttrSkillType.Duration));
	}

	protected override void Update()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (m_monstersInRange.Count > 0)
		{
			skillTF.Translate(m_direction * GetAttrValueByType(EAttrSkillType.MoveSpeed) / 2f * Time.deltaTime, (Space)0);
		}
		else
		{
			skillTF.Translate(m_direction * GetAttrValueByType(EAttrSkillType.MoveSpeed) * Time.deltaTime, (Space)0);
		}
		boundsCircle2D.center.x = skillTF.position.x;
		boundsCircle2D.center.y = skillTF.position.y;
		boundsCircle2D.radius = skillTF.localScale.x / 2f;
		UpdateWallCollision();
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateMonsterCollision(boundsCircle2D, ignoreMonsters, EAttrSkillType.Damage);
		}
	}

	private void UpdateWallCollision()
	{
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		if (!((float)bounceCount >= GetAttrValueByType(EAttrSkillType.RicochetCount)))
		{
			bool flag = false;
			if (IsWallCollision(combatData.combatScene.wallLeftBox, EWallTapDir.Left, lastWallTapDir))
			{
				m_direction.x *= -1f;
				ricochetCount++;
				lastWallTapDir = EWallTapDir.Left;
				flag = true;
			}
			else if (IsWallCollision(combatData.combatScene.wallRightBox, EWallTapDir.Right, lastWallTapDir))
			{
				m_direction.x *= -1f;
				ricochetCount++;
				lastWallTapDir = EWallTapDir.Right;
				flag = true;
			}
			else if (IsWallCollision(combatData.combatScene.wallTopBox, EWallTapDir.Top, lastWallTapDir))
			{
				m_direction.y *= -1f;
				ricochetCount++;
				lastWallTapDir = EWallTapDir.Top;
				flag = true;
			}
			else if (IsWallCollision(combatData.combatScene.wallBottomBox, EWallTapDir.Bottom, lastWallTapDir) && m_direction.y < 0f)
			{
				m_direction.y *= -1f;
				ricochetCount++;
				lastWallTapDir = EWallTapDir.Bottom;
				flag = true;
			}
			if (flag)
			{
				ignoreMonsters.Clear();
				SetDirection(Vector2.op_Implicit(m_direction));
				bounceCount++;
			}
		}
	}

	private bool IsWallCollision(BoxCollider2D wallBox, EWallTapDir wallDir, EWallTapDir lastDir)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (wallDir == lastDir)
		{
			return false;
		}
		return boundsCircle2D.Overlaps(((Collider2D)wallBox).bounds);
	}
}
