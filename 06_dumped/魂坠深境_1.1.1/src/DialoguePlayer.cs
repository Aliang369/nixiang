using System.Collections.Generic;
using UnityEngine;

public class DialoguePlayer : MonoBehaviour
{
	private readonly HashSet<DialogueTrigger> triggeredDialogue = new HashSet<DialogueTrigger>();

	public bool IsWorldObjectPlayer => GetComponentsInParent<Canvas>(includeInactive: true).IsNullOrEmpty();

	public bool IsPlaying(GameStateInfo gameState)
	{
		return IsPlaying(gameState.UIManager.dialoguePanel);
	}

	public bool IsPlaying(FortressStateInfo fortressState)
	{
		return IsPlaying(fortressState.uiManager.DialoguePanel);
	}

	public bool IsPlaying(DialoguePanel panel)
	{
		return panel.dialogueBubblePool.GetActiveElements().Find((DialogBubble d) => d.player == this) != null;
	}

	public bool CheckDialogueTriggered(DialogueTrigger trigger)
	{
		return triggeredDialogue.Contains(trigger);
	}

	public void RegisterTriggeredDialogue(DialogueTrigger dialogue)
	{
		triggeredDialogue.Add(dialogue);
	}
}
