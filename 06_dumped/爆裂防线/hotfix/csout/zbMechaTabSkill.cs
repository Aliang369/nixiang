using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine.UIElements;

public class zbMechaTabSkill
{
	private VisualTreeAsset m_itemTA;

	private zbMechaWnd m_wndParent;

	private VisualElement m_RootElement;

	private ListView m_listView;

	private List<int> m_listItemDatas = new List<int>();

	public void OnEnable(zbMechaWnd wndParent, VisualElement root)
	{
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/mecha/tb_mechaSkill.uxml");
		m_wndParent = wndParent;
		m_RootElement = root;
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, (string)null, (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 140f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		InitListView();
	}

	private void InitListView()
	{
		m_listView.makeItem = () => (VisualElement)(object)new zbMechaSkillItem(m_itemTA, m_wndParent);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbMechaSkillItem).Refresh(m_listItemDatas[index], index);
		};
	}

	public void Refresh()
	{
		if (m_wndParent.m_record != null)
		{
			m_listItemDatas.Clear();
			m_listItemDatas = ((IEnumerable<MechaSkillDetail>)((IEnumerable<MechaDetail>)zbServerData.sGameData.MechaInfo.MechaList).FirstOrDefault((MechaDetail mecha) => mecha.MechaId == m_wndParent.m_record.id).SkillList).Select((MechaSkillDetail skill) => skill.SkillId).ToList();
			((BaseVerticalCollectionView)m_listView).itemsSource = null;
			((BaseVerticalCollectionView)m_listView).itemsSource = m_listItemDatas;
		}
	}
}
