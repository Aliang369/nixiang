using System;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbMechaSkill10015 : zbMechaSkillBase
{
	[NonSerialized]
	private Bone[] gunpoint = (Bone[])(object)new Bone[5];

	private AudioSource m_audio;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	[NonSerialized]
	private GameObject[] goRiseEffect = (GameObject[])(object)new GameObject[5];

	[NonSerialized]
	private float bulletSpeed = 16f;

	private int[] targetMonsterUUIDList = new int[5];

	private int targetMaxNum;

	private float lastdeltaTime;

	protected override void OnInited()
	{
		base.OnInited();
		if ((Object)(object)m_skeletonAnimation != (Object)null)
		{
			gunpoint[0] = ((SkeletonRenderer)m_skeletonAnimation).skeleton.FindBone("gunpoint");
			gunpoint[1] = ((SkeletonRenderer)m_skeletonAnimation).skeleton.FindBone("gunpoint_2");
			gunpoint[2] = ((SkeletonRenderer)m_skeletonAnimation).skeleton.FindBone("gunpoint_3");
			gunpoint[3] = ((SkeletonRenderer)m_skeletonAnimation).skeleton.FindBone("gunpoint_4");
			gunpoint[4] = ((SkeletonRenderer)m_skeletonAnimation).skeleton.FindBone("gunpoint_5");
		}
		for (int i = 0; i < 5; i++)
		{
			goRiseEffect[i] = ((Component)((Component)this).transform.Find("gunpoint_" + (i + 1))).gameObject;
		}
		m_audio = null;
		m_audio = zbAudioMgr.Instance.PlayAudio("audio/mecha/103_3.ogg", loop: true);
		for (int j = 0; j < targetMaxNum; j++)
		{
			if (j == 0)
			{
				zbMonster closestMonster = combatData.monsters.GetClosestMonster(10f);
				if ((Object)(object)closestMonster != (Object)null)
				{
					UpdateLaser(j, closestMonster);
				}
			}
			else
			{
				zbMonster randomMonster = combatData.monsters.GetRandomMonster();
				if ((Object)(object)randomMonster != (Object)null)
				{
					UpdateLaser(j, randomMonster);
				}
			}
		}
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		m_damageIntervalPre = mechaSkillRecord.param[2];
		m_damageInterval = m_damageIntervalPre;
		needDurationPre = 0f;
		bulletSpeed = mechaSkillRecord.param[0] / 100f;
		targetMaxNum = (int)mechaSkillRecord.param[1];
	}

	private void UpdateLaser(int index, zbMonster targetMonster)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < index; i++)
		{
			if (targetMonsterUUIDList[i] == targetMonster.GetUUID())
			{
				SkeletonExtensions.SetLocalPosition(gunpoint[index], Vector2.zero);
				return;
			}
		}
		Vector2 val = Vector2.op_Implicit(targetMonster.GetCurPosition());
		Vector3 val2 = ((Component)m_skeletonAnimation).transform.InverseTransformPoint(Vector2.op_Implicit(val));
		SkeletonExtensions.SetLocalPosition(gunpoint[index], val2);
	}

	protected override void Update()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		lastdeltaTime = Time.deltaTime;
		((Component)this).transform.Translate(Vector3.up * bulletSpeed * Time.deltaTime, (Space)0);
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			ShootFormulaMonster();
		}
		for (int i = 0; i < targetMaxNum; i++)
		{
			zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUIDList[i]);
			if ((Object)(object)monsterById != (Object)null)
			{
				UpdateLaser(i, monsterById);
				continue;
			}
			zbMonster closestMonsterByVector = combatData.monsters.GetClosestMonsterByVector(goRiseEffect[i].transform.position);
			if ((Object)(object)closestMonsterByVector != (Object)null)
			{
				targetMonsterUUIDList[i] = closestMonsterByVector.GetUUID();
				UpdateLaser(i, closestMonsterByVector);
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		StopLoopAudio();
	}

	private void StopLoopAudio()
	{
		if (Object.op_Implicit((Object)(object)m_audio))
		{
			zbAudioMgr.Instance.StopAudio(m_audio);
			m_audio = null;
		}
	}

	private void ShootFormulaMonster()
	{
		for (int i = 0; i < targetMaxNum; i++)
		{
			zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUIDList[i]);
			if ((Object)(object)monsterById != (Object)null)
			{
				zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, monsterById, base.damage, mechaSkillRecord.atkType);
			}
		}
	}
}
