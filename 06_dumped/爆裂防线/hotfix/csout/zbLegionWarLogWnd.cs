using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLegionWarLogWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private ListView m_listView;

	private VisualElement m_listNo;

	private Button m_btnClose;

	private LegionWarRecordDetail[] m_details;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/legionWar/tb_legionWarLog.uxml");
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_listNo = UQueryExtensions.Q<VisualElement>(m_RootElement, "list_no", (string)null);
		OnEnableListView();
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom()
	{
		m_details = zbSelf.Instance.legionWarHandler.legionWarRecordDetails;
		RefreshListView();
	}

	private void RefreshListView()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_listView).Clear();
		bool flag = m_details != null && m_details.Length != 0;
		m_listNo.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		((VisualElement)m_listView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		if (flag)
		{
			((BaseVerticalCollectionView)m_listView).itemsSource = m_details;
		}
	}

	private void OnEnableListView()
	{
		m_listView.makeItem = () => (VisualElement)(object)new zbLegionWarLogItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbLegionWarLogItem).Init(((BaseVerticalCollectionView)m_listView).itemsSource[index] as LegionWarRecordDetail);
		};
	}

	private void OnClose()
	{
		Hide();
	}
}
