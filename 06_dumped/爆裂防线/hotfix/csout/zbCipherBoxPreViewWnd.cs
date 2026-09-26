using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCipherBoxPreViewWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnTouch;

	private Button m_btnClose;

	private ListView m_listView;

	private CipherBoxDetail m_cipherBoxDetail;

	public Sprite[] bg_nums = (Sprite[])(object)new Sprite[5];

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_cipherBoxPreview.uxml");
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, (string)null, (string)null);
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 117f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		for (int i = 0; i < 5; i++)
		{
			bg_nums[i] = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/operate/cipherbox/cipherbox_" + (i + 1) + ".png");
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(CipherBoxDetail cipherBoxDetail)
	{
		m_cipherBoxDetail = cipherBoxDetail;
		UpdateListView(((IEnumerable<CipherBoxTargetDetail>)m_cipherBoxDetail.TargetList).ToArray());
	}

	private void UpdateListView(CipherBoxTargetDetail[] items)
	{
		((VisualElement)m_listView).Clear();
		m_listView.makeItem = () => (VisualElement)(object)new zbCipherBoxPreviewItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbCipherBoxPreviewItem).Init(items[index], index + 1);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = items;
	}

	private void OnClose()
	{
		Hide();
	}
}
