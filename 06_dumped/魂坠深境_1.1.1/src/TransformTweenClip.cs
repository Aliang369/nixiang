using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class TransformTweenClip : PlayableAsset, ITimelineClipAsset
{
	public TransformTweenBehaviour template = new TransformTweenBehaviour();

	public ExposedReference<Transform> startLocation;

	public ExposedReference<Transform> endLocation;

	public ClipCaps clipCaps => (ClipCaps)16;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<TransformTweenBehaviour> scriptPlayable = ScriptPlayable<TransformTweenBehaviour>.Create(graph, template);
		TransformTweenBehaviour behaviour = scriptPlayable.GetBehaviour();
		behaviour.startLocation = startLocation.Resolve(graph.GetResolver());
		behaviour.endLocation = endLocation.Resolve(graph.GetResolver());
		return scriptPlayable;
	}
}
