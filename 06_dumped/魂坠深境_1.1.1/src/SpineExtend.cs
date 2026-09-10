using Spine;
using Spine.Unity;
using UnityEngine;

public static class SpineExtend
{
	public static TrackEntry PlayAnimation(this SkeletonAnimation skeletonAnimation, string animName, bool loop)
	{
		if (((SkeletonRenderer)(skeletonAnimation?)).skeleton == null)
		{
			return null;
		}
		((SkeletonRenderer)skeletonAnimation).skeleton.SetToSetupPose();
		skeletonAnimation.AnimationState.ClearTracks();
		TrackEntry result = skeletonAnimation.AnimationState.SetAnimation(0, animName, loop);
		skeletonAnimation.AnimationState.Apply(((SkeletonRenderer)skeletonAnimation).skeleton);
		skeletonAnimation.AnimationState.Update(0f);
		return result;
	}

	public static TrackEntry PlayAnimation(this SkeletonAnimation skeletonAnimation, Animation anim, bool loop)
	{
		((SkeletonRenderer)skeletonAnimation).skeleton.SetToSetupPose();
		skeletonAnimation.AnimationState.ClearTracks();
		TrackEntry result = skeletonAnimation.AnimationState.SetAnimation(0, anim, loop);
		skeletonAnimation.AnimationState.Apply(((SkeletonRenderer)skeletonAnimation).skeleton);
		skeletonAnimation.AnimationState.Update(0f);
		return result;
	}

	public static TrackEntry PlayAnimation(this SkeletonGraphic skeletonGraphic, string animName, bool loop)
	{
		skeletonGraphic.Skeleton.SetToSetupPose();
		skeletonGraphic.AnimationState.ClearTracks();
		TrackEntry result = skeletonGraphic.AnimationState.SetAnimation(0, animName, loop);
		skeletonGraphic.AnimationState.Apply(skeletonGraphic.Skeleton);
		skeletonGraphic.AnimationState.Update(0f);
		return result;
	}

	public static TrackEntry PlayAnimation(this SkeletonGraphic skeletonGraphic, Animation anim, bool loop)
	{
		skeletonGraphic.Skeleton.SetToSetupPose();
		skeletonGraphic.AnimationState.ClearTracks();
		TrackEntry result = skeletonGraphic.AnimationState.SetAnimation(0, anim, loop);
		skeletonGraphic.AnimationState.Apply(skeletonGraphic.Skeleton);
		skeletonGraphic.AnimationState.Update(0f);
		return result;
	}

	public static Animation GetCurrentAnimation(this SkeletonGraphic skeletonGraphic)
	{
		if ((Object)(object)skeletonGraphic == null || skeletonGraphic.AnimationState == null)
		{
			return null;
		}
		return skeletonGraphic.AnimationState.GetCurrent(0)?.Animation;
	}
}
