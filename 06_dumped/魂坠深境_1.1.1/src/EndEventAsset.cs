using UnityEngine;
using UnityEngine.Playables;

public class EndEventAsset : PlayableAsset
{
	private EndEventBehaviour behaviour;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		behaviour = new EndEventBehaviour();
		return ScriptPlayable<EndEventBehaviour>.Create(graph, behaviour);
	}
}
