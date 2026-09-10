using System;
using System.Collections.Generic;
using Common.UI;
using UnityEngine;

public class DialoguePanel : UIBase, IFrameUpdate
{
	[SerializeField]
	private DialogBubble dialogBubble;

	[NonSerialized]
	public CellUIPrefabPool<DialogBubble, DialoguePlayer> dialogueBubblePool;

	[NonSerialized]
	private List<DialogBubble> frameUpdateSnapshot = new List<DialogBubble>();

	protected override void Init()
	{
		base.gameObject.SetActive(value: true);
		dialogBubble.gameObject.SetActive(value: false);
		dialogueBubblePool = GetPrefabPool<DialogBubble, DialoguePlayer>(dialogBubble, base.transform);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmtpyAreaClicked);
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmtpyAreaClicked);
	}

	public void FrameUpdate()
	{
		if (frameUpdateSnapshot == null)
		{
			frameUpdateSnapshot = new List<DialogBubble>();
		}
		frameUpdateSnapshot.Clear();
		IReadOnlyList<DialogBubble> activeElements = dialogueBubblePool.GetActiveElements();
		for (int i = 0; i < activeElements.Count; i++)
		{
			frameUpdateSnapshot.Add(activeElements[i]);
		}
		for (int j = 0; j < frameUpdateSnapshot.Count; j++)
		{
			DialogBubble dialogBubble = frameUpdateSnapshot[j];
			if (!dialogBubble.ShouldActive)
			{
				dialogBubble.Stop(DialogBubble.StopType.BreakStop);
			}
		}
	}

	public DialogBubble GetOrSpawnDialogueBubble(DialoguePlayer player)
	{
		DialogBubble dialogBubble = dialogueBubblePool.GetActiveElements().Find((DialogBubble d) => d.player == player);
		if (dialogBubble == null)
		{
			dialogBubble = dialogueBubblePool.Spawn(player);
		}
		return dialogBubble;
	}

	public void SetDialogueText(string text, DialoguePlayer player)
	{
		GetOrSpawnDialogueBubble(player).SetText(text);
	}

	public void StopAllDialogueOfPlayer(DialoguePlayer player)
	{
		IReadOnlyList<DialogBubble> activeElements = dialogueBubblePool.GetActiveElements();
		for (int i = 0; i < activeElements.Count; i++)
		{
			if (activeElements[i].player == player)
			{
				activeElements[i].Stop(DialogBubble.StopType.BreakStop);
			}
		}
	}

	public void TriggerDialogue(string str, DialoguePlayer player, Action<DialogBubble.StopType> onStop)
	{
		DialogueSequence sequence = new DialogueSequence
		{
			dialogueSequence = new List<DialogueText>
			{
				new DialogueText
				{
					text = str
				}
			}
		};
		TriggerDialogue(sequence, player, onStop);
	}

	public void TriggerDialogue(DialogueSequence sequence, DialoguePlayer player, Action<DialogBubble.StopType> onStop, bool setTransform = false)
	{
		StopAllDialogueOfPlayer(player);
		DialogBubble bubble = GetOrSpawnDialogueBubble(player);
		Transform originTransform = bubble.transform.parent;
		if (setTransform)
		{
			bubble.transform.parent = player.transform;
		}
		bubble.PlayDialogueSequence(sequence, delegate(DialogBubble.StopType o)
		{
			onStop?.Invoke(o);
			if (setTransform)
			{
				bubble.transform.parent = originTransform;
			}
		});
	}

	private void _OnEmtpyAreaClicked()
	{
		foreach (DialogBubble activeElement in dialogueBubblePool.GetActiveElements())
		{
			activeElement.SkipNextDialogue();
		}
	}
}
