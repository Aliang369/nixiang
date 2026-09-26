using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbJourneyOneRechargeSumWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnClose;

	private Label m_lbTitle;

	private Label m_lbTime;

	private ListView m_listView;

	private Button m_btnFreePack;

	private VisualElement m_iconFreePack;

	private Label m_lbFreePack;

	private VisualElement m_notifyFreePack;

	private JourneyStatisticsDetail m_data;

	private zbOperateCsvRecord m_record;

	private List<JourneyTargetRecordDetail> m_ListViewDatas = new List<JourneyTargetRecordDetail>();

	private Button m_btnTip;

	private Label m_lbTipDesc;

	private AutoHideTipController m_tipController;

	protected override void OnEnable()
	{
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_rechargeDaily.uxml");
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 150f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_btnFreePack = UQueryExtensions.Q<Button>(m_RootElement, "btn_freepack", (string)null);
		m_iconFreePack = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnFreePack, "icon", (string)null);
		m_lbFreePack = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnFreePack, "lb_price", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTip = UQueryExtensions.Q<Button>(m_RootElement, "btn_tipDesc", (string)null);
		((VisualElement)m_btnTip).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_lbTipDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_tipDesc", (string)null);
		m_tipController = new AutoHideTipController(m_uiEvent, m_btnTip, m_lbTipDesc);
		m_tipController.HideImmediate();
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFreePack, (Action)OnFreePack, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_JourneyInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Aux_FreeGiftAward, OnUpdateEventAuxFreeGift);
		m_notifyFreePack = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnFreePack, (Vector2?)new Vector2(-5f, 12f), 1f, true);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Gold
		});
	}

	private void OnEventUpdate(object data)
	{
		Refresh();
	}

	public override void Refresh()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		m_data = zbServerData.sGameData?.JourneyInfo?.ChapterOne?.RechargeSum;
		if (m_data != null)
		{
			((TextElement)m_lbTitle).text = zbUtil.Language("lang.10807");
			m_notifyFreePack.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyOneHandler.HasRechargeFreePack()));
			((VisualElement)m_btnFreePack).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyOneHandler.HasRechargeFreePack()));
			RefreshList();
			RefreshDailyFree();
		}
	}

	private void RefreshDailyFree()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		zbFreeGiftCsvRecord recordByKey = zbDataConfig.Instance.freeGiftCsvTable.GetRecordByKey(5);
		string strIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)recordByKey.award[0], recordByKey.award[1]).strIconPath;
		m_iconFreePack.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(strIconPath));
		((TextElement)m_lbFreePack).text = "X" + recordByKey.award[2];
	}

	private void RefreshList()
	{
		m_ListViewDatas.Clear();
		foreach (JourneyTargetRecordDetail target in m_data.TargetList)
		{
			m_ListViewDatas.Add(target);
		}
		m_listView.makeItem = () => (VisualElement)(object)new zbJourneyOneRechargeSumItem(this, m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbJourneyOneRechargeSumItem).Refresh(m_ListViewDatas[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = m_ListViewDatas;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
	}

	private void OnClose()
	{
		Hide();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		UpdateTime();
	}

	private void OnFreePack()
	{
		if (zbSelf.Instance.journeyOneHandler.HasRechargeFreePack())
		{
			zbWsMsgHandler.Instance.Aux_FreeGiftAward_Req(5);
		}
	}

	private void UpdateTime()
	{
		long valueOrDefault = (zbServerData.sGameData?.JourneyInfo?.ChapterOne?.BaseInfo?.EndTm).GetValueOrDefault();
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		((TextElement)m_lbTime).text = ((valueOrDefault > serverTime) ? zbUtil.FormatTimeHHMMSS(valueOrDefault - serverTime) : zbUtil.Language("lang.10345"));
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		zbWsMsgHandler.Instance.Journey_Data_Req();
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		zbWsMsgHandler.Instance.Journey_Data_Req();
	}

	private void OnUpdateEventAuxFreeGift(object data)
	{
		Refresh();
	}
}
