using Spine.Unity;
using Spine.Unity.Playables;
using UnityEngine;
using UnityEngine.Playables;

public class ScreenCGMixerBehaviour : SpineAnimationStateMixerBehaviour
{
	private SkeletonGraphic graphic;

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		if (!(playerData is Cutscene { currentPlayState: not null } cutscene))
		{
			return;
		}
		if ((Object)(object)graphic == null)
		{
			graphic = cutscene.currentPlayState.CutsceneOverlaySpineGraphic;
		}
		bool flag = false;
		for (int i = 0; i < playable.GetInputCount(); i++)
		{
			bool num = playable.GetInputWeight(i) > 0f;
			SpineAnimationStateBehaviour behaviour = ((ScriptPlayable<SpineAnimationStateBehaviour>)playable.GetInput(i)).GetBehaviour();
			if (num && (Object)(object)behaviour?.animationReference != null)
			{
				((Component)(object)graphic).gameObject.SetActive(value: true);
				if ((Object)(object)behaviour.animationReference.SkeletonDataAsset != (Object)(object)graphic.skeletonDataAsset)
				{
					graphic.skeletonDataAsset = behaviour.animationReference.SkeletonDataAsset;
					graphic.Initialize(true);
				}
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			((Component)(object)graphic).gameObject.SetActive(value: false);
		}
		((SpineAnimationStateMixerBehaviour)this).ProcessFrame(playable, info, (object)cutscene.currentPlayState.CutsceneOverlaySpineGraphic);
	}

	public override void OnGraphStop(Playable playable)
	{
		if ((Object)(object)graphic != null)
		{
			((Component)(object)graphic).gameObject.SetActive(value: false);
		}
		((SpineAnimationStateMixerBehaviour)this).OnGraphStop(playable);
	}
}
