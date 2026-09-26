using System;
using System.Collections.Generic;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFootholdConvertWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnTouch;

	private VisualElement m_content;

	private Label m_lbTitle;

	private ListView m_listView;

	private Label m_lbDesc;

	private Button m_btnConfirm;

	private FootholdDetail[] m_footholdDetails;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/guard/tb_footholdConvert.uxml");
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_content = UQueryExtensions.Q<VisualElement>(m_RootElement, "content", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 158f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(FootholdDetail[] footholdDetails)
	{
		m_footholdDetails = footholdDetails;
		InitBg();
		UpdateListView();
	}

	private void InitBg()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(m_footholdDetails[0].FootholdId);
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/bg/convert_bg4.jpg");
		m_content.style.backgroundImage = new StyleBackground(val);
	}

	public void UpdateListView()
	{
		((VisualElement)m_listView).Clear();
		m_listView.makeItem = () => (VisualElement)(object)new zbFootholdConvertItem(this, m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbFootholdConvertItem).Init(m_footholdDetails[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = m_footholdDetails;
	}

	private void OnClose()
	{
		if (m_footholdDetails.Length != 0)
		{
			List<int> list = new List<int>();
			FootholdDetail[] footholdDetails = m_footholdDetails;
			foreach (FootholdDetail footholdDetail in footholdDetails)
			{
				list.Add(footholdDetail.FootholdId);
			}
			zbWsMsgHandler.Instance.Foothold_Convert_Req(list.ToArray());
		}
		Hide();
	}
}
