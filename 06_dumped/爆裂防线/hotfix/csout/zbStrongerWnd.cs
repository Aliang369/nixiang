using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbStrongerWnd : zbBaseWnd
{
	public Button m_btnTouch;

	public Button m_btnClose;

	private ListView m_listView;

	private Action<bool> m_callback;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
		InitListView();
	}

	private void InitListView()
	{
		m_listView.makeItem = delegate
		{
			zbStrongerItem obj = new zbStrongerItem();
			obj.OnEnable(this);
			return (VisualElement)(object)obj;
		};
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbStrongerItem).Refresh(((BaseVerticalCollectionView)m_listView).itemsSource[index] as zbStrongerCsvRecord);
		};
	}

	public void RefreshCustom(Action<bool> callback)
	{
		m_callback = callback;
		List<zbStrongerCsvRecord> list = new List<zbStrongerCsvRecord>();
		foreach (zbStrongerCsvRecord value in zbDataConfig.Instance.strongerCsvTable.GetRecords().records.Values)
		{
			if (value.funcType == 0 || !zbSelf.Instance.funHandler.IsLockNoTip((EFuncType)value.funcType))
			{
				list.Add(value);
			}
		}
		list.Sort((zbStrongerCsvRecord a, zbStrongerCsvRecord b) => a.index.CompareTo(b.index));
		((BaseVerticalCollectionView)m_listView).itemsSource = list.ToArray();
		((BaseVerticalCollectionView)m_listView).RefreshItems();
	}

	public void OnCallBack()
	{
		Hide();
		m_callback?.Invoke(obj: true);
	}

	private void OnClose()
	{
		Hide();
	}
}
