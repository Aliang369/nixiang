using System.Collections.Generic;
using Proto.Types;
using UnityEngine.UIElements;

public class zbNormalMailTab
{
	private List<MailDetail> m_sortedMailList;

	private int m_currTab;

	private ListView m_listView;

	private zbMailWnd m_parent;

	public zbNormalMailTab(ListView listView, zbMailWnd parent)
	{
		m_listView = listView;
		m_parent = parent;
	}

	public void Refresh(List<MailDetail> _sortedMailList)
	{
		m_sortedMailList = _sortedMailList;
		RefreshList();
	}

	public void RefreshList()
	{
		((VisualElement)m_listView).Clear();
		((VisualElement)m_listView).visible = true;
		m_listView.makeItem = () => (VisualElement)(object)new zbMailItem(m_parent, m_parent.m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			if (m_sortedMailList[index] != null)
			{
				(element as zbMailItem).Init(m_sortedMailList[index]);
			}
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = m_sortedMailList;
	}
}
