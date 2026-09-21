using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class TextSwitcherClip : PlayableAsset, ITimelineClipAsset
{
	public TextSwitcherBehaviour template = new TextSwitcherBehaviour();

	public ClipCaps clipCaps => (ClipCaps)16;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return ScriptPlayable<TextSwitcherBehaviour>.Create(graph, template);
	}
}
