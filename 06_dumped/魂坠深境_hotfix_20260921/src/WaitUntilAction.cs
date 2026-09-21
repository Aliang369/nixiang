using System;

public class WaitUntilAction : GameAction
{
	public Func<bool> condition;

	public WaitUntilAction(Func<bool> condition)
	{
		this.condition = condition;
	}

	protected override void Update()
	{
		if (condition == null)
		{
			FinishAction();
		}
		else if (condition())
		{
			FinishAction();
		}
	}
}
