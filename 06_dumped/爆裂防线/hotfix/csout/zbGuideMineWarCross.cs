public class zbGuideMineWarCross : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbUIMgr.Instance.ShowGuideWeak(12201, ContinueGuide0);
	}

	public void ContinueGuide0()
	{
		zbGuide.Instance.StopCurGuideStep();
	}
}
