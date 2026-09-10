using System;
using System.Threading.Tasks;

public class AsyncAction : GameAction
{
	private readonly Func<Task> action;

	private bool started;

	public AsyncAction(Func<Task> action)
	{
		this.action = action;
		started = false;
	}

	protected override void Update()
	{
		if (action != null && !started)
		{
			started = true;
			Start();
		}
	}

	private async void Start()
	{
		await action().ConfigureAwait(continueOnCapturedContext: false);
		FinishAction();
	}
}
