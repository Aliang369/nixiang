using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class PlayDialogueClip : PlayableAsset, ITimelineClipAsset
{
	[TextArea(3, 10)]
	[LocalizeText("对话")]
	public string text;

	public bool progress;

	public PlayDialogueBehaviour template = new PlayDialogueBehaviour();

	public ClipCaps clipCaps => (ClipCaps)0;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<PlayDialogueBehaviour> scriptPlayable = ScriptPlayable<PlayDialogueBehaviour>.Create(graph, template);
		PlayDialogueBehaviour behaviour = scriptPlayable.GetBehaviour();
		behaviour.fullText = text;
		behaviour.progress = progress;
		return scriptPlayable;
	}
}
