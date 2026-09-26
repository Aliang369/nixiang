using Proto.Enums;
using UnityEngine;

public class zbSub10002 : zbSkillBase
{
	protected override void OnInited()
	{
		base.OnInited();
		zbMonster monsterById = combatData.monsters.GetMonsterById(mainMonsterUUID);
		if ((Object)(object)monsterById != (Object)null && !monsterById.isEliteBoss)
		{
			m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false);
			CalcMonsterDamage(EAttrSkillType.Damage, monsterById);
			ReturnToPool(0.4f);
		}
		else
		{
			ReturnToPool(0.1f);
		}
	}
}
