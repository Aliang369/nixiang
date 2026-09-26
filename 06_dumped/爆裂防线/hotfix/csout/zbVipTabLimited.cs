using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbVipTabLimited
{
	public class RechargeGiftWrapper
	{
		public int GiftType;

		public RechargeGoodsDetail GoodsDetail;

		public RechargeDirectDetail DirectDetail;

		public OperateBaseInfo BaseInfo;

		public FreeGiftDetail freeGiftDetail;
	}

	private VisualTreeAsset m_itemTA;

	private zbVipWnd m_wndParent;

	private VisualElement m_RootElement;

	private VisualElement m_spriteBanner;

	private Button m_btnCall;

	private Label m_lbTitle;

	private Label m_lbTime;

	private ListView m_listView;

	private List<zbVipTabLimitedItem> m_activeItems = new List<zbVipTabLimitedItem>();

	private List<RechargeGiftDetail> m_giftList = new List<RechargeGiftDetail>();

	private string m_uuid;

	private List<RechargeGiftWrapper> m_wrappedList = new List<RechargeGiftWrapper>();

	private int m_gotoIndex = -1;

	public void OnEnable(zbVipWnd wndParent, VisualElement root)
	{
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_rechargeGift.uxml");
		m_wndParent = wndParent;
		m_RootElement = root;
		m_spriteBanner = UQueryExtensions.Q<VisualElement>(m_RootElement, "banner", (string)null);
		m_btnCall = UQueryExtensions.Q<Button>(m_RootElement, "btn_call", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)1;
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCall, (Action)OnCall, "", (TrickleDown)0);
		wndParent.CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
		wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnEventUpdate);
		InitRecord();
	}

	private void InitRecord()
	{
		zbOperateCsvRecord recordByKey = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(4);
		if (recordByKey != null)
		{
			((TextElement)m_lbTitle).text = recordByKey.name;
		}
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(m_uuid);
	}

	public void RefreshCustom(string uuid)
	{
		m_uuid = uuid;
		m_giftList.Clear();
		RepeatedField<RechargeGiftDetail> val = zbServerData.sGameData.OperateInfo?.RechargeGiftList;
		if (val == null)
		{
			return;
		}
		RechargeGiftDetail rechargeGiftDetail = null;
		foreach (RechargeGiftDetail item in val)
		{
			if (item.BaseInfo.UUID == uuid)
			{
				rechargeGiftDetail = item;
				break;
			}
		}
		if (rechargeGiftDetail == null)
		{
			return;
		}
		foreach (RechargeGiftDetail item2 in val)
		{
			if (zbSelf.Instance.operateHandler.IsShowTime(item2.BaseInfo) && item2.BaseInfo.Type != EOperatePlanType.Holiday)
			{
				m_giftList.Add(item2);
			}
		}
		RefreshList();
		CheckFromGetWayBuyPanel();
	}

	public void CheckFromGetWayBuyPanel()
	{
		int getWayBuyIndexOrId = zbSelf.Instance.getWayHandler.GetWayBuyIndexOrId;
		if (getWayBuyIndexOrId < 0)
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < m_wrappedList.Count; i++)
		{
			RechargeGiftWrapper rechargeGiftWrapper = m_wrappedList[i];
			if (rechargeGiftWrapper.GiftType == 1 && rechargeGiftWrapper.GoodsDetail != null && rechargeGiftWrapper.GoodsDetail.GoodsId == getWayBuyIndexOrId)
			{
				num = i;
				break;
			}
			if (rechargeGiftWrapper.GiftType == 2 && rechargeGiftWrapper.DirectDetail != null && rechargeGiftWrapper.DirectDetail.DirectId == getWayBuyIndexOrId)
			{
				num = i;
				break;
			}
		}
		if (num >= 0)
		{
			m_gotoIndex = num;
			((BaseVerticalCollectionView)m_listView).ScrollToItem(num);
			((BaseVerticalCollectionView)m_listView).RefreshItems();
		}
	}

	public void RefreshList()
	{
		m_gotoIndex = -1;
		foreach (zbVipTabLimitedItem activeItem in m_activeItems)
		{
			activeItem.Dispose();
		}
		m_activeItems.Clear();
		((VisualElement)m_listView).Clear();
		((VisualElement)m_listView).visible = false;
		m_wrappedList.Clear();
		OperateBaseInfo operateBaseInfo = ((m_giftList.Count > 0) ? m_giftList[0].BaseInfo : null);
		foreach (RechargeGiftDetail gift in m_giftList)
		{
			if (gift.GoodsList != null)
			{
				foreach (RechargeGoodsDetail goods in gift.GoodsList)
				{
					m_wrappedList.Add(new RechargeGiftWrapper
					{
						GiftType = 1,
						GoodsDetail = goods,
						BaseInfo = gift.BaseInfo
					});
				}
			}
			if (gift.DirectList == null)
			{
				continue;
			}
			foreach (RechargeDirectDetail direct in gift.DirectList)
			{
				m_wrappedList.Add(new RechargeGiftWrapper
				{
					GiftType = 2,
					DirectDetail = direct,
					BaseInfo = gift.BaseInfo
				});
			}
		}
		if (zbServerData.sGameData.FreeGiftList != null && operateBaseInfo != null)
		{
			foreach (FreeGiftDetail freeGift in zbServerData.sGameData.FreeGiftList)
			{
				if (freeGift.FreeType == 1)
				{
					m_wrappedList.Add(new RechargeGiftWrapper
					{
						GiftType = 3,
						freeGiftDetail = freeGift,
						BaseInfo = operateBaseInfo
					});
				}
			}
		}
		if (m_wrappedList.Count == 0)
		{
			return;
		}
		((VisualElement)m_listView).visible = true;
		m_listView.makeItem = delegate
		{
			zbVipTabLimitedItem zbVipTabLimitedItem2 = new zbVipTabLimitedItem(m_wndParent, m_itemTA);
			m_activeItems.Add(zbVipTabLimitedItem2);
			return (VisualElement)(object)zbVipTabLimitedItem2;
		};
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			zbVipTabLimitedItem item = element as zbVipTabLimitedItem;
			if (item != null)
			{
				item.Init(m_wrappedList[index]);
				if (index == m_gotoIndex)
				{
					m_gotoIndex = -1;
					((VisualElement)item).schedule.Execute((Action)delegate
					{
						zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)item.m_btnBuy);
						zbSelf.Instance.getWayHandler.GetWayBuyIndexOrId = -1;
					}).StartingIn(100L);
					m_gotoIndex = -1;
				}
			}
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = m_wrappedList;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
	}

	public void UpdateAtShow()
	{
		if (m_uuid != "")
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(zbSelf.Instance.operateHandler.GetDirectGiftLeftTime(m_uuid));
		}
	}

	private void OnCall()
	{
		zbUIMgr.Instance.ShowSupportContact();
	}
}
