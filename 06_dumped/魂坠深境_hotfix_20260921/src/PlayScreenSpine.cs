using System.Collections;
using Spine.Unity;
using UnityEngine;

public class PlayScreenSpine : CutSceneNode
{
	public AnimationReferenceAsset animation;

	public bool halt;

	public bool loop;

	public override IEnumerator Play(PlayCutSceneState state)
	{
		((Component)(object)state.CutsceneOverlaySpineGraphic).gameObject.SetActive(value: true);
		state.CutsceneOverlaySpineGraphic.skeletonDataAsset = animation.SkeletonDataAsset;
		state.CutsceneOverlaySpineGraphic.Initialize(true);
		state.CutsceneOverlaySpineGraphic.PlayAnimation(AnimationReferenceAsset.op_Implicit(animation), loop).Complete += delegate
		{
			((Component)(object)state.CutsceneOverlaySpineGraphic).gameObject.SetActive(value: false);
		};
		if (!loop && halt)
		{
			yield return new WaitUntil(() => !((Component)(object)state.CutsceneOverlaySpineGraphic).gameObject.activeSelf);
		}
	}
}
