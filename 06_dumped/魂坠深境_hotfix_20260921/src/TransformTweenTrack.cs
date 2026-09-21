using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.855f, 0.8623f, 0.87f)]
[TrackClipType(typeof(TransformTweenClip))]
[TrackBindingType(typeof(Transform))]
public class TransformTweenTrack : TrackAsset
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		return ScriptPlayable<TransformTweenMixerBehaviour>.Create(graph, inputCount);
	}

	public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
	{
		((TrackAsset)this).GatherProperties(director, driver);
	}
}
