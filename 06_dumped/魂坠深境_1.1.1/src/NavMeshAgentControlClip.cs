using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class NavMeshAgentControlClip : PlayableAsset, ITimelineClipAsset
{
	public ExposedReference<Transform> destination;

	[HideInInspector]
	public NavMeshAgentControlBehaviour template = new NavMeshAgentControlBehaviour();

	public ClipCaps clipCaps => (ClipCaps)0;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<NavMeshAgentControlBehaviour> scriptPlayable = ScriptPlayable<NavMeshAgentControlBehaviour>.Create(graph, template);
		scriptPlayable.GetBehaviour().destination = destination.Resolve(graph.GetResolver());
		return scriptPlayable;
	}
}
