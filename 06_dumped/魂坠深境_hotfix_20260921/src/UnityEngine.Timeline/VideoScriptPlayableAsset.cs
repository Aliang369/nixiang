using System;
using UnityEngine.Playables;
using UnityEngine.Video;

namespace UnityEngine.Timeline;

[Serializable]
public class VideoScriptPlayableAsset : PlayableAsset
{
	public ExposedReference<VideoPlayer> videoPlayer;

	[SerializeField]
	[NotKeyable]
	public VideoClip videoClip;

	[SerializeField]
	[NotKeyable]
	public bool mute;

	[SerializeField]
	[NotKeyable]
	public bool loop = true;

	[SerializeField]
	[NotKeyable]
	public double preloadTime = 0.3;

	[SerializeField]
	[NotKeyable]
	public double clipInTime;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
	{
		ScriptPlayable<VideoPlayableBehaviour> scriptPlayable = ScriptPlayable<VideoPlayableBehaviour>.Create(graph);
		VideoPlayableBehaviour behaviour = scriptPlayable.GetBehaviour();
		behaviour.videoPlayer = videoPlayer.Resolve(graph.GetResolver());
		behaviour.videoClip = videoClip;
		behaviour.mute = mute;
		behaviour.loop = loop;
		behaviour.preloadTime = preloadTime;
		behaviour.clipInTime = clipInTime;
		return scriptPlayable;
	}
}
