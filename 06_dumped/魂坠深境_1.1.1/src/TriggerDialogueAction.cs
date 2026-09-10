using System;
using UnityEngine;

public class TriggerDialogueAction : GameAction
{
	private const double MinimumHaltTimeoutSeconds = 30.0;

	private const double HaltTimeoutGraceSeconds = 10.0;

	public bool halt;

	public DialoguePlayer player;

	public DialogueSequence sequence;

	private bool started;

	private bool completed;

	private bool inputLockAcquired;

	private double haltElapsedSeconds;

	private double haltTimeoutSeconds;

	public TriggerDialogueAction(DialogueSequence sequence, DialoguePlayer player, bool halt = false)
	{
		this.sequence = sequence;
		this.player = player;
		this.halt = halt;
	}

	protected override void Update()
	{
		if (player == null || sequence == null)
		{
			Complete();
			return;
		}
		if (!started)
		{
			started = true;
			try
			{
				if (halt)
				{
					haltTimeoutSeconds = GetHaltTimeoutSeconds();
					gameState.AcquirePlayerInputLock();
					inputLockAcquired = true;
				}
				gameState.UIManager.dialoguePanel.TriggerDialogue(sequence, player, OnFinish);
			}
			catch (Exception e)
			{
				TraceLog.Exception(e, "触发对话异常", ThinkingAnalyticsExtends.ExceptionType.UIException);
				Complete();
			}
		}
		if (!halt)
		{
			Complete();
		}
		else
		{
			if (completed)
			{
				return;
			}
			if (!player.IsPlaying(gameState))
			{
				Complete();
				return;
			}
			haltElapsedSeconds += Time.unscaledDeltaTime;
			if (haltElapsedSeconds < haltTimeoutSeconds)
			{
				return;
			}
			TraceLog.Error($"阻塞对话超时，强制结束对话并继续动作队列。player={player.name}, timeout={haltTimeoutSeconds:F1}s");
			try
			{
				gameState.UIManager.dialoguePanel.StopAllDialogueOfPlayer(player);
			}
			catch (Exception e2)
			{
				TraceLog.Exception(e2, "阻塞对话超时回收异常", ThinkingAnalyticsExtends.ExceptionType.UIException);
			}
			finally
			{
				Complete();
			}
		}
		void OnFinish(DialogBubble.StopType t)
		{
			if (halt)
			{
				Complete();
			}
		}
	}

	private double GetHaltTimeoutSeconds()
	{
		double num = 0.0;
		if (sequence.dialogueSequence != null)
		{
			foreach (DialogueText item in sequence.dialogueSequence)
			{
				if (item != null)
				{
					num += (double)item.prewarmTime;
					num += (double)item.AfterWaitTime;
					num += (double)item.SimuReadTime;
				}
			}
		}
		return Math.Max(30.0, num + 10.0);
	}

	private void Complete()
	{
		if (!completed)
		{
			completed = true;
			if (inputLockAcquired)
			{
				inputLockAcquired = false;
				gameState.ReleasePlayerInputLock();
			}
			FinishAction();
		}
	}
}
