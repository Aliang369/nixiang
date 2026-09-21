using System.Collections;

public abstract class AbstractEnumeratorAction : GameAction
{
	private bool started;

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
		yield return Run();
		FinishAction();
	}

	protected abstract IEnumerator Run();
}
