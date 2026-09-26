using System;
using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbMechaSkill10044 : zbMechaSkillBase
{
	[NonSerialized]
	public Bone gunpoint;

	private Vector3 targetMonsterPosition;

	private zbMechaWuli01 mechaWuli01;

	private zbMechaSkillHandleBase skill;

	protected override void OnInited()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		if ((Object)(object)m_skeletonAnimation != (Object)null)
		{
			gunpoint = ((SkeletonRenderer)m_skeletonAnimation).skeleton.FindBone("shootpoint");
		}
		mechaWuli01 = (zbMechaWuli01)hero;
		skill = mechaWuli01.skills[3];
		ignoreMonsters.Clear();
		targetMonsterPosition = Vector2.op_Implicit(new Vector2(Rand.Range(-3f, 3f), Rand.Range(-5f, 6f)));
		ReturnToPool(lifetime);
		zbMonster monsterById = combatData.monsters.GetMonsterById(mainMonsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			targetMonsterPosition = monsterById.GetCurPosition();
		}
		Vector3 val = ((Component)m_skeletonAnimation).transform.InverseTransformPoint(targetMonsterPosition);
		SkeletonExtensions.SetLocalPosition(gunpoint, val);
		((MonoBehaviour)this).StartCoroutine(ShootRepeater());
	}

	public IEnumerator ShootRepeater()
	{
		TrackEntry current = ((Rand.Range(0, 2) != 0) ? m_skeletonAnimation.AnimationState.SetAnimation(0, "animation2", false) : m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false));
		yield return (object)new WaitForSeconds(0.5f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/104_skill4baoza.ogg");
		((zbMechaSkill10044_2)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10044_2.prefab", mechaWuli01, targetMonsterPosition, Vector2.up, 0, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * mechaWuli01.Damage, skill.mechaSkillRecord);
		yield return (object)new WaitForSeconds(current.AnimationEnd - 0.5f);
		ReturnToPool();
	}

	protected override void Update()
	{
		base.Update();
	}
}
