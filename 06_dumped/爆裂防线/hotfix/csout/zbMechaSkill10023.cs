using System;
using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbMechaSkill10023 : zbMechaSkillBase
{
	[NonSerialized]
	public Bone gunpoint;

	private Vector3 targetMonsterPosition;

	private zbMechaHuo01 mechaHuo01;

	protected override void OnInited()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		if ((Object)(object)m_skeletonAnimation != (Object)null)
		{
			gunpoint = ((SkeletonRenderer)m_skeletonAnimation).skeleton.FindBone("shoot");
		}
		mechaHuo01 = (zbMechaHuo01)hero;
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
		TrackEntry current = m_skeletonAnimation.AnimationState.SetAnimation(0, "zidan", false);
		yield return (object)new WaitForSeconds(0.5f);
		((zbMechaSkill10023_2)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10023_2.prefab", mechaHuo01, targetMonsterPosition, Vector2.up, 0, 0, mechaHuo01.skills[2].SkillId)).SetMechaAtr(mechaHuo01.skills[2].damageRate * mechaHuo01.Damage, mechaHuo01.skills[2].mechaSkillRecord);
		yield return (object)new WaitForSeconds(current.AnimationEnd - 0.5f);
		ReturnToPool();
	}

	protected override void Update()
	{
		base.Update();
	}
}
