using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbServantSelectWnd : zbTabWnd
{
	private ScrollView m_scrollView;

	private Button m_btnClose;

	private Button m_btnConfirm;

	private zbServantWnd m_servantWnd;

	public string m_selectedServantId;

	public List<ServantDetail> m_FilteredServantList;

	protected override void OnEnable()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, (string)null, (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_scrollView, "unity-content-container", (string)null).style.flexWrap = StyleEnum<Wrap>.op_Implicit((Wrap)1);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnConfirm, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom()
	{
		m_servantWnd = zbUIMgr.Instance.GetWndById(EWndId.Servant) as zbServantWnd;
		ServantDetail curServant = m_servantWnd.m_detail;
		m_FilteredServantList = (from s in (IEnumerable<ServantDetail>)zbServerData.sGameData.ServantInfo.ServantList
			where s.Quality == curServant.Quality && s.Lock == EBoolState.False && s.Deploy == EBoolState.False && s.ServantId != curServant.ServantId
			orderby zbDataConfig.Instance.servantCsvTable.GetRecordByKey(s.ServantTid).sortId descending
			select s).ToList();
		if (string.IsNullOrEmpty(m_selectedServantId) && m_FilteredServantList.Count > 0)
		{
			m_selectedServantId = m_FilteredServantList[0].ServantId;
		}
		((VisualElement)m_scrollView).schedule.Execute((Action)RefreshListView).StartingIn(10L);
	}

	private void RefreshListView()
	{
		((VisualElement)m_scrollView).Clear();
		foreach (ServantDetail filteredServant in m_FilteredServantList)
		{
			zbServantSelectItem zbServantSelectItem2 = new zbServantSelectItem();
			zbServantSelectItem2.OnEnable(this);
			zbServantSelectItem2.Refresh(filteredServant);
			((VisualElement)m_scrollView).Add((VisualElement)(object)zbServantSelectItem2);
		}
	}

	public void OnClose()
	{
		m_selectedServantId = null;
		Hide();
	}

	private void OnConfirm()
	{
		Hide();
		m_servantWnd.m_modulModule.m_costServantId = m_selectedServantId;
		m_servantWnd.m_modulModule.RefreshSelectServant();
		m_selectedServantId = null;
	}
}
