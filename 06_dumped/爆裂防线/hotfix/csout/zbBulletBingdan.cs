using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbBulletBingdan : zbSkillBase
{
	[NonSerialized]
	private int bounceCount;

	[NonSerialized]
	private static float boxRadius = 0.15f;

	[NonSerialized]
	private zbBounds2D bounds2D;

	[NonSerialized]
	private EWallTapDir lastWallTapDir;

	protected override void OnInited()
	{
		base.OnInited();
		currPenetration = 0;
		bounceCount = 0;
		lastWallTapDir = EWallTapDir.Empty;
		ignoreMonsters.Clear();
		ReturnToPool(lifetime);
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
		base.Update();
		skillTF.Translate(m_direction * GetAttrValueByType(EAttrSkillType.BulletSpeed) * Time.deltaTime, (Space)0);
		bounds2D.center.x = skillTF.position.x;
		bounds2D.center.y = skillTF.position.y;
		bounds2D.halfSize.x = boxRadius * skillTF.localScale.x;
		bounds2D.halfSize.y = boxRadius * skillTF.localScale.y;
		UpdateWallCollision();
		UpdateMonsterCollision();
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
		combatData.monsters.monsterGrid.QueryFirst(in bounds2D, ignoreMonsters, overlapResultList);
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
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		if ((float)currPenetration < GetAttrValueByType(EAttrSkillType.Penetration) + 1f && CalcMonsterDamage(EAttrSkillType.Damage, monster).damageValue > 0 && hero.skillRecord.id == 40002 && hero.team.ExtraProperties[ETeamExtra.BingdanMod2143] > 0f)
		{
			combatData.monsters.GetRandomMonsterList(10, monsterRandomList, monster.GetUUID());
			for (int i = 0; (float)i < hero.team.ExtraProperties[ETeamExtra.BingdanMod2143]; i++)
			{
				zbSkillSystem.Instance.CreateSkill("prefab/combat/hero/bingdan/bulletBingdanZimu.prefab", hero, ((Component)this).transform.position, Vector2.up, monster.GetUUID(), (monsterRandomList.Count > i) ? monsterRandomList[i].GetUUID() : 0);
			}
		}
		currPenetration++;
		if ((float)currPenetration >= GetAttrValueByType(EAttrSkillType.Penetration) + 1f)
		{
			ReturnToPool();
		}
	}
}
