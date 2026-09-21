using UnityEngine.Playables;

public class CutsceneScreenFadeMixerBehaviour : ScreenFaderMixerBehaviour
{
	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		if (playerData is Cutscene { currentPlayState: not null } cutscene)
		{
			base.ProcessFrame(playable, info, (object)cutscene.currentPlayState.CutscenePanel.cutsceneAnimDarkenImage);
		}
	}
}
