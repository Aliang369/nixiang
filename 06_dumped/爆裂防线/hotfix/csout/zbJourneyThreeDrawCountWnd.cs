using System;
using System.Linq;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbJourneyThreeDrawCountWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Label m_lbTitle;

	private Label m_lbTime;

	private ListView m_listView;

	private Button m_btnClose;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/journey/journeyThree/tb_journeyThreeDrawCount.uxml");
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		((TextElement)m_lbTitle).text = zbUtil.Language("lang.11173");
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 150f;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_JourneyInfo, OnEventUpdate);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Gold
		});
		InitRecords();
	}

	private void InitRecords()
	{
		zbChapterThreeDrawCountCsvRecord[] records = zbDataConfig.Instance.chapterThreeDrawCountCsvTable.GetRecords().records.Values.ToArray();
		m_listView.makeItem = () => (VisualElement)(object)new zbJourneyThreeDrawCountItem(this, m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbJourneyThreeDrawCountItem).Refresh(records[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = records;
	}

	public override void Refresh()
	{
		((BaseVerticalCollectionView)m_listView).RefreshItems();
		RefreshTime();
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		Refresh();
	}

	private void RefreshTime()
	{
		long endLeftTime = zbSelf.Instance.journeyThreeHandler.GetEndLeftTime();
		if (endLeftTime > 0)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(endLeftTime);
		}
		else
		{
			OnClose();
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
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
}
