using System;
using System.Collections.Generic;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;

public class zbBulletShexian : zbSkillBase
{
	private LineRenderer rayRenderer;

	private AudioSource m_audio;

	private Dictionary<int, int> hitMonsterTimes;

	private float duration;

	private float timer;

	private bool turnBack;

	private float maxX;

	private float minY;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private float laserWidth;

	private int maxReflections;

	private float minReflectX;

	private float maxReflectX;

	private float minReflectY;

	private float maxReflectY;

	private List<Vector3> reflectPoints;

	private zbOBB2D[] bounds2D;

	private Vector2 targetCurrPosition;

	private Vector2 m_lastRayDir;

	private bool m_needRecastRay;

	protected override void OnInited()
	{
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		StopLoopAudio();
		m_audio = zbAudioMgr.Instance.PlayAudio("audio/combat/gaonengshexian.ogg", loop: true);
		duration = GetAttrValueByType(EAttrSkillType.Duration);
		timer = 0f;
		needDurationPre = 0f;
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		turnBack = false;
		hitMonsterTimes.Clear();
		minReflectX = -3.5f + combatData.nodeRootPositonX;
		maxReflectX = 3.5f + combatData.nodeRootPositonX;
		minReflectY = -6f + combatData.nodeRootPositonY;
		maxReflectY = 8.4f + combatData.nodeRootPositonY;
		maxReflections = (int)Math.Round(hero.team.ExtraProperties[ETeamExtra.GemMod2114]);
		Vector2 val = Vector2.op_Implicit(hero.GetGunpointPosition());
		if (hero.team.ExtraProperties[ETeamExtra.ShexianMod2111] > 0f)
		{
			targetCurrPosition = Vector2.op_Implicit(new Vector3(0f - maxX + combatData.nodeRootPositonX, minY + combatData.nodeRootPositonY, 0f));
		}
		else
		{
			zbMonster monsterById = combatData.monsters.GetMonsterById(mainMonsterUUID);
			if ((Object)(object)monsterById != (Object)null)
			{
				targetCurrPosition = Vector2.op_Implicit(monsterById.GetCurPosition());
			}
			else
			{
				targetCurrPosition = Vector2.op_Implicit(new Vector3(Rand.Range(0f - maxX, maxX) + combatData.nodeRootPositonX, minY + combatData.nodeRootPositonY, 0f));
			}
		}
		rayRenderer = ((Component)this).gameObject.GetComponent<LineRenderer>();
		if ((Object)(object)rayRenderer == (Object)null)
		{
			rayRenderer = ((Component)this).gameObject.GetComponentInChildren<LineRenderer>();
		}
		rayRenderer.alignment = (LineAlignment)0;
		laserWidth = GetAttrValueByType(EAttrSkillType.DamageRange);
		rayRenderer.startWidth = laserWidth;
		rayRenderer.endWidth = laserWidth;
		rayRenderer.positionCount = 2;
		rayRenderer.SetPosition(0, Vector2.op_Implicit(val));
		rayRenderer.SetPosition(1, Vector2.op_Implicit(targetCurrPosition));
		((Renderer)rayRenderer).material = zbAssetLoader.Instance.LoadResouce<Material>("effect/material/" + hero.skillRecord.bulletSpineName + ".mat");
		m_lastRayDir = new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY);
		m_needRecastRay = true;
		HeroboneShoot(Vector2.op_Implicit(targetCurrPosition));
		CastReflectingRay(val, targetCurrPosition);
	}

	protected override void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		Vector2 val = Vector2.op_Implicit(hero.GetGunpointPosition());
		if (hero.team.ExtraProperties[ETeamExtra.ShexianMod2111] > 0f && !turnBack)
		{
			timer += Time.deltaTime;
			float num = timer / duration;
			targetCurrPosition = Vector2.Lerp(new Vector2(0f - maxX + combatData.nodeRootPositonX, minY + combatData.nodeRootPositonY), new Vector2(maxX + combatData.nodeRootPositonX, minY + combatData.nodeRootPositonY), num);
			if (num > 0.999f)
			{
				turnBack = true;
				m_needRecastRay = true;
			}
		}
		else if (hero.team.ExtraProperties[ETeamExtra.ShexianMod2112] > 0f && turnBack)
		{
			timer -= Time.deltaTime;
			float num2 = timer / duration;
			targetCurrPosition = Vector2.Lerp(new Vector2(0f - maxX + combatData.nodeRootPositonX, minY + combatData.nodeRootPositonY), new Vector2(maxX + combatData.nodeRootPositonX, minY + combatData.nodeRootPositonY), num2);
		}
		HeroboneShoot(Vector2.op_Implicit(targetCurrPosition));
		Vector2 val2 = targetCurrPosition - val;
		if (((Vector2)(ref val2)).sqrMagnitude > 0.0001f)
		{
			((Vector2)(ref val2)).Normalize();
			if (m_needRecastRay || Vector2.Dot(val2, m_lastRayDir) < 0.999f)
			{
				CastReflectingRay(val, targetCurrPosition);
				m_lastRayDir = val2;
				m_needRecastRay = false;
			}
		}
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			CollisionCooldown();
		}
		float num3 = 0f;
		num3 = ((!(hero.team.ExtraProperties[ETeamExtra.ShexianMod2112] > 0f)) ? duration : (duration * 2f));
		needDurationPre += Time.deltaTime;
		if (needDurationPre >= num3 + hero.needDurationProgressMaxTemp)
		{
			needDurationPre = 0f;
			DelayDestroySelf();
		}
	}

	private void HeroboneShoot(Vector3 targetPosition)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)hero.anim.skeletonAnimation).transform.InverseTransformPoint(targetPosition);
		SkeletonExtensions.SetLocalPosition(hero.boneShoot, val);
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
		hero.SetIdleAnimation(loop: true);
		hero.anim.skeletonAnimation.AnimationState.SetEmptyAnimation(1, 0f);
	}

	private void CollisionCooldown()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			if (i <= maxReflections && i < reflectPoints.Count - 1)
			{
				bounds2D[i].SetPram(Vector2.op_Implicit(reflectPoints[i]), Vector2.op_Implicit(reflectPoints[i + 1]), laserWidth);
			}
			combatData.monsters.monsterGrid.QueryList(in bounds2D[i], ignoreMonsters, overlapResultList);
			foreach (zbMonster overlapResult in overlapResultList)
			{
				ShootFormulaMonster(overlapResult);
			}
		}
	}

	private void ShootFormulaMonster(zbMonster targetMonster)
	{
		if ((Object)(object)targetMonster != (Object)null)
		{
			CalcMonsterDamage(EAttrSkillType.Damage, targetMonster);
			if (hitMonsterTimes.TryGetValue(targetMonster.GetUUID(), out var value))
			{
				hitMonsterTimes[targetMonster.GetUUID()] = value + 1;
			}
			else
			{
				hitMonsterTimes[targetMonster.GetUUID()] = 1;
			}
		}
	}

	public override float GetExtraAddRatio(zbMonster monster)
	{
		float num = 1f;
		if (hitMonsterTimes.TryGetValue(monster.GetUUID(), out var value))
		{
			for (int i = 0; i < value; i++)
			{
				num += hero.team.ExtraProperties[ETeamExtra.ShexianMod2113];
			}
		}
		return num;
	}

	private void CastReflectingRay(Vector2 start, Vector2 end)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		reflectPoints.Clear();
		Vector2 val = start;
		Vector2 val2 = end - start;
		Vector2 normalized = ((Vector2)(ref val2)).normalized;
		reflectPoints.Add(Vector2.op_Implicit(val));
		Vector2 reflectedDir = normalized;
		Vector2 hitPoint = val;
		for (int i = 0; i < maxReflections; i++)
		{
			CastToBounds(val, reflectedDir, out hitPoint, out reflectedDir);
			reflectPoints.Add(Vector2.op_Implicit(hitPoint));
			val = hitPoint;
		}
		Vector2 val3 = hitPoint + ((Vector2)(ref reflectedDir)).normalized * 20f;
		reflectPoints.Add(Vector2.op_Implicit(val3));
		rayRenderer.positionCount = reflectPoints.Count;
		for (int j = 0; j < reflectPoints.Count; j++)
		{
			rayRenderer.SetPosition(j, reflectPoints[j]);
		}
	}

	private void CastToBounds(Vector2 origin, Vector2 dir, out Vector2 hitPoint, out Vector2 reflectedDir)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		float num = 100f;
		Vector2 val = (hitPoint = origin + ((Vector2)(ref dir)).normalized * num);
		reflectedDir = dir;
		float num2 = float.MaxValue;
		Vector2 val2 = val;
		Vector2 val3 = Vector2.zero;
		if (dir.y > 0f)
		{
			float num3 = (maxReflectY - origin.y) / dir.y;
			Vector2 val4 = origin + dir * num3;
			if (val4.x >= minReflectX && val4.x <= maxReflectX && num3 < num2)
			{
				num2 = num3;
				val2 = val4;
				val3 = Vector2.down;
			}
		}
		if (dir.y < 0f)
		{
			float num4 = (minReflectY - origin.y) / dir.y;
			Vector2 val5 = origin + dir * num4;
			if (val5.x >= minReflectX && val5.x <= maxReflectX && num4 < num2)
			{
				num2 = num4;
				val2 = val5;
				val3 = Vector2.up;
			}
		}
		if (dir.x > 0f)
		{
			float num5 = (maxReflectX - origin.x) / dir.x;
			Vector2 val6 = origin + dir * num5;
			if (val6.y >= minReflectY && val6.y <= maxReflectY && num5 < num2)
			{
				num2 = num5;
				val2 = val6;
				val3 = Vector2.left;
			}
		}
		if (dir.x < 0f)
		{
			float num6 = (minReflectX - origin.x) / dir.x;
			Vector2 val7 = origin + dir * num6;
			if (val7.y >= minReflectY && val7.y <= maxReflectY && num6 < num2)
			{
				num2 = num6;
				val2 = val7;
				val3 = Vector2.right;
			}
		}
		hitPoint = val2;
		reflectedDir = Vector2.Reflect(dir, val3);
	}

	public zbBulletShexian()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		hitMonsterTimes = new Dictionary<int, int>();
		maxX = 17f;
		minY = 7f;
		laserWidth = 0.2f;
		minReflectX = -3.5f;
		maxReflectX = 3.5f;
		minReflectY = -6f;
		maxReflectY = 8.4f;
		reflectPoints = new List<Vector3>();
		bounds2D = new zbOBB2D[3];
		targetCurrPosition = Vector2.zero;
		m_lastRayDir = Vector2.zero;
		m_needRecastRay = true;
		base._002Ector();
	}
}
