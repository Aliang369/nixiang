using System;
using System.Collections.Generic;

[Serializable]
public abstract class Effect : IEffectCastNode, IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public enum NodeCastResult
	{
		Success = 1,
		Failure
	}

	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public virtual List<IEffectNode> GetChildren()
	{
		return null;
	}

	public NodeCastResult Cast(GameEvent eve)
	{
		try
		{
			return OnCast(eve);
		}
		catch (Exception e)
		{
			TraceLog.Exception(e, $"节点执行错误: {GetType().Name}-{eve.type}\n", ThinkingAnalyticsExtends.ExceptionType.Effect);
			return NodeCastResult.Failure;
		}
	}

	protected abstract NodeCastResult OnCast(GameEvent eve);

	protected static void EnqueueAction(GameEvent eve, GameAction action)
	{
		if (action is ISpellCastContextAction spellCastContextAction && eve?.spellInfo != null)
		{
			spellCastContextAction.SpellInfo = eve.spellInfo;
		}
		eve.gameState?.actionManager?.EnqueueAction(action);
	}

	public static GameEvent GenerateDisplayEvent(GameStateInfo gameState, object caster, SpellCastInfo spellInfo = null, GameEvent.TriggerType triggerType = GameEvent.TriggerType.OutBattleDisplay)
	{
		GameEvent gameEvent = new GameEvent(GameEvent.Type.Display, triggerType);
		gameEvent.gameState = gameState;
		gameEvent.spellInfo = spellInfo;
		gameEvent.SetEveObj(GameEvent.EveObjType.Caster, caster);
		if (caster is Item sourceItem)
		{
			gameEvent.SourceItem = sourceItem;
		}
		if (spellInfo?.targetItem != null)
		{
			gameEvent.TargetItem = spellInfo.targetItem;
		}
		return gameEvent;
	}
}
