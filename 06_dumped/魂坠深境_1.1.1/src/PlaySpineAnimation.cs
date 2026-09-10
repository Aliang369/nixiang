using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;

public class PlaySpineAnimation : CutSceneNode
{
	public string animationName;

	public bool halt;

	public bool loop;

	public SkeletonAnimation skeletonAnimation;

	public override IEnumerator Play(PlayCutSceneState state)
	{
		if (!((Object)(object)skeletonAnimation != null))
		{
			yield break;
		}
		if (loop)
		{
			skeletonAnimation.PlayAnimation(animationName, loop: true);
			yield break;
		}
		skeletonAnimation.PlayAnimation(animationName, loop: false);
		if (halt)
		{
			yield return new WaitUntil(() => skeletonAnimation.AnimationState.GetCurrent(0).IsComplete);
		}
	}

	public List<string> GetAllAnimationNames()
	{
		List<string> list = new List<string>();
		if ((Object)(object)skeletonAnimation != null)
		{
			foreach (Animation animation in ((SkeletonRenderer)skeletonAnimation).Skeleton.Data.Animations)
			{
				list.Add(animation.Name);
			}
		}
		return list;
	}
}
