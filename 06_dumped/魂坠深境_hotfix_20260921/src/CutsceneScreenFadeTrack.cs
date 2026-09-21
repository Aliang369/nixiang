using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.875f, 0.5944853f, 0.1737132f)]
[TrackClipType(typeof(ScreenFaderClip))]
[TrackBindingType(typeof(Cutscene))]
[DisplayName("黑屏淡入淡出轨道")]
public class CutsceneScreenFadeTrack : ScreenFaderTrack
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		return ScriptPlayable<CutsceneScreenFadeMixerBehaviour>.Create(graph, inputCount);
	}
}
