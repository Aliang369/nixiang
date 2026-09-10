using UnityEngine.Playables;

public class PlayDialogueMixerBehaviour : PlayableBehaviour
{
	private Cutscene cutScene;

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		DialoguePlayer dialoguePlayer = playerData as DialoguePlayer;
		if (dialoguePlayer == null)
		{
			return;
		}
		if (cutScene == null)
		{
			cutScene = dialoguePlayer.GetComponentInParent<Cutscene>();
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
			ScriptPlayable<PlayDialogueBehaviour> playable2 = (ScriptPlayable<PlayDialogueBehaviour>)playable.GetInput(num);
			PlayDialogueBehaviour behaviour = playable2.GetBehaviour();
			if (behaviour.progress)
			{
				float num2 = (float)playable2.GetTime();
				float num3 = (float)playable2.GetDuration();
				float progress = num2 / num3;
				string progressTextWithoutTag = behaviour.fullText.GetProgressTextWithoutTag(progress);
				if (cutScene.currentPlayState != null)
				{
					cutScene.currentPlayState.CutsceneDialoguePanel.SetDialogueText(progressTextWithoutTag, dialoguePlayer);
				}
			}
			else if (cutScene.currentPlayState != null)
			{
				cutScene.currentPlayState.CutsceneDialoguePanel.SetDialogueText(behaviour.fullText, dialoguePlayer);
			}
		}
		else if (cutScene.currentPlayState != null)
		{
			cutScene.currentPlayState.CutsceneDialoguePanel.StopAllDialogueOfPlayer(dialoguePlayer);
		}
	}
}
