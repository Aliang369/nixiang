using System;
using System.Collections.Generic;

public class TriggerDialog : Effect
{
	public enum TriggerType
	{
		Custom,
		EnemyPreset
	}

	public TriggerType type;

	public List<DialogueText> dialogs;

	public bool halt;

	public string dialogueId;

	public bool checkTriggerAble = true;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (eve.CasterItem == null)
		{
			return NodeCastResult.Failure;
		}
		if (type == TriggerType.Custom)
		{
			DialogueSequence dialogueSequence = new DialogueSequence();
			dialogueSequence.dialogueSequence = new List<DialogueText>();
			foreach (DialogueText dialog in dialogs)
			{
				dialogueSequence.dialogueSequence.Add(dialog);
			}
			ItemDisplayBase activeItemSlotDisplay = ItemDisplayBase.GetActiveItemSlotDisplay(eve.CasterItem);
			if (activeItemSlotDisplay == null)
			{
				return NodeCastResult.Failure;
			}
			DialoguePlayer component = activeItemSlotDisplay.GetComponent<DialoguePlayer>();
			if (component == null)
			{
				return NodeCastResult.Failure;
			}
			Effect.EnqueueAction(eve, new TriggerDialogueAction(dialogueSequence, component, halt));
			return NodeCastResult.Success;
		}
		if (type == TriggerType.EnemyPreset)
		{
			CharacterAI characterAI = eve.CasterItem?.OwnerBft?.characterAI;
			if (characterAI == null)
			{
				return NodeCastResult.Failure;
			}
			DialoguePlayer orAddDialogPlayer = characterAI.GetOrAddDialogPlayer();
			if (orAddDialogPlayer == null)
			{
				return NodeCastResult.Failure;
			}
			DialogueTrigger dialogueTrigger = characterAI.component.dialogueTriggers.Find((DialogueTrigger t) => t.dialogueId == dialogueId);
			if (dialogueTrigger != null && (!checkTriggerAble || dialogueTrigger.CheckTriggerOneShot(orAddDialogPlayer)))
			{
				dialogueTrigger.Trigger(orAddDialogPlayer, eve.gameState);
			}
		}
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		string text = "";
		foreach (DialogueText item in dialogs.OrEmptyIfNull())
		{
			text = text + item.text + Environment.NewLine;
		}
		return new NodeDisplayInfo
		{
			str = new NodeDisplayInfo.Element<string>(text)
		};
	}
}
