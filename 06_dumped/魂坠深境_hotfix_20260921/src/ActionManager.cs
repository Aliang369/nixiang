using System;
using System.Collections.Generic;
using System.Linq;

public class ActionManager : InGameManager
{
	private const double EnemyDeathQueueGraceSeconds = 3.0;

	private LinkedList<GameAction> actionQueue = new LinkedList<GameAction>();

	private GameAction currentAction;

	private readonly HashSet<Item> afterTriggerChapterEffectSuppressedItemsUntilQueueClean = new HashSet<Item>();

	private readonly HashSet<Item> shadowTraceSelectedItemsUntilQueueClean = new HashSet<Item>();

	private ItemContainer shadowEscapeChainContainer;

	private readonly HashSet<Item> shadowEscapeChainItems = new HashSet<Item>();

	private bool shadowEscapeChainRejected;

	private bool tableItemHardLimitHintShownUntilQueueClean;

	private LinkedListNode<GameAction> currentActionChunkLastNode;

	private List<ActionLock> locks;

	public DateTime lastEmptyTime;

	public bool isEmpty
	{
		get
		{
			if (actionQueue.Count == 0)
			{
				return currentAction == null;
			}
			return false;
		}
	}

	public bool skipFrameAnim => GetBattleScale() > 2000f;

	public float curve => ScriptableObjectSingleton<GameDataManager>.instance.timeScaleCurve.Evaluate((float)(DateTime.UtcNow - lastEmptyTime).Ticks / 10000000f);

	public event Action<GameAction> actionFinishEvent;

	public event Action actionFrameFinishEvent;

	public event Action actionQueueCleanEvent;

	public float GetBattleScale(float actionScale = 1f)
	{
		return 1f + base.gameState.applicationState.settingManager.battleTimeScaleSetting * actionScale * base.gameState.actionManager.curve;
	}

	public float GetUIScale(float actionScale = 1f)
	{
		return 1f + base.gameState.applicationState.settingManager.uiTimeScaleSetting * actionScale;
	}

	public override void Init(GameStateInfo gameState)
	{
		afterTriggerChapterEffectSuppressedItemsUntilQueueClean.Clear();
		shadowTraceSelectedItemsUntilQueueClean.Clear();
		BreakShadowEscapeChain();
		tableItemHardLimitHintShownUntilQueueClean = false;
		locks = new List<ActionLock>();
	}

	public void Update()
	{
		UpdateAction();
	}

	public void EnqueueAction(GameAction action)
	{
		action.gameState = base.gameState;
		if (currentActionChunkLastNode == null)
		{
			if (currentAction != null)
			{
				actionQueue.AddFirst(action);
				currentActionChunkLastNode = actionQueue.First;
			}
			else
			{
				actionQueue.AddLast(action);
			}
		}
		else
		{
			actionQueue.AddAfter(currentActionChunkLastNode, action);
			currentActionChunkLastNode = currentActionChunkLastNode.Next;
		}
	}

	public void CleanActionQueue()
	{
		actionQueue.Clear();
		currentAction = null;
		currentActionChunkLastNode = null;
		afterTriggerChapterEffectSuppressedItemsUntilQueueClean.Clear();
		shadowTraceSelectedItemsUntilQueueClean.Clear();
		BreakShadowEscapeChain();
		tableItemHardLimitHintShownUntilQueueClean = false;
	}

	public void EnqueueActionToEnd(GameAction action)
	{
		action.gameState = base.gameState;
		actionQueue.AddLast(action);
	}

	public void EnqueueAction(Action action)
	{
		EnqueueAction(new ActionAction(action));
	}

	public void SuppressAfterTriggerChapterEffectsUntilQueueClean(Item item)
	{
		if (item != null)
		{
			afterTriggerChapterEffectSuppressedItemsUntilQueueClean.Add(item);
		}
	}

	public bool AreAfterTriggerChapterEffectsSuppressed(Item item)
	{
		if (item != null)
		{
			return afterTriggerChapterEffectSuppressedItemsUntilQueueClean.Contains(item);
		}
		return false;
	}

	public bool HasShadowTraceSelection(Item item)
	{
		if (item != null)
		{
			return shadowTraceSelectedItemsUntilQueueClean.Contains(item);
		}
		return false;
	}

	public bool TryRegisterShadowTraceSelection(Item item)
	{
		if (item != null)
		{
			return shadowTraceSelectedItemsUntilQueueClean.Add(item);
		}
		return false;
	}

