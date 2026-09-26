using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbChatSystemPanel
{
	private zbChatWnd m_parent;

	private VisualElement m_root;

	private ListView m_listView;

	private float itemHeight = 110f;

	private List<string> m_chatDatas = new List<string>();

	private VisualTreeAsset m_itemTA;

	public zbChatSystemPanel(VisualElement root, zbChatWnd parent)
	{
		m_parent = parent;
		m_root = root;
		m_listView = UQueryExtensions.Q<ListView>(root, "ListView", (string)null);
		((VisualElement)m_listView).visible = false;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = itemHeight;
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/chat/tb_chatworld.uxml");
	}

	public void Refresh()
	{
		DateTimeOffset.UtcNow.AddDays(-1.0).ToUnixTimeSeconds();
		zbWsMsgHandler.Instance.Chat_Data_Req(EChatType.Esystem, 0);
	}

	public void UpdateListView()
	{
		m_chatDatas.Clear();
		for (int i = 0; i < 29; i++)
		{
			m_chatDatas.Add("ahahah" + i);
		}
		((VisualElement)m_listView).visible = m_chatDatas.Count > 0;
		if (m_chatDatas.Count == 0)
		{
			return;
		}
		m_listView.makeItem = () => (VisualElement)(object)new zbChatSystemItem(m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			if (m_chatDatas[index] != null)
			{
				(element as zbChatSystemItem).Init(m_parent, m_chatDatas[index]);
			}
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = m_chatDatas;
	}
}
