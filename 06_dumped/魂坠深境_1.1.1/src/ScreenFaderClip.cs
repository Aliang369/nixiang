using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class ScreenFaderClip : PlayableAsset, ITimelineClipAsset
{
	public ScreenFaderBehaviour template = new ScreenFaderBehaviour();

	public ClipCaps clipCaps => (ClipCaps)16;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return ScriptPlayable<ScreenFaderBehaviour>.Create(graph, template);
	}
}
