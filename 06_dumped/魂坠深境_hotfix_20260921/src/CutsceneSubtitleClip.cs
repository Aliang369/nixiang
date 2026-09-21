using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CutsceneSubtitleClip : PlayableAsset, ITimelineClipAsset
{
	[TextArea(3, 10)]
	[LocalizeText("CG演出字幕")]
	public string text;

	public CutsceneSubtitleBehaviour template = new CutsceneSubtitleBehaviour();

	public ClipCaps clipCaps => (ClipCaps)0;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<CutsceneSubtitleBehaviour> scriptPlayable = ScriptPlayable<CutsceneSubtitleBehaviour>.Create(graph, template);
		scriptPlayable.GetBehaviour().text = text;
		return scriptPlayable;
	}
}
