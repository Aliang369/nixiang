using System;
using System.Collections.Generic;

public abstract class GameAction
{
	public GameStateInfo gameState;

	public List<ActionLock.Type> ignoreLocks = new List<ActionLock.Type>();

	public bool isDone;

	public ActionManager actionManager => gameState.actionManager;

	protected float actionTimeScale
	{
		get
		{
			if (!gameState.battleState.isInBattle)
			{
				return 1f + gameState.applicationState.settingManager.uiTimeScaleSetting * actionTimeScaleSetting;
			}
			return 1f + gameState.applicationState.settingManager.battleTimeScaleSetting * actionTimeScaleSetting * gameState.actionManager.curve;
		}
	}

	protected virtual float actionTimeScaleSetting => 1f;

	public virtual void Excute(GameStateInfo gameState)
	{
		this.gameState = gameState;
		Update();
	}

	public void Ignore(ActionLock.Type type)
	{
		ignoreLocks.AddUnique(type);
	}

	public void PassIgnoreLocks(GameAction action)
	{
		foreach (ActionLock.Type ignoreLock in ignoreLocks)
		{
			action.Ignore(ignoreLock);
		}
	}

	protected void EnqueueAction(Action action, bool passIgnoreLocks = true)
	{
		EnqueueAction(new ActionAction(action), passIgnoreLocks);
	}

	protected void EnqueueAction(GameAction action, bool passIgnoreLocks = true)
	{
		if (passIgnoreLocks)
		{
			PassIgnoreLocks(action);
		}
		gameState?.actionManager?.EnqueueAction(action);
	}

	protected void FinishAction()
	{
		isDone = true;
	}

	protected abstract void Update();
}
