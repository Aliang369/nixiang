using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.855f, 0.8623f, 0.87f)]
[TrackClipType(typeof(CutsceneSubtitleClip))]
[TrackBindingType(typeof(Cutscene))]
[DisplayName("演出字幕轨道")]
public class CutsceneSubtitleTrack : TrackAsset
{
	private void OnEnable()
	{
		foreach (TimelineClip clip in ((TrackAsset)this).GetClips())
		{
			CutsceneSubtitleClip cutsceneSubtitleClip = clip.asset as CutsceneSubtitleClip;
			clip.displayName = cutsceneSubtitleClip.text;
		}
	}

	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		return ScriptPlayable<CutsceneSubtitleBehaviour>.Create(graph, inputCount);
	}
}
