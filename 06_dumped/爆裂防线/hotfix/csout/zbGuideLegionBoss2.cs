public class zbGuideLegionBoss2 : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbUIMgr.Instance.ShowGuideWeak(11021, ContinueGuide0);
	}

	public void ContinueGuide0()
	{
		zbUIMgr.Instance.ShowGuideWeak(11022, ContinueGuide1);
	}

	public void ContinueGuide1()
	{
		zbGuide.Instance.StopCurGuideStep();
	}
}
