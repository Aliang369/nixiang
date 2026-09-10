using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using Spine;
using Spine.Unity;
using UnityEngine;

public class RoomObjectDisplay : CTPrefabAsset
{
	public Transform lookPivot;

	public Transform hideObjectAfterEffect;

	public SkeletonAnimation spine;

	public string enterAnimName;

	public string idleAnimName;

	public string exitAnimName;

	public DialoguePlayer dialoguePivot;

	public List<DialogueSequence> enterDialogues;

	public List<DialogueSequence> idleDialogues;

	public List<DialogueSequence> exitDialogues;

	[NonSerialized]
	public bool finishing;

	[NonSerialized]
	private GameStateInfo gameState;

	[NonSerialized]
	private List<DialogueSequence> idleDialoguesCopy;

	public override string CTAssetIDPrepend => "ROD";

	public void Setup(GameStateInfo gs)
	{
		gameState = gs;
		if (idleDialogues != null)
		{
			idleDialoguesCopy = new List<DialogueSequence>(idleDialogues);
		}
	}

	private void Update()
	{
		if (gameState == null || finishing)
		{
			return;
		}
		if (dialoguePivot != null && !dialoguePivot.IsPlaying(gameState) && !idleDialoguesCopy.IsNullOrEmpty() && !gameState.UIManager.avatarDialoguePanel.gameObject.activeInHierarchy)
		{
			DialogueSequence dialogueSequence = idleDialoguesCopy.SelectRandom();
			idleDialoguesCopy.Remove(dialogueSequence);
			gameState.UIManager.dialoguePanel.TriggerDialogue(dialogueSequence, dialoguePivot, null);
		}
		if ((UnityEngine.Object)(object)spine != null && !idleAnimName.IsNullOrEmpty())
		{
			TrackEntry current = spine.AnimationState.GetCurrent(0);
			if (current == null || current.IsComplete)
			{
				spine.PlayAnimation(idleAnimName, loop: true);
			}
		}
	}

	private List<string> GetAllSpineAnimNames()
	{
		SkeletonAnimation obj = spine;
		if (obj == null)
		{
			return null;
		}
		SkeletonDataAsset skeletonDataAsset = ((SkeletonRenderer)obj).skeletonDataAsset;
		if (skeletonDataAsset == null)
		{
			return null;
		}
		return skeletonDataAsset.GetSkeletonData(true)?.Animations?.Select((Animation a) => a.Name).ToList();
	}
}
