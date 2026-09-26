using Proto.Enums;
using UnityEngine;

public class zbSub11201 : zbSkillBase
{
	protected override void OnInited()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		if ((Object)(object)m_skeletonAnimation != (Object)null)
		{
			m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false);
		}
		zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			((Component)this).transform.position = monsterById.GetCurPosition();
			CalcMonsterDamage(EAttrSkillType.Damage, monsterById);
		}
		ReturnToPool(0.4f);
	}
}
