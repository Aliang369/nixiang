using System;
using System.Collections.Generic;
using GameRandom;

public class IdleDialog : DialogController.DialogModule
{
	public class Config
	{
		public List<DialogueSequence> idleDialogues;

		public float interval;
	}

	private DateTime canTriggerTime;

	private Config config;

	public IdleDialog(Config config)
	{
		this.config = config;
	}

	protected override void OnInit()
	{
		canTriggerTime = DateTime.MinValue;
	}

	protected override void OnFrameUpdate()
	{
		if (!(DateTime.UtcNow < canTriggerTime) && !base.isPlaying)
		{
			canTriggerTime = DateTime.MaxValue;
			DialogueSequence sequence = config.idleDialogues.SelectRandom();
			base.gameState.UIManager.dialoguePanel.TriggerDialogue(sequence, base.dialoguePlayer, OnFinish);
		}
		void OnFinish(DialogBubble.StopType stopType)
		{
			canTriggerTime = DateTime.UtcNow + TimeSpan.FromSeconds(config.interval);
		}
	}
}
