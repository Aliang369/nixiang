using UnityEngine.Playables;

public class CutsceneSubtitleBehaviour : PlayableBehaviour
{
	public string text;

	private Cutscene cutScene;

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		if (cutScene == null)
		{
			cutScene = playerData as Cutscene;
		}
		if (cutScene?.currentPlayState == null)
		{
			return;
		}
		int num = -1;
		int inputCount = playable.GetInputCount();
		playable.GetOutputCount();
		playable.GetTime();
		for (int i = 0; i < inputCount; i++)
		{
			if (playable.GetInputWeight(i) > 0f)
			{
				num = i;
			}
		}
		if (num != -1)
		{
			cutScene.currentPlayState.CutscenePanel.subtitleText.gameObject.SetActive(value: true);
			CutsceneSubtitleBehaviour behaviour = ((ScriptPlayable<CutsceneSubtitleBehaviour>)playable.GetInput(num)).GetBehaviour();
			cutScene.currentPlayState.CutscenePanel.subtitleText.gameObject.SetActive(value: true);
			cutScene.currentPlayState.CutscenePanel.subtitleText.text = behaviour.text;
		}
		else if (cutScene.currentPlayState != null)
		{
			cutScene.currentPlayState.CutscenePanel.subtitleText.gameObject.SetActive(value: false);
		}
	}
}
