using UnityEngine;

public class WaitAction : GameAction
{
	public float seconds;

	private float timer;

	public WaitAction(float seconds)
	{
		this.seconds = seconds;
	}

	protected override void Update()
	{
		timer += Time.deltaTime * base.actionTimeScale;
		if (timer > seconds)
		{
			FinishAction();
		}
	}
}
