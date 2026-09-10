using System;
using System.Collections.Generic;

[Serializable]
public class DialogueSequence
{
	public List<DialogueText> dialogueSequence;

	public bool disableSkip;

	public void SetupDialogueText(DialogueTrigger trigger, List<DialogueText> texts)
	{
		List<DialogueText> list = new List<DialogueText>();
		foreach (DialogueText text in texts)
		{
			DialogueText dialogueText = new DialogueText();
			dialogueText.prewarmTime = text.prewarmTime;
			dialogueText.afterWaitTimeRatio = text.afterWaitTimeRatio;
			dialogueText.readSpeedRatio = text.readSpeedRatio;
			dialogueText.text = text.text;
			list.Add(dialogueText);
		}
		dialogueSequence = list;
	}
}
