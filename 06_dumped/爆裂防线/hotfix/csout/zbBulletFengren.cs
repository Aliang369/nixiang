using System;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;

public class zbBulletFengren : zbSkillBase
{
	[NonSerialized]
	private int bounceCount;

	[NonSerialized]
	private EWallTapDir lastWallTapDir;

	private zbOBB2D boundsOBB2D;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnInited()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		currPenetration = 0;
		bounceCount = 0;
		lastWallTapDir = EWallTapDir.Empty;
		ignoreMonsters.Clear();
		ReturnToPool(lifetime);
		if (zbCombatSystem.Instance.combatFPS.IsLowPerformanceMode())
		{
			m_skeletonAnimation.UpdateTiming = (UpdateTiming)0;
		}
		else
		{
			m_skeletonAnimation.UpdateTiming = (UpdateTiming)1;
		}
	}

	protected override void Update()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		skillTF.Translate(m_direction * GetAttrValueByType(EAttrSkillType.BulletSpeed) * Time.deltaTime, (Space)0);
		boundsOBB2D.SetPram(Vector2.op_Implicit(skillTF.position), Vector2.op_Implicit(m_direction), 1f * skillTF.localScale.x, 0.2f * skillTF.localScale.x);
		UpdateWallCollision();
		UpdateMonsterCollision();
		if ((int)m_skeletonAnimation.UpdateTiming == 0 && Time.frameCount % zbCombatFPS.MANUAL_UPDATE_FRAME == 0)
		{
			m_skeletonAnimation.Update(Time.deltaTime * (float)zbCombatFPS.MANUAL_UPDATE_FRAME);
		}
	}

	private void UpdateWallCollision()
	{
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		if ((float)bounceCount >= GetAttrValueByType(EAttrSkillType.RicochetCount))
		{
			return;
		}
		bool flag = false;
		if (IsWallCollision(combatData.combatScene.wallLeftBox, EWallTapDir.Left, lastWallTapDir))
		{
			if (m_direction.x < 0f)
			{
				m_direction.x *= -1f;
				ricochetCount++;
				lastWallTapDir = EWallTapDir.Left;
				flag = true;
			}
		}
		else if (IsWallCollision(combatData.combatScene.wallRightBox, EWallTapDir.Right, lastWallTapDir))
		{
			if (m_direction.x > 0f)
			{
				m_direction.x *= -1f;
				ricochetCount++;
				lastWallTapDir = EWallTapDir.Right;
				flag = true;
			}
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

	private bool IsWallCollision(BoxCollider2D wallBox, EWallTapDir wallDir, EWallTapDir lastDir)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (wallDir == lastDir)
		{
			return false;
		}
		return boundsOBB2D.Overlaps(((Collider2D)wallBox).bounds);
	}

	private void UpdateMonsterCollision()
	{
		base.m_combatMonsterGrid.QueryFirst(in boundsOBB2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			ignoreMonsters.Add(overlapResult.GetUUID());
			TakeDamage(overlapResult);
		}
	}

	private void TakeDamage(zbMonster monster)
	{
		if ((float)currPenetration < GetAttrValueByType(EAttrSkillType.Penetration) + 1f)
		{
			CalcMonsterDamage(EAttrSkillType.Damage, monster);
		}
		currPenetration++;
		if ((float)currPenetration >= GetAttrValueByType(EAttrSkillType.Penetration) + 1f)
		{
			ReturnToPool();
		}
	}
}
