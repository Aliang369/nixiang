using System.Collections;
using UnityEngine;

public class PlayDialogue : CutSceneNode
{
	public DialoguePlayer dialoguePlayer;

	public DialogueSequence dialogueSequence;

	public override IEnumerator Play(PlayCutSceneState state)
	{
		bool finished = false;
		state.CutsceneDialoguePanel.TriggerDialogue(dialogueSequence, dialoguePlayer, OnFinish);
		yield return new WaitUntil(() => finished);
		void OnFinish(DialogBubble.StopType t)
		{
			finished = true;
		}
	}
}
