using System;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFacilityEventShopWnd : zbBaseWnd
{
	private Label m_lbTime;

	private MultiColumnListView m_listView;

	private Button m_btnClose;

	public FacilityRoomDetail m_roomDetail;

	private FacilityShopInfo m_shopInfo;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
		m_gameEevent.RegisterEvent(EGameEvent.Event_FacilityShopBuy_Ack, OnEventUpdate);
	}

	public void RefreshCustom(FacilityRoomDetail roomDetail, FacilityShopInfo shopInfo)
	{
		m_roomDetail = roomDetail;
		RefreshList(shopInfo);
		RefreshLeftTime();
	}

	private void RefreshLeftTime()
	{
		if (m_roomDetail != null)
		{
			long num = m_roomDetail.ExplorerExpire - zbSelf.Instance.timeHandler.GetServerTime();
			if (num > 0)
			{
				((TextElement)m_lbTime).text = zbUtil.Language("lang.11274") + ": " + zbUtil.FormatTimeHHMMSS(num);
			}
			else
			{
				((TextElement)m_lbTime).text = zbUtil.Language("lang.11311");
			}
		}
	}

	private void RefreshList(FacilityShopInfo facilityShopInfo)
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		m_shopInfo = facilityShopInfo;
		int count = facilityShopInfo.GoodsList.Count;
		((VisualElement)m_listView).visible = count > 0;
		if (m_listView.columns.Count == 0)
		{
			for (int i = 0; i < 3; i++)
			{
				int columnIndex = i;
				m_listView.columns.Add(new Column
				{
					width = Length.op_Implicit(220f),
					makeCell = () => (VisualElement)(object)new zbFacilityEventShopItem(this),
					bindCell = delegate(VisualElement element, int row)
					{
						//IL_005d: Unknown result type (might be due to invalid IL or missing references)
						//IL_004b: Unknown result type (might be due to invalid IL or missing references)
						int num = row * 3 + columnIndex;
						if (num < m_shopInfo.GoodsList.Count)
						{
							(element as zbFacilityEventShopItem).Refresh(m_shopInfo.GoodsList[num]);
							element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
						}
						else
						{
							element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
						}
					}
				});
			}
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = Enumerable.Range(0, Mathf.CeilToInt((float)m_shopInfo.GoodsList.Count / 3f)).ToList();
		((BaseVerticalCollectionView)m_listView).Rebuild();
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.height = StyleLength.op_Implicit(0f);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
	}

	private void OnClose()
	{
		Hide();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshLeftTime();
	}

	private void OnEventUpdate(object data)
	{
		if (data is FacilityShopBuyAck facilityShopBuyAck)
		{
			RefreshList(facilityShopBuyAck.FacilityShopInfo);
		}
	}
}