	public bool TryRegisterShadowEscapeChainItem(Item item, ItemContainer container)
	{
		if (item == null || container == null || shadowEscapeChainRejected)
		{
			return false;
		}
		if (shadowEscapeChainContainer != container)
		{
			shadowEscapeChainContainer = container;
			shadowEscapeChainItems.Clear();
			shadowEscapeChainRejected = false;
		}
		if (!shadowEscapeChainItems.Add(item))
		{
			shadowEscapeChainRejected = true;
			return false;
		}
		return true;
	}

	public void BreakShadowEscapeChain(ItemContainer container = null)
	{
		if (container == null || shadowEscapeChainContainer == container)
		{
			shadowEscapeChainContainer = null;
			shadowEscapeChainItems.Clear();
			shadowEscapeChainRejected = false;
		}
	}

	public void ShowTableItemHardLimitHintOnce()
	{
		if (!tableItemHardLimitHintShownUntilQueueClean)
		{
			tableItemHardLimitHintShownUntilQueueClean = true;
			EnqueueAction(new HintAction(ScriptableObjectSingleton<TextDataPreset>.instance.term_item_table_hard_limit_reached));
		}
	}

	public bool IsActionLocked(ActionLock.Type type)
	{
		return locks.Any((ActionLock l) => l.type == type && l.count > 0);
	}

	public void AcquireLock(ActionLock.Type type)
	{
		ActionLock actionLock = locks.Find((ActionLock l) => l.type == type);
		if (actionLock == null)
		{
			actionLock = new ActionLock(type);
			locks.Add(actionLock);
		}
		actionLock.Acquire();
	}

	public void ReleaseLock(ActionLock.Type type)
	{
		locks.Find((ActionLock l) => l.type == type)?.Release();
	}

	public void ResetLocks()
	{
		locks.Clear();
	}

	public void Reset()
	{
		actionQueue.Clear();
		currentAction = null;
		currentActionChunkLastNode = null;
		afterTriggerChapterEffectSuppressedItemsUntilQueueClean.Clear();
		shadowTraceSelectedItemsUntilQueueClean.Clear();
		tableItemHardLimitHintShownUntilQueueClean = false;
		locks.Clear();
	}

	private void UpdateAction()
	{
		bool flag = false;
		do
		{
			if (currentAction == null || currentAction.isDone)
			{
				if (actionQueue.Count != 0)
				{
					currentAction = actionQueue.First.Value;
					currentActionChunkLastNode = null;
					actionQueue.RemoveFirst();
				}
				else if (currentAction != null)
				{
					currentAction = null;
					afterTriggerChapterEffectSuppressedItemsUntilQueueClean.Clear();
					shadowTraceSelectedItemsUntilQueueClean.Clear();
					BreakShadowEscapeChain();
					tableItemHardLimitHintShownUntilQueueClean = false;
					actionQueueCleanEvent?.Invoke();
				}
			}
			if (currentAction != null && !currentAction.isDone && !IsLocked(currentAction))
			{
				try
				{
					currentAction.Excute(currentAction.gameState);
				}
				catch (Exception e)
				{
					TraceLog.Exception(e, $"Action结算错误{currentAction}", ThinkingAnalyticsExtends.ExceptionType.Action);
					currentAction = null;
					continue;
				}
				if (currentAction != null && currentAction.isDone)
				{
					actionFinishEvent?.Invoke(currentAction);
					flag = true;
				}
			}
			if (TryForceFinishPendingEnemyDeath())
			{
				break;
			}
		}
		while (currentAction != null && currentAction.isDone);
		if (flag)
		{
			actionFrameFinishEvent?.Invoke();
		}
		if (actionQueue.IsNullOrEmpty())
		{
			lastEmptyTime = DateTime.UtcNow;
		}
	}

	private bool TryForceFinishPendingEnemyDeath()
	{
		CharacterAI characterAI = base.gameState?.battleState?.enemy;
		if (characterAI == null || !characterAI.deathPending || characterAI.dead)
		{
			return false;
		}
		if ((DateTime.UtcNow - characterAI.deathPendingSinceUtc).TotalSeconds < 3.0)
		{
			return false;
		}
		TraceLog.Warn("[ActionManager] 敌人最终死亡后动作队列超时，终止剩余结算并进入胜利流程。");
		CleanActionQueue();
		characterAI.Die();
		return true;
	}

	private bool IsLocked(GameAction action)
	{
		foreach (ActionLock @lock in locks)
		{
			if (@lock.IsLocked(action))
			{
				return true;
			}
		}
		return false;
	}
}
