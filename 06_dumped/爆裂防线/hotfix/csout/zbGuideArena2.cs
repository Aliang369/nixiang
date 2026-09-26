public class zbGuideArena2 : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbGameSave.Instance.SetLong(zbSelf.Instance.arenaHandler.saveShowLegionWarEffectKey, zbSelf.Instance.timeHandler.GetServerTime());
		zbUIMgr.Instance.GetWndById(EWndId.Arena);
		zbUIMgr.Instance.ShowGuideWeak(11621, ContinueGuide1, 360f);
	}

	public void ContinueGuide1()
	{
		zbArenaWnd zbArenaWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Arena) as zbArenaWnd;
		zbUIMgr.Instance.ShowGuideWeak(11622, ContinueGuide2, 360f, zbArenaWnd2.m_nodes[2]);
	}

	public void ContinueGuide2()
	{
		(zbUIMgr.Instance.GetWndById(EWndId.Arena) as zbArenaWnd).OnTouchs(null, 2);
		zbUIMgr.Instance.ShowGuideWeak(11623, ContinueGuide10, 1100f);
	}

	public void ContinueGuide10()
	{
		zbGuide.Instance.StopCurGuideStep();
	}
}
