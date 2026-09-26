public class zbMonsterSkillIndex10002 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		PlayTackAnimation(1, flag: false);
		monster.anim.spineAnimation.skeletonAnimation.AnimationState.AddEmptyAnimation(1, 0.05f, 0f);
	}
}
