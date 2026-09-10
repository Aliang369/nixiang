using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.855f, 0.8623f, 0.87f)]
[TrackClipType(typeof(PlayDialogueClip))]
[TrackBindingType(typeof(DialoguePlayer))]
[DisplayName("播放对话轨道")]
public class PlayDialogueTrack : TrackAsset
{
	private void OnEnable()
	{
		foreach (TimelineClip clip in ((TrackAsset)this).GetClips())
		{
			PlayDialogueClip playDialogueClip = clip.asset as PlayDialogueClip;
			clip.displayName = playDialogueClip.text;
		}
	}

	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		return ScriptPlayable<PlayDialogueMixerBehaviour>.Create(graph, inputCount);
	}
}
