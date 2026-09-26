using Proto.Enums;
using Spine;
using UnityEngine;

public class zbSub10702 : zbSkillBase
{
	protected override void OnInited()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false);
		ReturnToPool(val.AnimationEnd);
		zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if (!((Object)(object)monsterById != (Object)null))
		{
			return;
		}
		((Component)this).transform.position = ((Component)monsterById).transform.position;
		if (CalcMonsterDamage(EAttrSkillType.Damage, monsterById).isKilled)
		{
			combatData.monsters.GetRandomMonsterList(1, monsterRandomList, monsterById.GetUUID());
			if (monsterRandomList.Count > 0)
			{
				zbSkillSystem.Instance.CreateSubSkill(skillSubRecord.id, hero, this, skillAttrList, ((Component)monsterRandomList[0]).transform.position, Vector2.up, 0, monsterRandomList[0].GetUUID());
			}
		}
	}
}
