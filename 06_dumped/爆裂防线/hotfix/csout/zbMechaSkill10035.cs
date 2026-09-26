using System;
using System.Collections.Generic;
using Spine;
using UnityEngine;

public class zbMechaSkill10035 : zbMechaSkillBase
{
	private List<int> m_monstersInRange = new List<int>();

	private AudioSource audioSource;

	[NonSerialized]
	private zbBoundsCircle2D boundsCircle2D;

	[NonSerialized]
	private EWallTapDir lastWallTapDir;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private float moveSpeed;

	protected override void OnInited()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		lastWallTapDir = EWallTapDir.Empty;
		m_skeletonAnimation.AnimationState.SetAnimation(0, "loop", true);
		m_monstersInRange.Clear();
		m_direction = zbUtil.GetTargetNormalized(Vector3.down, Vector3.left);
		needDurationPre = 0f;
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		m_damageIntervalPre = mechaSkillRecord.param[2];
		m_damageInterval = m_damageIntervalPre;
		moveSpeed = mechaSkillRecord.param[3] / 100f;
		ReturnToPool(mechaSkillRecord.param[1] + 20f);
		float num = mechaSkillRecord.param[0] / 100f;
		((Component)this).transform.localScale = new Vector3(num, num, 1f);
	}

	protected void FlyToHero()
	{
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "exit", false);
		ReturnToPool(val.AnimationEnd);
		zbAudioMgr.Instance.StopAudio(audioSource);
	}

	protected override void Update()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (m_monstersInRange.Count > 0)
		{
			((Component)this).transform.Translate(m_direction * moveSpeed / 2f * Time.deltaTime, (Space)0);
		}
		else
		{
			((Component)this).transform.Translate(m_direction * moveSpeed * Time.deltaTime, (Space)0);
		}
		boundsCircle2D.center.x = ((Component)this).transform.position.x;
		boundsCircle2D.center.y = ((Component)this).transform.position.y;
		boundsCircle2D.radius = ((Component)this).transform.localScale.x / 2f;
		UpdateWallCollision();
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateMonsterCollision();
		}
		needDurationPre += Time.deltaTime;
		if (needDurationPre >= mechaSkillRecord.param[1])
		{
			needDurationPre = 0f;
			FlyToHero();
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
		m_monstersInRange.Clear();
		combatData.monsters.monsterGrid.QueryList(in boundsCircle2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			m_monstersInRange.Add(overlapResult.GetUUID());
			TakeDamage(overlapResult);
		}
	}

	private void TakeDamage(zbMonster monster)
	{
		zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, monster, base.damage, mechaSkillRecord.atkType);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		zbAudioMgr.Instance.StopAudio(audioSource);
	}
}
