using System;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;

public class zbSub40801 : zbSkillBase
{
	private int m_monstersNumsInRange;

	private Vector3 srcPosition;

	private Vector3 targetPosition;

	private int wurenjiState;

	private float moveTime;

	private float comeBackTime;

	private AudioSource audioSource;

	[NonSerialized]
	private zbBoundsCircle2D boundsCircle2D;

	[NonSerialized]
	private EWallTapDir lastWallTapDir;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private float m_skillDuration;

	private float m_skillMoveSpeed;

	protected override void OnInited()
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		wurenjiState = 0;
		moveTime = 0f;
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_skillDuration = GetAttrValueByType(EAttrSkillType.Duration);
		m_skillMoveSpeed = GetAttrValueByType(EAttrSkillType.MoveSpeed);
		lastWallTapDir = EWallTapDir.Empty;
		ReturnToPool(m_skillDuration + 20f);
		if (skillSubRecord.id == 40801)
		{
			SkeletonExtensions.SetColor(((SkeletonRenderer)m_skeletonAnimation).skeleton, Color.blue);
		}
		else if (skillSubRecord.id == 40802)
		{
			SkeletonExtensions.SetColor(((SkeletonRenderer)m_skeletonAnimation).skeleton, new Color(1f, 0.5f, 0f));
		}
		else if (skillSubRecord.id == 40803)
		{
			SkeletonExtensions.SetColor(((SkeletonRenderer)m_skeletonAnimation).skeleton, Color.clear);
			m_skeletonAnimation.AnimationState.SetAnimation(1, "fire", true);
		}
		m_skeletonAnimation.AnimationState.SetAnimation(0, "small", true);
		m_skeletonAnimation.AnimationState.SetEmptyAnimation(1, 0f);
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		srcPosition = ((Component)this).transform.position;
		m_monstersNumsInRange = 0;
		zbMonster monsterById = combatData.monsters.GetMonsterById(mainMonsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			targetPosition = monsterById.GetCurPosition();
		}
		else
		{
			targetPosition = Vector2.op_Implicit(new Vector2(3f, 0f));
		}
		m_direction = zbUtil.GetTargetNormalized(((Component)this).transform.position, targetPosition);
		hero.needDurationProgress = m_skillDuration + comeBackTime * 2f;
		hero.needDurationProgressMax = hero.needDurationProgress;
		hero.needDurationProgressMaxTemp = 0f;
		needDurationPre = 0f;
		audioSource = zbAudioMgr.Instance.PlayAudio("audio/combat/wurenjichongji.ogg", loop: true);
	}

	protected void FlyToHero()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		targetPosition = ((Component)this).transform.position;
		m_skeletonAnimation.AnimationState.SetAnimation(0, "small", true);
		m_skeletonAnimation.AnimationState.SetEmptyAnimation(1, 0f);
		wurenjiState = 2;
		combatData.monsters.GetRandomMonsterList(10, monsterRandomList);
		if (skillRecord == null || !hero.team.mainEndToSubSkillBySkill.TryGetValue(8, out var value))
		{
			return;
		}
		foreach (stMainEndToSubSkill item in value)
		{
			for (int i = 0; i < item.subCount; i++)
			{
				zbSkillSystem.Instance.CreateSubSkill(item.subSkillId, hero, this, skillAttrList, ((Component)this).transform.position, Vector2.up, 0, (monsterRandomList.Count > i) ? monsterRandomList[i].GetUUID() : 0);
			}
		}
	}

	protected override void Update()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (wurenjiState == 0 && moveTime <= comeBackTime)
		{
			moveTime += Time.deltaTime;
			float num = Mathf.Clamp01(moveTime / comeBackTime);
			skillTF.position = Vector2.op_Implicit(Vector2.Lerp(Vector2.op_Implicit(srcPosition), Vector2.op_Implicit(targetPosition), num));
			if (moveTime >= comeBackTime)
			{
				m_skeletonAnimation.AnimationState.SetAnimation(0, "big", true);
				wurenjiState = 1;
				moveTime = 0f;
			}
		}
		if (wurenjiState == 2 && moveTime <= comeBackTime)
		{
			moveTime += Time.deltaTime;
			float num2 = Mathf.Clamp01(moveTime / comeBackTime);
			skillTF.position = Vector2.op_Implicit(Vector2.Lerp(Vector2.op_Implicit(targetPosition), Vector2.op_Implicit(((Component)hero).transform.position), num2));
			if (moveTime >= comeBackTime)
			{
				wurenjiState = 3;
				ReturnToPool();
				hero.SetIdleAnimation(loop: true);
				zbAudioMgr.Instance.StopAudio(audioSource);
			}
		}
		if (wurenjiState == 1)
		{
			if (m_monstersNumsInRange > 0)
			{
				skillTF.Translate(m_direction * m_skillMoveSpeed / 2f * Time.deltaTime, (Space)0);
			}
			else
			{
				skillTF.Translate(m_direction * m_skillMoveSpeed * Time.deltaTime, (Space)0);
			}
			boundsCircle2D.center.x = skillTF.position.x;
			boundsCircle2D.center.y = skillTF.position.y;
			boundsCircle2D.radius = skillTF.localScale.x / 2f;
			UpdateWallCollision();
			m_damageIntervalPre += Time.deltaTime;
			if (m_damageIntervalPre >= m_damageInterval)
			{
				m_damageIntervalPre -= m_damageInterval;
				UpdateMonsterCollision();
			}
			needDurationPre += Time.deltaTime;
			if (needDurationPre >= m_skillDuration + hero.needDurationProgressMaxTemp)
			{
				needDurationPre = 0f;
				FlyToHero();
			}
		}
	}

	private void UpdateWallCollision()
	{
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		if (IsWallCollision(combatData.combatScene.wallLeftBox, EWallTapDir.Left, lastWallTapDir))
		{
			if (Mathf.Abs(m_direction.y) < 0.15f)
			{
				if (m_direction.y < 0f)
				{
					m_direction.y = -0.2f;
				}
				else
				{
					m_direction.y = 0.2f;
				}
				m_direction = ((Vector3)(ref m_direction)).normalized;
			}
			m_direction.x *= -1f;
			ricochetCount++;
			lastWallTapDir = EWallTapDir.Left;
		}
		else if (IsWallCollision(combatData.combatScene.wallRightBox, EWallTapDir.Right, lastWallTapDir))
		{
			if (Mathf.Abs(m_direction.y) < 0.15f)
			{
				if (m_direction.y < 0f)
				{
					m_direction.y = -0.2f;
				}
				else
				{
					m_direction.y = 0.2f;
				}
				m_direction = ((Vector3)(ref m_direction)).normalized;
			}
			m_direction.x *= -1f;
			ricochetCount++;
			lastWallTapDir = EWallTapDir.Right;
		}
		else if (IsWallCollision(combatData.combatScene.wallTopBox, EWallTapDir.Top, lastWallTapDir))
		{
			if (Mathf.Abs(m_direction.x) < 0.15f)
			{
				if (m_direction.x < 0f)
				{
					m_direction.x = -0.2f;
				}
				else
				{
					m_direction.x = 0.2f;
				}
				m_direction = ((Vector3)(ref m_direction)).normalized;
			}
			m_direction.y *= -1f;
			ricochetCount++;
			lastWallTapDir = EWallTapDir.Top;
		}
		else if (IsWallCollision(combatData.combatScene.wallBottomBox, EWallTapDir.Bottom, lastWallTapDir) && m_direction.y < 0f)
		{
			if (Mathf.Abs(m_direction.x) < 0.15f)
			{
				if (m_direction.x < 0f)
				{
					m_direction.x = -0.2f;
				}
				else
				{
					m_direction.x = 0.2f;
				}
				m_direction = ((Vector3)(ref m_direction)).normalized;
			}
			m_direction.y *= -1f;
			ricochetCount++;
			lastWallTapDir = EWallTapDir.Bottom;
		}
		if (false)
		{
			SetDirection(Vector2.op_Implicit(m_direction));
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

	private void UpdateMonsterCollision()
	{
		m_monstersNumsInRange = 0;
		combatData.monsters.monsterGrid.QueryList(in boundsCircle2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			m_monstersNumsInRange++;
			TakeDamage(overlapResult);
		}
	}

	private void TakeDamage(zbMonster monster)
	{
		CalcMonsterDamage(EAttrSkillType.Damage, monster);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		zbAudioMgr.Instance.StopAudio(audioSource);
	}

	public zbSub40801()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		srcPosition = new Vector3(0f, 0f, 0f);
		targetPosition = new Vector3(0f, 0f, 0f);
		comeBackTime = 0.5f;
		base._002Ector();
	}
}
