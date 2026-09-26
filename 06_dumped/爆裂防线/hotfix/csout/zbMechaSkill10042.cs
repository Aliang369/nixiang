using Spine;

public class zbMechaSkill10042 : zbMechaSkillBase
{
	protected override void OnInited()
	{
		m_sortType = ESortLayerType.HeroFront;
		base.OnInited();
		ignoreMonsters.Clear();
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false);
		if (val != null)
		{
			ReturnToPool(val.AnimationEnd);
		}
		else
		{
			ReturnToPool(2.1333334f);
		}
	}
}
