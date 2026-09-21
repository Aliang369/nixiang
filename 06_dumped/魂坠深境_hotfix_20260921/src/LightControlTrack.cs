using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.9454092f, 0.9779412f, 0.3883002f)]
[TrackClipType(typeof(LightControlClip))]
[TrackBindingType(typeof(Light))]
public class LightControlTrack : TrackAsset
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		return ScriptPlayable<LightControlMixerBehaviour>.Create(graph, inputCount);
	}

	public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
	{
		((TrackAsset)this).GatherProperties(director, driver);
	}
}
