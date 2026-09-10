using System.Collections;
using System.Collections.Generic;

public class WaitEnumeratorAction : GameAction
{
	private readonly WrapperEnumerator action;

	private bool started;

	public WaitEnumeratorAction(WrapperEnumerator action)
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
		yield return WrapperEnumerator.WaitAll(new List<WrapperEnumerator> { action });
		FinishAction();
	}
}
