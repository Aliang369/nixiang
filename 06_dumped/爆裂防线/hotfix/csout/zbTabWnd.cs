using System;
using UnityEngine.UIElements;

public class zbTabWnd : zbBaseWnd
{
	private Button[] m_tabButtons;

	private VisualElement[] m_tabLayers;

	private Action<int> m_switchCB;

	private int m_tabCount;

	[NonSerialized]
	public int m_currTab = -1;

	[NonSerialized]
	public int m_lastTab = -1;

	public void InitTab(Button[] tabButtons, VisualElement[] tabLayers, Action<int> switchCB, int selectIndex = 0)
	{
		if (tabButtons.Length != tabLayers.Length)
		{
			return;
		}
		m_switchCB = switchCB;
		m_tabCount = tabButtons.Length;
		m_tabButtons = tabButtons;
		m_tabLayers = tabLayers;
		for (int i = 0; i < m_tabCount; i++)
		{
			int index = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_tabButtons[i], (Action<ClickEvent>)delegate
			{
				OnSwitchTab(index);
			}, "", (TrickleDown)0);
		}
		if (selectIndex >= 0 && selectIndex <= m_tabCount - 1)
		{
			SelectTab(selectIndex);
		}
		else
		{
			SelectTab(0);
		}
	}

	private void OnSwitchTab(int tabIndex)
	{
		if (tabIndex != m_currTab)
		{
			SelectTab(tabIndex);
		}
	}

	public void SelectTab(int tabIndex)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (tabIndex >= 0 && tabIndex <= m_tabCount - 1 && IsCanOpen(tabIndex))
		{
			if (m_switchCB != null)
			{
				m_switchCB(tabIndex);
			}
			for (int i = 0; i < m_tabCount; i++)
			{
				m_tabLayers[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			m_tabLayers[tabIndex].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_lastTab = m_currTab;
			m_currTab = tabIndex;
			DoSelectTab();
			zbProgressEventHelper.Instance.SetProgressEvent2(GetWndId(), tabIndex + 1);
		}
	}

	public virtual bool IsCanOpen(int tabIndex)
	{
		return true;
	}

	public virtual void DoSelectTab()
	{
	}
}
