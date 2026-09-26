using System.Collections.Generic;
using UnityEngine.UIElements;

public class zbChatFacePanel : VisualElement
{
	private ListView m_listView;

	private zbChatWnd m_wndParent;

	private VisualElement m_root;

	private const int m_faceCount = 40;

	private const int m_rowCount = 8;

	private VisualTreeAsset m_itemTA;

	public zbChatFacePanel(VisualElement root, zbChatWnd wndParent)
	{
		m_root = root;
		m_wndParent = wndParent;
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/chat/tb_chatFace.uxml");
	}

	public void Init()
	{
		m_listView = UQueryExtensions.Q<ListView>(m_root, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_root.visible = false;
		InitFacePanel();
	}

	private void InitFacePanel()
	{
		List<ChatFaceItem> items = GenerateData();
		m_listView.makeItem = () => (VisualElement)(object)new zbChatFaceItem(m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbChatFaceItem).Init(m_wndParent, items[index], this);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = items;
	}

	private List<ChatFaceItem> GenerateData()
	{
		bool flag = false;
		int num = (flag ? 6 : 5);
		List<ChatFaceItem> list = new List<ChatFaceItem>(num);
		for (int i = 0; i < num; i++)
		{
			int rowIndex = i;
			if ((i + 1 == num) & flag)
			{
				int count = 0;
				list.Add(new ChatFaceItem(rowIndex, count));
			}
			else
			{
				list.Add(new ChatFaceItem(rowIndex, 8));
			}
		}
		return list;
	}

	public void Hide()
	{
		m_root.visible = false;
	}

	public void Show()
	{
		m_root.visible = true;
	}
}
