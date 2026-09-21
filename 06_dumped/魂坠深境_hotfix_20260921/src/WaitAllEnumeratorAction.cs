using System.Collections;
using System.Collections.Generic;

public class WaitAllEnumeratorAction : GameAction
{
	private readonly List<WrapperEnumerator> enumerators;

	private bool started;

	public WaitAllEnumeratorAction(List<WrapperEnumerator> enumerators)
	{
		this.enumerators = enumerators;
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
		yield return WrapperEnumerator.WaitAll(enumerators);
		FinishAction();
	}
}
