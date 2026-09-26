public class zbGuideWorldBoss2 : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbUIMgr.Instance.ShowGuideWeak(11121, ContinueGuide0);
	}

	public void ContinueGuide0()
	{
		zbUIMgr.Instance.ShowGuideWeak(11122, ContinueGuide1);
	}

	public void ContinueGuide1()
	{
		zbGuide.Instance.StopCurGuideStep();
	}
}
