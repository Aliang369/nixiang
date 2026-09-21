using System;
using System.Collections;

public class EnumeratorAction : GameAction
{
	protected Func<IEnumerator> action;

	protected bool started;

	public EnumeratorAction(Func<IEnumerator> action)
	{
		this.action = action;
		started = false;
	}

	protected override void Update()
	{
		if (!started)
		{
			started = true;
			gameState.StartCoroutine(Start);
		}
	}

	private IEnumerator Start()
	{
		yield return action();
		FinishAction();
	}
}
