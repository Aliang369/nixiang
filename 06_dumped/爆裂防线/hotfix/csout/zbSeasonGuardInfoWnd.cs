using System;
using UnityEngine.UIElements;

public class zbSeasonGuardInfoWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private ScrollView m_listView;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_listView = UQueryExtensions.Q<ScrollView>(m_RootElement, (string)null, (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom()
	{
		RefreshListView();
	}

	private void RefreshListView()
	{
		((VisualElement)m_listView).Clear();
		int[] array = new int[5];
		for (int i = 0; i < array.Length; i++)
		{
			zbSeasonGuardInfoItem zbSeasonGuardInfoItem2 = new zbSeasonGuardInfoItem();
			zbSeasonGuardInfoItem2.OnEnable(this);
			zbSeasonGuardInfoItem2.Init();
			((VisualElement)m_listView).Add((VisualElement)(object)zbSeasonGuardInfoItem2);
		}
	}

	public void OnClose()
	{
		Hide();
	}
}
