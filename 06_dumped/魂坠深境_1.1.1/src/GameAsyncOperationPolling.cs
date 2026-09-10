using System.Collections.Generic;

public class GameAsyncOperationPolling
{
	public readonly GameAsyncOperation operation;

	public readonly List<float> intervalList;

	public int pollTimeCount;

	public float timer;

	public bool stoped;

	private bool started;

	public bool stopOnComplete;

	public GameAsyncOperationPolling(GameAsyncOperation operation, List<float> intervalList)
	{
		this.operation = operation;
		this.intervalList = intervalList;
		stopOnComplete = true;
	}

	public void AssignPolling()
	{
		Bootstrap.instance.bootstrapExtra.gameAsyncOperationManager.AssignPoll(this);
	}

	public void Stop()
	{
		stoped = true;
	}

	public void Update(float deltaTime)
	{
		if (stoped)
		{
			return;
		}
		if (operation.isRunning)
		{
			operation.Update();
			return;
		}
		if ((stopOnComplete && operation.isComplete) || pollTimeCount >= intervalList.Count)
		{
			Stop();
			return;
		}
		timer += deltaTime;
		float num = intervalList[pollTimeCount];
		if (timer >= num)
		{
			TraceLog.Debug($"第{pollTimeCount}次轮询{operation}");
			pollTimeCount++;
			timer = 0f;
			started = true;
			operation.Run();
		}
	}
}
