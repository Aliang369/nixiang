using System;
using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbMechaSkill10031 : zbMechaSkillBase
{
	[NonSerialized]
	public Bone gunpoint;

	private Vector3 targetMonsterPosition;

	private zbMechaBing01 mechaBing01;

	private zbMechaSkillHandleBase skill;

	[NonSerialized]
	public GameObject goRiseEffect_1;

	protected override void OnInited()
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		m_sortType = ESortLayerType.HeroFront;
		base.OnInited();
		if ((Object)(object)m_skeletonAnimation != (Object)null)
		{
			gunpoint = ((SkeletonRenderer)m_skeletonAnimation).skeleton.FindBone("shootpoint");
		}
		goRiseEffect_1 = ((Component)((Component)this).transform.Find("goRiseEffect_1")).gameObject;
		mechaBing01 = (zbMechaBing01)hero;
		skill = mechaBing01.skills[0];
		ignoreMonsters.Clear();
		targetMonsterPosition = ((Component)this).transform.position;
		ReturnToPool(lifetime);
		zbMonster monsterById = combatData.monsters.GetMonsterById(mainMonsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			targetMonsterPosition = monsterById.GetCurPosition();
			Vector3 val = ((Component)m_skeletonAnimation).transform.InverseTransformPoint(targetMonsterPosition);
			SkeletonExtensions.SetLocalPosition(gunpoint, val);
		}
		((MonoBehaviour)this).StartCoroutine(ShootRepeater());
	}

	public IEnumerator ShootRepeater()
	{
		TrackEntry current = m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false);
		yield return (object)new WaitForSeconds(5f / 6f);
		((zbMechaSkill10031_2)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10031_2.prefab", mechaBing01, targetMonsterPosition, Vector2.up, 0, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * mechaBing01.Damage, skill.mechaSkillRecord);
		yield return (object)new WaitForSeconds(current.AnimationEnd - 5f / 6f);
		ReturnToPool();
	}

	protected override void Update()
	{
		base.Update();
	}
}
