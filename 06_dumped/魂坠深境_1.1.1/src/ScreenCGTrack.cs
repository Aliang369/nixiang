using System.ComponentModel;
using Spine.Unity.Playables;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(1f, 0.2509804f, 0.003921569f)]
[TrackClipType(typeof(SpineAnimationStateClip))]
[TrackBindingType(typeof(Cutscene))]
[DisplayName("屏幕SpineCG动画轨道")]
public class ScreenCGTrack : SpineAnimationStateGraphicTrack
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		foreach (TimelineClip clip in ((TrackAsset)this).GetClips())
		{
			Object asset = clip.asset;
			SpineAnimationStateClip val = (SpineAnimationStateClip)(object)((asset is SpineAnimationStateClip) ? asset : null);
			if ((Object)(object)val != null)
			{
				val.timelineClip = clip;
			}
		}
		ScriptPlayable<ScreenCGMixerBehaviour> scriptPlayable = ScriptPlayable<ScreenCGMixerBehaviour>.Create(graph, inputCount);
		ScreenCGMixerBehaviour behaviour = scriptPlayable.GetBehaviour();
		((SpineAnimationStateMixerBehaviour)behaviour).trackIndex = base.trackIndex;
		((SpineAnimationStateMixerBehaviour)behaviour).unscaledTime = base.unscaledTime;
		return scriptPlayable;
	}
}
