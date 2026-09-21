using System;

public class ActionAction : GameAction
{
	public Action action;

	private bool excuted;

	public ActionAction(Action action)
	{
		this.action = action;
	}

	protected override void Update()
	{
		if (!excuted)
		{
			action();
			excuted = true;
		}
		FinishAction();
	}
}
