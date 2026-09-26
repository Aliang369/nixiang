using System;
using System.Collections;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbResourceExchangeWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnBack;

	private VisualElement m_spriteBanner;

	private Label m_lbTitle;

	private Label m_lbTime;

	private ResourceExchangeDetail m_data;

	private ListView m_listview;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_resourceExchangeItem.uxml");
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_listview = UQueryExtensions.Q<ListView>(m_RootElement, "ListView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listview, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_spriteBanner = UQueryExtensions.Q<VisualElement>(m_RootElement, "banner", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnUpdateEventOperate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Prop, OnUpdateEventOperate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Item, OnUpdateEventOperate);
	}

	public void RefreshCustom(ResourceExchangeDetail data)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		Init();
		zbOperateCsvRecord recordByKey = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(22);
		if (recordByKey != null)
		{
			m_spriteBanner.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.banner));
			((TextElement)m_lbTitle).text = recordByKey.name;
		}
		RefreshTime();
	}

	private void Init()
	{
		UpdateListview();
	}

	private void OnClose()
	{
		Hide();
	}

	private void UpdateListview()
	{
		((VisualElement)m_listview).Clear();
		m_listview.makeItem = () => (VisualElement)(object)new zbResourceExchangeItem(this, m_itemTA);
		m_listview.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbResourceExchangeItem).Init(m_data.ExchangeList[index], m_data);
		};
		((BaseVerticalCollectionView)m_listview).itemsSource = (IList)m_data.ExchangeList;
	}

	private void RefreshTime()
	{
		if (m_data != null)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(m_data.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}

	private void OnUpdateEventOperate(object data)
	{
		if (IsShow())
		{
			ResourceExchangeDetail resourceExchangeInfoByUUID = zbSelf.Instance.operateHandler.GetResourceExchangeInfoByUUID(m_data.BaseInfo.UUID);
			RefreshCustom(resourceExchangeInfoByUUID);
		}
	}
}
