using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbHomeCreateWnd : zbBaseWnd
{
	private MultiColumnListView m_listView;

	private Button m_btnBag;

	private Button m_btnClose;

	private VisualElement m_notifyBag;

	private List<zbHomeRoomTypeCsvRecord> m_listItemDatas = new List<zbHomeRoomTypeCsvRecord>();

	private List<int> rowIndexes = new List<int>();

	[SerializeField]
	private int columnCount = 2;

	private int rowCount;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).mode = (ScrollViewMode)0;
		m_btnBag = UQueryExtensions.Q<Button>(m_RootElement, "btn_bag", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBag, (Action<ClickEvent>)OnBag, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_notifyBag = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnBag);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Gold
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_HomelandRoomCreate_Ack, OnCreateAck);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Box, RefreshNotify);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Item, RefreshNotify);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Prop, RefreshNotify);
	}

	public override void Refresh()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		base.Refresh();
		RefreshNotify(null);
		m_listView.columns.Clear();
		for (int i = 0; i < columnCount; i++)
		{
			int columnIndex = i;
			m_listView.columns.Add(new Column
			{
				width = Length.op_Implicit(340f),
				makeCell = () => (VisualElement)(object)new zbHomeCreateItem(this),
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_0063: Unknown result type (might be due to invalid IL or missing references)
					//IL_0051: Unknown result type (might be due to invalid IL or missing references)
					int num = row * columnCount + columnIndex;
					if (num < m_listItemDatas.Count)
					{
						(element as zbHomeCreateItem)?.Refresh(m_listItemDatas[num]);
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					}
					else
					{
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					}
				}
			});
		}
		GenerateData();
	}

	private void GenerateData()
	{
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		m_listItemDatas.Clear();
		foreach (zbHomeRoomTypeCsvRecord value in zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecords().records.Values)
		{
			if (value.sortIndex > 0 && zbServerData.sGameData.PeerageInfo.PeerageId >= value.peerageId)
			{
				m_listItemDatas.Add(value);
			}
		}
		m_listItemDatas.Sort((zbHomeRoomTypeCsvRecord a, zbHomeRoomTypeCsvRecord b) => a.sortIndex.CompareTo(b.sortIndex));
		rowIndexes.Clear();
		rowCount = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		for (int num = 0; num < rowCount; num++)
		{
			rowIndexes.Add(num);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
		((VisualElement)m_listView).style.position = StyleEnum<Position>.op_Implicit((Position)1);
	}

	private void RefreshNotify(object obj)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		m_notifyBag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Bag)));
	}

	private void OnBag(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowBag();
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		Refresh();
	}

	private void OnCreateAck(object data)
	{
		Hide();
	}
}
