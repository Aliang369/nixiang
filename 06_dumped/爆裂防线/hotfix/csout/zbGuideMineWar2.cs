using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGuideMineWar2 : zbGuideBase
{
	private bool showMineWarDetail;

	public override void Start()
	{
		base.Start();
		if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossStart < zbSelf.Instance.timeHandler.GetServerTime() && zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossEnd > zbSelf.Instance.timeHandler.GetServerTime())
		{
			ContinueGuide4();
			return;
		}
		OnClickBiaoji();
		((MonoBehaviour)(zbUIMgr.Instance.GetWndById(EWndId.MineWar) as zbMineWarWnd)).StartCoroutine(ActionCoroutine());
	}

	public IEnumerator ActionCoroutine()
	{
		yield return null;
		zbUIMgr.Instance.GetWndById(EWndId.MineWar);
		zbUIMgr.Instance.ShowGuideWeak(12111, ContinueGuide0, 200f, zbMineWarMgr.Instance.mineWarPointList[30].mineWarHud.m_Root);
	}

	public void ContinueGuide0()
	{
		zbWsMsgHandler.Instance.MineWar_PointData_Req(31);
		showMineWarDetail = true;
	}

	public void ContinueGuide1()
	{
		zbUIMgr.Instance.ShowGuideWeak(12112, ContinueGuide2, 1200f);
	}

	public void ContinueGuide2()
	{
		zbMineWarDetailWnd zbMineWarDetailWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MineWarDetail) as zbMineWarDetailWnd;
		zbUIMgr.Instance.ShowGuideWeak(12113, ContinueGuide3, 1200f, (VisualElement)(object)zbMineWarDetailWnd2.m_btnClose);
	}

	public void ContinueGuide3()
	{
		(zbUIMgr.Instance.GetWndById(EWndId.MineWarDetail) as zbMineWarDetailWnd).Hide();
		zbUIMgr.Instance.ShowGuideWeak(12114, ContinueGuide4);
	}

	public void ContinueGuide4()
	{
		zbGuide.Instance.StopCurGuideStep();
	}

	public void OnClickBiaoji()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		zbMineWarWnd obj = zbUIMgr.Instance.GetWndById(EWndId.MineWar) as zbMineWarWnd;
		obj.m_homeMapCinema.SetCameraOrthographicSize(6f);
		zbMineWarPoint zbMineWarPoint2 = zbMineWarMgr.Instance.mineWarPointList[0];
		zbMineWarPoint obj2 = zbMineWarMgr.Instance.mineWarPointList[30];
		float num = ((Component)obj2).transform.position.x - ((Component)zbMineWarPoint2).transform.position.x;
		float num2 = ((Component)obj2).transform.position.y - ((Component)zbMineWarPoint2).transform.position.y;
		obj.m_homeMapCinema.FocusToPosition(new Vector3(num, num2));
	}

	public override void Update()
	{
		base.Update();
		if (showMineWarDetail)
		{
			zbMineWarDetailWnd zbMineWarDetailWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MineWarDetail) as zbMineWarDetailWnd;
			if ((Object)(object)zbMineWarDetailWnd2 != (Object)null && zbMineWarDetailWnd2.IsShow())
			{
				ContinueGuide1();
				showMineWarDetail = false;
			}
		}
	}
}
