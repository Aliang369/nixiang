using System;
using System.Collections.Generic;
using Proto.Types;
using UnityEngine.UIElements;

public class zbWorldLairListWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	public Button m_btnBack;

	private ListView m_listView;

	private VisualElement m_listNo;

	private List<WorldLairHelpInfo> m_worldLairInfos;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/worldLairFIght/tb_worldLairListItem.uxml");
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		m_listNo = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_listno", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom(List<WorldLairHelpInfo> worldLairInfos)
	{
		m_worldLairInfos = worldLairInfos;
		UpdateListView();
	}

	private void UpdateListView()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_listView).Clear();
		m_listNo.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_worldLairInfos.Count > 0));
		m_listView.makeItem = () => (VisualElement)(object)new zbWorldLairListItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbWorldLairListItem).Init(m_worldLairInfos[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = m_worldLairInfos;
	}

	private void OnClose()
	{
		Hide();
	}
}
