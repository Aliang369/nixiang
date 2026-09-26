using System;
using System.Collections.Generic;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;

public class zbBulletQiang : zbSkillBase
{
	[NonSerialized]
	private int bounceCount;

	[NonSerialized]
	private EWallTapDir lastWallTapDir;

	[NonSerialized]
	private static float boxRadius = 0.06f;

	[NonSerialized]
	private zbBounds2D bounds2D;

	private bool isFirstDamage = true;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnInited()
	{
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		currPenetration = 0;
		bounceCount = 0;
		isFirstDamage = true;
		m_skeletonAnimation.AnimationState.SetAnimation(0, hero.skillRecord.flyEffectId, true);
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
		bool hasFetchedMonsters = false;
		foreach (stModIndex2014 item in hero.team.modIndex2014)
		{
			item.tickNumber++;
			if (item.tickNumber >= item.releaseNumber)
			{
				EnsureFetchMonsters();
				for (int i = 0; i < item.subCount; i++)
				{
					zbSkillSystem.Instance.CreateSubSkill(item.subSkillId, hero, null, skillAttrList, Vector2.op_Implicit(new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY)), Vector2.up, 0, (monsterRandomList.Count > i) ? monsterRandomList[i].GetUUID() : 0);
				}
				item.tickNumber = 0;
			}
		}
		void EnsureFetchMonsters()
		{
			if (!hasFetchedMonsters)
			{
				combatData.monsters.GetRandomMonsterList(10, monsterRandomList, mainMonsterUUID);
				hasFetchedMonsters = true;
			}
		}
	}

	protected override void Update()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		skillTF.Translate(m_direction * GetAttrValueByType(EAttrSkillType.BulletSpeed) * Time.deltaTime, (Space)0);
		bounds2D.center.x = skillTF.position.x;
		bounds2D.center.y = skillTF.position.y;
		bounds2D.halfSize.x = boxRadius * skillTF.localScale.x;
		bounds2D.halfSize.y = boxRadius * skillTF.localScale.y;
		UpdateWallCollision();
		UpdateMonsterCollision();
		if ((int)m_skeletonAnimation.UpdateTiming == 0 && Time.frameCount % zbCombatFPS.MANUAL_UPDATE_FRAME == 0)
		{
			m_skeletonAnimation.Update(Time.deltaTime * (float)zbCombatFPS.MANUAL_UPDATE_FRAME);
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
		return bounds2D.Overlaps(((Collider2D)wallBox).bounds);
	}

	private void UpdateMonsterCollision()
	{
		base.m_combatMonsterGrid.QueryFirst(in bounds2D, ignoreMonsters, overlapResultList);
		using List<zbMonster>.Enumerator enumerator = overlapResultList.GetEnumerator();
		if (enumerator.MoveNext())
		{
			zbMonster current = enumerator.Current;
			ignoreMonsters.Add(current.GetUUID());
			TakeDamage(current);
		}
	}

	private void TakeDamage(zbMonster monster)
	{
		if ((float)currPenetration < GetAttrValueByType(EAttrSkillType.Penetration) + 1f)
		{
			stDamageResult stDamageResult2 = CalcMonsterDamage(EAttrSkillType.Damage, monster);
			if (isFirstDamage && stDamageResult2.damageValue > 0 && hero.team.trackIndex == 1 && stDamageResult2.damageType == EDamageResult.Hit && zbCombatSystem.Instance.qiangFirstMonsterId == monster.GetTid())
			{
				if (zbCombatSystem.Instance.qiangHurtMin == 0L || zbCombatSystem.Instance.qiangHurtMin > stDamageResult2.damageValue)
				{
					zbCombatSystem.Instance.qiangHurtMin = stDamageResult2.damageValue;
				}
				if (zbCombatSystem.Instance.qiangHurtMax < stDamageResult2.damageValue)
				{
					zbCombatSystem.Instance.qiangHurtMax = stDamageResult2.damageValue;
				}
			}
		}
		currPenetration++;
		if ((float)currPenetration >= GetAttrValueByType(EAttrSkillType.Penetration) + 1f)
		{
			ReturnToPool();
		}
	}
}
