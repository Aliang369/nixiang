public class zbModelWnd : zbBaseWnd
{
	private int m_lastShowTime;

	private const int ShowTimeDuration = 5;

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	public override void Refresh()
	{
		m_lastShowTime = 0;
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		m_lastShowTime++;
		if (m_lastShowTime > 5)
		{
			Hide();
		}
	}
}
