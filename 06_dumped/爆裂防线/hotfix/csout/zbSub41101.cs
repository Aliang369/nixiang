using System.Collections.Generic;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;

public class zbSub41101 : zbSkillBase
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

	private List<Vector3> reflectPoints;

	private zbOBB2D[] bounds2D;

	private Vector2 targetCurrPosition;

	private Vector2 m_lastRayDir;

	private bool m_needRecastRay;

	private Vector2 offsetPosition;

	protected override void OnInited()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		m_audio = zbAudioMgr.Instance.PlayAudio("audio/combat/gaonengshexian.ogg", loop: true);
		duration = GetAttrValueByType(EAttrSkillType.Duration);
		timer = 0f;
		needDurationPre = 0f;
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		turnBack = false;
		hitMonsterTimes.Clear();
		offsetPosition = Vector2.op_Implicit(((Component)this).transform.position);
		if (skillSubRecord.id == 41103)
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
		rayRenderer.SetPosition(0, Vector2.op_Implicit(offsetPosition));
		rayRenderer.SetPosition(1, Vector2.op_Implicit(targetCurrPosition));
		if (skillSubRecord.id == 41101)
		{
			((Renderer)rayRenderer).material = zbAssetLoader.Instance.LoadResouce<Material>("effect/material/shexian2.mat");
		}
		else if (skillSubRecord.id == 41102)
		{
			((Renderer)rayRenderer).material = zbAssetLoader.Instance.LoadResouce<Material>("effect/material/shexian3.mat");
		}
		else
		{
			((Renderer)rayRenderer).material = zbAssetLoader.Instance.LoadResouce<Material>("effect/material/shexian1.mat");
		}
		m_lastRayDir = Vector2.zero;
		m_needRecastRay = true;
		HeroboneShoot(Vector2.op_Implicit(targetCurrPosition));
		CastReflectingRay(offsetPosition, targetCurrPosition);
	}

	protected override void Update()
	{
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (skillSubRecord.id == 41103 && !turnBack)
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
		else if (skillSubRecord.id == 41103 && turnBack)
		{
			timer -= Time.deltaTime;
			float num2 = timer / duration;
			targetCurrPosition = Vector2.Lerp(new Vector2(0f - maxX + combatData.nodeRootPositonX, minY + combatData.nodeRootPositonY), new Vector2(maxX + combatData.nodeRootPositonX, minY + combatData.nodeRootPositonY), num2);
		}
		HeroboneShoot(Vector2.op_Implicit(targetCurrPosition));
		Vector2 val = targetCurrPosition - offsetPosition;
		if (((Vector2)(ref val)).sqrMagnitude > 0.0001f)
		{
			((Vector2)(ref val)).Normalize();
			if (m_needRecastRay || Vector2.Dot(val, m_lastRayDir) < 0.999f)
			{
				CastReflectingRay(offsetPosition, targetCurrPosition);
				m_lastRayDir = val;
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
		num3 = ((skillSubRecord.id != 41103) ? duration : (duration * 2f));
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
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			if (i <= 0 && i < reflectPoints.Count - 1)
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
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		reflectPoints.Clear();
		Vector2 val = end - start;
		Vector2 normalized = ((Vector2)(ref val)).normalized;
		reflectPoints.Add(Vector2.op_Implicit(start));
		Vector2 val2 = normalized;
		Vector2 val3 = start + ((Vector2)(ref val2)).normalized * 20f;
		reflectPoints.Add(Vector2.op_Implicit(val3));
		rayRenderer.positionCount = reflectPoints.Count;
		for (int i = 0; i < reflectPoints.Count; i++)
		{
			rayRenderer.SetPosition(i, reflectPoints[i]);
		}
	}

	public zbSub41101()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		hitMonsterTimes = new Dictionary<int, int>();
		maxX = 17f;
		minY = 7f;
		laserWidth = 0.2f;
		reflectPoints = new List<Vector3>();
		bounds2D = new zbOBB2D[3];
		targetCurrPosition = Vector2.zero;
		m_lastRayDir = Vector2.zero;
		m_needRecastRay = true;
		base._002Ector();
	}
}
