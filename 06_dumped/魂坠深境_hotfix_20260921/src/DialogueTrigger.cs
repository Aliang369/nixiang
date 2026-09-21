using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public enum TriggerType
	{
		None,
		BattleStart,
		Idle,
		Defeated,
		KillPlayer,
		ExitCrystalFreeze,
		Hit,
		CastItem,
		DrawItem,
		AnimEvent,
		OnDeathBlow,
		MoonCocoonBreak
	}

	public class AnimEventConfig
	{
		public string animName;

		public ItemDefinition itemDefinition;
	}

	[SerializeField]
	public AnimEventConfig animEventConfig;

	public CheckNode checkNode;

	public string dialogueId;

	public List<DialogueText> dialogueTexts = new List<DialogueText>();

	public bool halt;

	public bool idle;

	public bool disableSkip;

	public bool oneshot = true;

	public TriggerType triggerType;

	public GameEvent.Type type;

	private bool shouldShowAnimEventConfig
	{
		get
		{
			if (triggerType != TriggerType.AnimEvent)
			{
				return triggerType == TriggerType.CastItem;
			}
			return true;
		}
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { checkNode };
	}

	public void Trigger(DialoguePlayer dialoguePlayer, GameStateInfo gameState, bool direct = false)
	{
		DialogueSequence sequence = GenerateDialogueInfo();
		if (direct)
		{
			gameState.UIManager.dialoguePanel.TriggerDialogue(sequence, dialoguePlayer, null);
		}
		else
		{
			gameState.actionManager.EnqueueAction(new TriggerDialogueAction(sequence, dialoguePlayer, halt));
		}
		dialoguePlayer.RegisterTriggeredDialogue(this);
	}

	public bool CheckGameEventTriggerAble(GameEvent eve, DialoguePlayer dialoguePlayer)
	{
		if (eve.type == type && (checkNode == null || checkNode.Check(eve) == true) && CheckTriggerOneShot(dialoguePlayer))
		{
			return true;
		}
		return false;
	}

	public bool CheckIdleTriggerAble(GameStateInfo gameState, DialoguePlayer dialoguePlayer)
	{
		if (gameState.actionManager.isEmpty && !dialoguePlayer.IsPlaying(gameState))
		{
			return CheckTriggerOneShot(dialoguePlayer);
		}
		return false;
	}

	public bool CheckDefeatedTriggerAble(GameStateInfo gameState, DialoguePlayer dialoguePlayer)
	{
		if (!dialoguePlayer.IsPlaying(gameState))
		{
			return CheckTriggerOneShot(dialoguePlayer);
		}
		return false;
	}

	public bool CheckHitTriggerAble(GameStateInfo gameState, DialoguePlayer dialoguePlayer)
	{
		if (!dialoguePlayer.IsPlaying(gameState))
		{
			return CheckTriggerOneShot(dialoguePlayer);
		}
		return false;
	}

	public bool CheckTriggerOneShot(DialoguePlayer dialoguePlayer)
	{
		if (oneshot)
		{
			return !dialoguePlayer.CheckDialogueTriggered(this);
		}
		return true;
	}

	public bool CheckAnimEventTriggerAble(DialoguePlayer dialoguePlayer, string eventName, ItemDefinition itemDefinition)
	{
		if (!CheckTriggerOneShot(dialoguePlayer))
		{
			return false;
		}
		if (triggerType == TriggerType.CastItem || triggerType == TriggerType.AnimEvent)
		{
			if (animEventConfig == null)
			{
				return false;
			}
			if (animEventConfig.animName == eventName || animEventConfig.itemDefinition == itemDefinition)
			{
				return true;
			}
		}
		return false;
	}

	public DialogueSequence GenerateDialogueInfo()
	{
		DialogueSequence dialogueSequence = new DialogueSequence();
		dialogueSequence.SetupDialogueText(this, dialogueTexts);
		dialogueSequence.disableSkip = disableSkip;
		return dialogueSequence;
	}

	public override string ToString()
	{
		return triggerType switch
		{
			TriggerType.None => "节点", 
			TriggerType.BattleStart => "战斗开始", 
			TriggerType.Idle => "闲置", 
			TriggerType.Defeated => "被击败", 
			TriggerType.KillPlayer => "击败玩家", 
			TriggerType.ExitCrystalFreeze => "离开水晶阶段", 
			TriggerType.Hit => "被攻击", 
			TriggerType.CastItem => "施放", 
			TriggerType.DrawItem => "抽取", 
			TriggerType.AnimEvent => "动画事件", 
			TriggerType.OnDeathBlow => "进入二阶段", 
			TriggerType.MoonCocoonBreak => "月之茧破裂", 
			_ => "", 
		};
	}
}
