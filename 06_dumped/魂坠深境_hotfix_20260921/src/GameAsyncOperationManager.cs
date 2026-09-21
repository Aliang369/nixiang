using System;
using System.Collections.Generic;
using Common.UI;
using UnityEngine;

public class GameAsyncOperationManager
{
	[NonSerialized]
	private Queue<GameAsyncOperation> foregroundOperationQueue = new Queue<GameAsyncOperation>();

	[NonSerialized]
	private Queue<GameAsyncOperation> backgroundOperationQueue = new Queue<GameAsyncOperation>();

	[NonSerialized]
	private List<GameAsyncOperationPolling> oprationPolls = new List<GameAsyncOperationPolling>();

	[NonSerialized]
	private GameAsyncOperation currentForegroundOperation;

	[NonSerialized]
	private GameAsyncOperation currentBackgroundOperation;

	[NonSerialized]
	public PrefabPool<AsyncOperationWaitPanel> asyncOperationPanelPool;

	[NonSerialized]
	private WaitPanelBase waitPanelBase;

	public bool IsCurrentRunning
	{
		get
		{
			if (currentForegroundOperation == null && foregroundOperationQueue.Count <= 0 && currentBackgroundOperation == null)
			{
				return backgroundOperationQueue.Count > 0;
			}
			return true;
		}
	}

	public bool IsBlockingOperationRunning
	{
		get
		{
			GameAsyncOperation gameAsyncOperation = currentForegroundOperation;
			if (gameAsyncOperation == null || !gameAsyncOperation.BlocksLoading)
			{
				GameAsyncOperation gameAsyncOperation2 = currentBackgroundOperation;
				if (gameAsyncOperation2 == null || !gameAsyncOperation2.BlocksLoading)
				{
					foreach (GameAsyncOperation item in foregroundOperationQueue)
					{
						if (item != null && item.BlocksLoading)
						{
							return true;
						}
					}
					foreach (GameAsyncOperation item2 in backgroundOperationQueue)
					{
						if (item2 != null && item2.BlocksLoading)
						{
							return true;
						}
					}
					return false;
				}
			}
			return true;
		}
	}

	public GameAsyncOperationManager(BootstrapExtra bootstrapExtra)
	{
		if (bootstrapExtra != null)
		{
			asyncOperationPanelPool = new PrefabPool<AsyncOperationWaitPanel>(bootstrapExtra.asyncOperationWaitPanelPrefab, bootstrapExtra.asyncOperationWaitPanelParent);
			waitPanelBase = bootstrapExtra.waitPanelBase;
		}
	}

	public void Clear()
	{
		foregroundOperationQueue.Clear();
		backgroundOperationQueue.Clear();
		oprationPolls.Clear();
		currentForegroundOperation = null;
		currentBackgroundOperation = null;
		asyncOperationPanelPool?.DespawnAll();
		waitPanelBase?.Clear();
	}

	public void Update()
	{
		UpdateLane(ref currentForegroundOperation, foregroundOperationQueue);
		UpdateLane(ref currentBackgroundOperation, backgroundOperationQueue);
		if (oprationPolls.IsNullOrEmpty())
		{
			return;
		}
		foreach (GameAsyncOperationPolling oprationPoll in oprationPolls)
		{
			oprationPoll.Update(Time.deltaTime);
		}
		oprationPolls.RemoveAll((GameAsyncOperationPolling p) => p.stoped);
	}

	public void EnqueueAsyncOperation(GameAsyncOperation operation)
	{
		GetQueue(operation.Lane).Enqueue(operation);
	}

	public void EnqueueAsyncOperation(GameAsyncOperation operation, GameAsyncOperationLane lane)
	{
		operation.SetLane(lane);
		GetQueue(lane).Enqueue(operation);
	}

	public void AssignPoll(GameAsyncOperationPolling polling)
	{
		oprationPolls.Add(polling);
	}

	private Queue<GameAsyncOperation> GetQueue(GameAsyncOperationLane lane)
	{
		if (lane != GameAsyncOperationLane.Background)
		{
			return foregroundOperationQueue;
		}
		return backgroundOperationQueue;
	}

	private static void UpdateLane(ref GameAsyncOperation currentOperation, Queue<GameAsyncOperation> operationQueue)
	{
		if (currentOperation == null && operationQueue.Count > 0)
		{
			currentOperation = operationQueue.Dequeue();
			currentOperation.Run();
		}
		if (currentOperation != null)
		{
			try
			{
				currentOperation.Update();
			}
			catch (Exception e)
			{
				TraceLog.Exception(e, "异步操作错误");
			}
			if (currentOperation.isComplete || currentOperation.isFault)
			{
				currentOperation = null;
			}
		}
	}
}
