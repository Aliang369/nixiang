using System.Collections.Generic;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;

public class zbLineJiguang : zbSkillBase
{
	private float laserWidth = 0.17f;

	private LineRenderer lineRenderer;

	private AudioSource m_audio;

	private zbBulletJiguang bulletJiguang;

	private List<int> m_monstersInRange;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	protected override void OnInited()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		((Component)this).transform.position = Vector2.op_Implicit(new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY));
		targetMonsterUUID = mainMonsterUUID;
		lineRenderer = ((Component)this).gameObject.GetComponent<LineRenderer>();
		if ((Object)(object)lineRenderer == (Object)null)
		{
			lineRenderer = ((Component)this).gameObject.GetComponentInChildren<LineRenderer>();
		}
		lineRenderer.startWidth = laserWidth;
		lineRenderer.endWidth = laserWidth;
		lineRenderer.positionCount = 2;
		((Renderer)lineRenderer).material = zbAssetLoader.Instance.LoadResouce<Material>("effect/material/" + hero.skillRecord.bulletSpineName + ".mat");
		m_audio = null;
		bulletJiguang = (zbBulletJiguang)zbSkillSystem.Instance.CreateSkill("prefab/combat/hero/jiguang/bulletJiguang.prefab", hero, ((Component)this).transform.position, Vector2.up);
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		needDurationPre = 0f;
		m_monstersInRange = new List<int>();
		m_audio = zbAudioMgr.Instance.PlayAudio("audio/combat/jiguang.ogg", loop: true);
	}

	private void UpdateLaser()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if (!((Object)(object)monsterById == (Object)null))
		{
			Vector2 val = Vector2.op_Implicit(monsterById.GetCurPosition());
			Vector3 val2 = ((Component)hero.anim.skeletonAnimation).transform.InverseTransformPoint(Vector2.op_Implicit(val));
			if (hero.boneShoot != null)
			{
				SkeletonExtensions.SetLocalPosition(hero.boneShoot, val2);
			}
			bulletJiguang.mainMonsterUUID = targetMonsterUUID;
			Vector2 val3 = Vector2.op_Implicit(hero.GetGunpointPosition());
			((Component)bulletJiguang).transform.position = Vector2.op_Implicit(val);
			lineRenderer.SetPosition(0, Vector2.op_Implicit(val3));
			lineRenderer.SetPosition(1, Vector2.op_Implicit(val));
		}
	}

	protected override void Update()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateMonsterCollision();
		}
		zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if ((Object)(object)monsterById != (Object)null && monsterById.isCanSelected())
		{
			UpdateLaser();
		}
		else
		{
			zbMonster closestMonsterByVector = combatData.monsters.GetClosestMonsterByVector(lineRenderer.GetPosition(1));
			if ((Object)(object)closestMonsterByVector != (Object)null)
			{
				targetMonsterUUID = closestMonsterByVector.GetUUID();
				UpdateLaser();
			}
		}
		needDurationPre += Time.deltaTime;
		if (needDurationPre >= GetAttrValueByType(EAttrSkillType.Duration) + hero.needDurationProgressMaxTemp)
		{
			needDurationPre = 0f;
			DelayDestroySelf();
		}
	}

	private void StopLoopAudio()
	{
		if (Object.op_Implicit((Object)(object)m_audio))
		{
			zbAudioMgr.Instance.StopAudio(m_audio);
			m_audio = null;
		}
	}

	private void DelayDestroySelf()
	{
		StopLoopAudio();
		ReturnToPool();
		bulletJiguang.ReturnToPool();
		hero.SetIdleAnimation(loop: true);
		hero.anim.skeletonAnimation.AnimationState.AddEmptyAnimation(1, 0f, 0f);
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null && component.GetUUID() != bulletJiguang.mainMonsterUUID)
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
