using System;
using UnityEngine.UIElements;

public class zbSeasonTargetWnd : zbBaseWnd
{
	private Button m_btnBack;

	private ScrollView m_scrollView;

	private Button m_btnSettlement;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnSettlement = UQueryExtensions.Q<Button>(m_RootElement, "btn_gift", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSettlement, (Action)OnSettlement, "", (TrickleDown)0);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, (string)null, (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom()
	{
		RefreshListView();
	}

	private void RefreshListView()
	{
		((VisualElement)m_scrollView).Clear();
		for (int i = 0; i < 6; i++)
		{
			zbSeasonTargetItem zbSeasonTargetItem2 = new zbSeasonTargetItem(this);
			zbSeasonTargetItem2.Refresh();
			((VisualElement)m_scrollView).Add((VisualElement)(object)zbSeasonTargetItem2);
		}
	}

	private void OnSettlement()
	{
		zbUIMgr.Instance.ShowSeasonSettlement();
	}

	public void OnClose()
	{
		Hide();
	}
}
