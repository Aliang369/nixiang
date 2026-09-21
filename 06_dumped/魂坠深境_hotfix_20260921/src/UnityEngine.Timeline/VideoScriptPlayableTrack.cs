using System;
using UnityEngine.Playables;

namespace UnityEngine.Timeline;

[Serializable]
[TrackClipType(typeof(VideoScriptPlayableAsset))]
[TrackColor(0.008f, 0.698f, 0.655f)]
public class VideoScriptPlayableTrack : TrackAsset
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		PlayableDirector component = go.GetComponent<PlayableDirector>();
		ScriptPlayable<VideoSchedulerPlayableBehaviour> scriptPlayable = ScriptPlayable<VideoSchedulerPlayableBehaviour>.Create(graph, inputCount);
		VideoSchedulerPlayableBehaviour behaviour = scriptPlayable.GetBehaviour();
		if (behaviour != null)
		{
			behaviour.director = component;
			behaviour.clips = ((TrackAsset)this).GetClips();
		}
		return scriptPlayable;
	}
}
