using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbVipTabGift
{
	private class DailyPackCell : VisualElement
	{
		public zbDailyPackItem normalItem;

		public DailyPackCell(zbVipWnd parent)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			((VisualElement)this)._002Ector();
			((VisualElement)this).style.backgroundColor = StyleColor.op_Implicit(Color.clear);
			normalItem = new zbDailyPackItem(parent);
			((VisualElement)this).Add((VisualElement)(object)normalItem);
		}
	}

	private zbVipWnd m_wndParent;

	private VisualElement m_rootElement;

	private Label m_lbltitle;

	private Button m_btnbatch;

	private Label m_lbTime;

	private Label m_lbBonus;

	private int m_batchGoodId;

	private ListView m_listView;

	private Button m_btnCall;

	private DailyPackInfo m_dailyPackInfo;

	private DailyPackDetail m_batchInfo;

	private Dictionary<int, zbDailyPackItem> m_dailyPackItemMap = new Dictionary<int, zbDailyPackItem>();

	private int m_targetId = -1;

	public void OnEnable(zbVipWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_rootElement = root;
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_rootElement, "batch", (string)null);
		m_lbltitle = UQueryExtensions.Q<Label>(val, "lbltitle", (string)null);
		m_btnbatch = UQueryExtensions.Q<Button>(val, "btnbuy", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_rootElement, "lb_time", (string)null);
		m_lbBonus = UQueryExtensions.Q<Label>(m_rootElement, "lb_bonus", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnbatch, (Action)OnBuyBatch, "", (TrickleDown)0);
		m_listView = UQueryExtensions.Q<ListView>(m_rootElement, "listpack", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 184f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_btnCall = UQueryExtensions.Q<Button>(m_rootElement, "btn_call", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCall, (Action)OnCall, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		if (zbServerData.sGameData != null && zbServerData.sGameData.RechargeActiveInfo != null)
		{
			m_dailyPackInfo = zbServerData.sGameData.RechargeActiveInfo.DailyPackInfo;
		}
		if (m_dailyPackInfo != null)
		{
			RefreshBatchInfo();
			RefresDailyPackList();
			CheckFromGetWayBuyPanel();
		}
	}

	public void CheckFromGetWayBuyPanel()
	{
		int getWayBuyIndexOrId = zbSelf.Instance.getWayHandler.GetWayBuyIndexOrId;
		if (getWayBuyIndexOrId < 0 || !(((BaseVerticalCollectionView)m_listView).itemsSource is List<DailyPackDetail> { Count: not 0 } list))
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Id == getWayBuyIndexOrId)
			{
				num = i;
				break;
			}
		}
		if (num >= 0)
		{
			m_targetId = getWayBuyIndexOrId;
			((MonoBehaviour)m_wndParent).StartCoroutine(DelayedScroll(num));
		}
	}

	private IEnumerator DelayedScroll(int row)
	{
		yield return null;
		((BaseVerticalCollectionView)m_listView).ScrollToItem(row);
		yield return null;
		if (m_dailyPackItemMap.TryGetValue(m_targetId, out var value))
		{
			zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)value);
			zbSelf.Instance.getWayHandler.GetWayBuyIndexOrId = -1;
		}
	}

	public void RefreshBatchInfo()
	{
		m_batchInfo = zbSelf.Instance.vipHandler.GetBatchDailyPackDetail();
		if (m_batchInfo != null)
		{
			zbVipDailyPackCsvRecord recordByKey = zbDataConfig.Instance.vipDailyPackCsvTable.GetRecordByKey(m_batchInfo.Id);
			if (recordByKey != null)
			{
				m_batchGoodId = recordByKey.goodsId;
				((TextElement)m_btnbatch).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(recordByKey.goodsId);
				((TextElement)m_lbBonus).text = recordByKey.bonus;
				RefreshBatchBtnInfo();
			}
		}
	}

	private void RefreshBatchBtnInfo()
	{
		if (m_batchInfo != null && m_batchInfo.State == EActiveState.Done)
		{
			string refreshTimeStr = zbSelf.Instance.timeHandler.GetRefreshTimeStr();
			if (m_btnbatch != null)
			{
				((TextElement)m_btnbatch).text = refreshTimeStr;
			}
		}
	}

	public void RefresDailyPackList()
	{
		((VisualElement)m_listView).Clear();
		m_dailyPackItemMap.Clear();
		((VisualElement)m_listView).visible = false;
		List<DailyPackDetail> list = new List<DailyPackDetail>();
		foreach (DailyPackDetail pack in m_dailyPackInfo.PackList)
		{
			if (zbDataConfig.Instance.vipDailyPackCsvTable.GetRecordByKey(pack.Id).type != 3)
			{
				list.Add(pack);
			}
		}
		((VisualElement)m_listView).visible = list.Count > 0;
		List<DailyPackDetail> dataSource = new List<DailyPackDetail>(list);
		((BaseVerticalCollectionView)m_listView).itemsSource = dataSource;
		m_listView.makeItem = () => (VisualElement)(object)new DailyPackCell(m_wndParent);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			DailyPackCell dailyPackCell = element as DailyPackCell;
			DailyPackDetail dailyPackDetail = dataSource[index];
			((VisualElement)dailyPackCell.normalItem).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			dailyPackCell.normalItem.Init(dailyPackDetail);
			m_dailyPackItemMap[dailyPackDetail.Id] = dailyPackCell.normalItem;
		};
		((BaseVerticalCollectionView)m_listView).RefreshItems();
	}

	private void OnBuyBatch()
	{
		if (m_batchInfo != null && m_batchInfo.State == EActiveState.Done)
		{
			zbUIMgr.Instance.BubbleInfo("已购买,明日再来");
		}
		else
		{
			zbChannel_Pay.OnPay(m_batchGoodId);
		}
	}

	public void UpdatePerSecond()
	{
		if (m_lbTime != null)
		{
			string refreshTimeStr = zbSelf.Instance.timeHandler.GetRefreshTimeStr();
			((TextElement)m_lbTime).text = refreshTimeStr;
		}
		RefreshBatchBtnInfo();
	}

	private void OnCall()
	{
		zbUIMgr.Instance.ShowSupportContact();
	}
}
