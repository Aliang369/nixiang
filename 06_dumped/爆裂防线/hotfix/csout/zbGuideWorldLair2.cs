public class zbGuideWorldLair2 : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbUIMgr.Instance.GetWndById(EWndId.WorldLairFight);
		zbUIMgr.Instance.ShowGuideWeak(10821, ContinueGuide0);
	}

	public void ContinueGuide0()
	{
		zbGuide.Instance.StopCurGuideStep();
	}
}
