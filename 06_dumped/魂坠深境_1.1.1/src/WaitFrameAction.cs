public class WaitFrameAction : GameAction
{
	public int times;

	public WaitFrameAction(int times)
	{
		this.times = times;
	}

	protected override void Update()
	{
		if (times <= 0)
		{
			FinishAction();
		}
		else
		{
			times--;
		}
	}
}
