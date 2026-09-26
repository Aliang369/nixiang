using System;
using System.Collections.Generic;
using Proto.Types;
using UnityEngine.UIElements;

public class zbMechaConvertWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private ListView m_listView;

	private Button m_btnConfirm;

	private MechaDetail[] m_details;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/mecha/tb_mechaConvert.uxml");
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 158f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(MechaDetail[] _mechaDetails)
	{
		m_details = _mechaDetails;
		UpdateListView();
	}

	public void UpdateListView()
	{
		((VisualElement)m_listView).Clear();
		m_listView.makeItem = () => (VisualElement)(object)new zbMechaConvertItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbMechaConvertItem).Init(m_details[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = m_details;
	}

	private void OnClose()
	{
		if (m_details.Length != 0)
		{
			List<int> list = new List<int>();
			MechaDetail[] details = m_details;
			foreach (MechaDetail mechaDetail in details)
			{
				list.Add(mechaDetail.MechaId);
			}
			zbWsMsgHandler.Instance.Mecha_Convert_Req(list.ToArray());
		}
		Hide();
	}
}
