using Spine;
using UnityEngine;

public class zbMechaSkill10045 : zbMechaSkillBase
{
	private zbMechaWuli01 mechaWuli01;

	protected override void OnInited()
	{
		base.OnInited();
		mechaWuli01 = (zbMechaWuli01)hero;
		m_sortType = ESortLayerType.HeroFront;
		zbSortMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false);
		ReturnToPool(val.AnimationEnd);
	}
}
