using Proto.Enums;
using Spine.Unity;
using UnityEngine;

public class zbSub40701 : zbSkillBase
{
	protected override void OnInited()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		if ((Object)(object)m_skeletonAnimation != (Object)null)
		{
			m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false);
			if (skillSubRecord.id == 40701)
			{
				SkeletonExtensions.SetColor(((SkeletonRenderer)m_skeletonAnimation).skeleton, Color.yellow);
			}
			else
			{
				SkeletonExtensions.SetColor(((SkeletonRenderer)m_skeletonAnimation).skeleton, Color.clear);
			}
		}
		zbMonster monsterById = combatData.monsters.GetMonsterById(mainMonsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			((Component)this).transform.position = ((Component)monsterById).transform.position;
			CalcMonsterDamage(EAttrSkillType.Damage, monsterById);
		}
		ReturnToPool(0.4f);
	}
}
