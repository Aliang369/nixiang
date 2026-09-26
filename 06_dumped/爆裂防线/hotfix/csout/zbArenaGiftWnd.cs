using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class zbArenaGiftWnd : zbBaseWnd
{
	private ListView m_listView;

	private ScrollView m_scrollView;

	private Button m_btnClose;

	private Dictionary<int, zbArenaGiftItem> m_layerItemMap = new Dictionary<int, zbArenaGiftItem>();

	private int m_targetLayer = -1;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Hero, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_HeroFrag, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_ArenaInfo, OnEventUpdate);
		InitListView();
	}

	public void RefreshCustom(bool isFirstIn = false)
	{
		UpdateViewList(isFirstIn);
		CheckFromGetWayBuyPanel();
	}

	public void CheckFromGetWayBuyPanel()
	{
		int targetId = zbSelf.Instance.getWayHandler.GetWayBuyIndexOrId;
		if (targetId < 0)
		{
			return;
		}
		int layer = zbDataConfig.Instance.arenaGiftCsvTable.GetRecords().records.Values.FirstOrDefault((zbArenaGiftCsvRecord r) => r.id == targetId).layer;
		if (!(((BaseVerticalCollectionView)m_listView).itemsSource is List<int> { Count: not 0 } list))
		{
			return;
		}
		int num = -1;
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			if (list[num2] == layer)
			{
				num = num2;
				break;
			}
		}
		if (num >= 0)
		{
			m_targetLayer = layer;
			((MonoBehaviour)this).StartCoroutine(DelayedScroll(num, targetId));
		}
	}

	private IEnumerator DelayedScroll(int row, int targetId)
	{
		yield return null;
		((BaseVerticalCollectionView)m_listView).ScrollToItem(row);
		yield return null;
		if (m_layerItemMap.TryGetValue(m_targetLayer, out var value))
		{
			int num = 0;
			zbArenaGiftCsvRecord[] records = value.m_records;
			for (int i = 0; i < records.Length && records[i].id != targetId; i++)
			{
				num++;
			}
			zbUIMgr.Instance.ShowGuideFinger(value.m_spriteBgs[num]);
			zbSelf.Instance.getWayHandler.GetWayBuyIndexOrId = -1;
		}
	}

	private void InitListView()
	{
		m_listView.makeItem = () => (VisualElement)(object)new zbArenaGiftItem(this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			if (((BaseVerticalCollectionView)m_listView).itemsSource is List<int> list && index >= 0 && index < list.Count)
			{
				zbArenaGiftItem zbArenaGiftItem2 = element as zbArenaGiftItem;
				zbArenaGiftItem2?.Refresh(list[index]);
				m_layerItemMap[list[index]] = zbArenaGiftItem2;
			}
		};
	}

	public void UpdateViewList(bool isFirstIn)
	{
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		m_layerItemMap.Clear();
		List<int> list = (from layer in zbDataConfig.Instance.arenaGiftCsvTable.GetRecords().records.Values.Select((zbArenaGiftCsvRecord r) => r.layer).Distinct()
			orderby layer
			select layer).ToList();
		((BaseVerticalCollectionView)m_listView).itemsSource = list;
		if (!isFirstIn)
		{
			return;
		}
		int num = list.Count;
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			if (zbServerData.sGameData.ArenaInfo.BestRank <= list[num2])
			{
				num = math.max(0, num2 - 1);
				break;
			}
		}
		float fixedItemHeight = ((BaseVerticalCollectionView)m_listView).fixedItemHeight;
		float num3 = Mathf.Min((float)num * fixedItemHeight, 1295f);
		m_scrollView.scrollOffset = new Vector2(0f, Mathf.Max(0f, num3));
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom();
	}
}
