using UnityEngine;
using UnityEngine.Playables;

public class EmptyPlayableAsset : PlayableAsset
{
	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return default(Playable);
	}
}
