using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbChatLegionPanel
{
	private zbChatWnd m_parent;

	private VisualElement m_root;

	public ListView m_listView;

	public ScrollView m_scrollView;

	public float itemHeight = 130f;

	private VisualTreeAsset m_itemTA;

	public zbChatLegionPanel(VisualElement root, zbChatWnd parent)
	{
		m_parent = parent;
		m_root = root;
		m_listView = UQueryExtensions.Q<ListView>(root, "ListView", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(root, (string)null, (string)null);
		((VisualElement)m_listView).visible = false;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = itemHeight;
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/chat/tb_chatworld.uxml");
		InitListView();
	}

	public void Refresh()
	{
	}

	private void InitListView()
	{
		m_listView.makeItem = () => (VisualElement)(object)new zbChatWorldItem(m_itemTA, m_parent);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			if (((BaseVerticalCollectionView)m_listView).itemsSource is List<ChatDetail> list && index >= 0 && index < list.Count)
			{
				ChatDetail chatDetail = list[index];
				if (chatDetail != null)
				{
					(element as zbChatWorldItem)?.Init(m_parent, chatDetail);
				}
			}
		};
	}

	public void UpdateViewList(bool scrollBottom = false)
	{
		List<ChatDetail> items = ((IEnumerable<ChatDetail>)zbSelf.Instance.chatHandler.legionDetails).Reverse().ToList();
		((BaseVerticalCollectionView)m_listView).itemsSource = items;
		if (scrollBottom)
		{
			((VisualElement)m_listView).schedule.Execute((Action)delegate
			{
				((BaseVerticalCollectionView)m_listView).ScrollToItem(items.Count - 1);
			}).StartingIn(0L);
		}
	}

	public void OnNewLegionMsg(string item)
	{
		zbWsMsgHandler.Instance.Chat_Send_Req(EChatType.Elegion, 1, item);
	}
}
