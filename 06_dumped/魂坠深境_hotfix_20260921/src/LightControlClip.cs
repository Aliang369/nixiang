using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class LightControlClip : PlayableAsset, ITimelineClipAsset
{
	public LightControlBehaviour template = new LightControlBehaviour();

	public ClipCaps clipCaps => (ClipCaps)16;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return ScriptPlayable<LightControlBehaviour>.Create(graph, template);
	}
}
