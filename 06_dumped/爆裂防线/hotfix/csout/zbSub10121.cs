using System;
using Proto.Enums;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbSub10121 : zbSkillBase
{
	private AudioSource m_audio;

	[NonSerialized]
	public Bone boneShoot;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private float duration;

	protected override void OnInited()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		m_audio = zbAudioMgr.Instance.PlayAudio("audio/combat/jiguang.ogg", loop: true);
		duration = GetAttrValueByType(EAttrSkillType.Duration);
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		boneShoot = ((SkeletonRenderer)m_skeletonAnimation).skeleton.FindBone("shoot");
		zbMonster monsterById = combatData.monsters.GetMonsterById(mainMonsterUUID);
		Vector2 val = default(Vector2);
		if ((Object)(object)monsterById != (Object)null)
		{
			val = Vector2.op_Implicit(monsterById.GetCurPosition());
		}
		else
		{
			((Vector2)(ref val))._002Ector(combatData.nodeRootPositonX, combatData.nodeRootPositonY);
		}
		Vector3 val2 = ((Component)m_skeletonAnimation).transform.InverseTransformPoint(Vector2.op_Implicit(val));
		SkeletonExtensions.SetLocalPosition(boneShoot, val2);
	}

	private void UpdateLaser()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if (!((Object)(object)monsterById == (Object)null))
		{
			Vector2 val = Vector2.op_Implicit(monsterById.GetCurPosition());
			Vector3 val2 = ((Component)m_skeletonAnimation).transform.InverseTransformPoint(Vector2.op_Implicit(val));
			SkeletonExtensions.SetLocalPosition(boneShoot, val2);
		}
	}

	protected override void Update()
	{
		base.Update();
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
			if ((Object)(object)monsterById != (Object)null && monsterById.isCanSelected())
			{
				CalcMonsterDamage(EAttrSkillType.Damage, monsterById);
			}
		}
		zbMonster monsterById2 = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if ((Object)(object)monsterById2 != (Object)null && monsterById2.isCanSelected())
		{
			UpdateLaser();
		}
		else
		{
			zbMonster randomMonster = combatData.monsters.GetRandomMonster();
			if ((Object)(object)randomMonster != (Object)null)
			{
				targetMonsterUUID = randomMonster.GetUUID();
				UpdateLaser();
			}
		}
		needDurationPre += Time.deltaTime;
		if (needDurationPre >= duration)
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
	}
}
