using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbJiziWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnClose;

	private Button m_btnProba;

	private VisualElement m_spriteBanner;

	private Label m_lbTitle;

	private Label m_lbTime;

	private Label m_lbDesc;

	private zbItem[] m_zbItems = new zbItem[6];

	private ListView m_listView;

	private JiZiDetail m_data;

	private Dictionary<int, int> m_batchCountMap = new Dictionary<int, int>();

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_jiZiItem.uxml");
		m_spriteBanner = UQueryExtensions.Q<VisualElement>(m_RootElement, "banner", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnProba = UQueryExtensions.Q<Button>(m_RootElement, "btn_probability", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		((TextElement)m_lbDesc).text = zbUtil.Language("lang.10944");
		for (int i = 0; i < 6; i++)
		{
			m_zbItems[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_zbItems[i].OnEnable(this);
		}
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "ListView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnProba, (Action)OnClickProba, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnUpdateEventOperate);
	}

	public void RefreshCustom(JiZiDetail data)
	{
		m_data = data;
		Init();
		zbOperateCsvRecord recordByKey = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(16);
		if (recordByKey != null)
		{
			((TextElement)m_lbTitle).text = recordByKey.name;
		}
		RefreshTime();
	}

	private void Init()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 6; i++)
		{
			bool flag = i < m_data.PropList.Count;
			((VisualElement)m_zbItems[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				bool flag2 = zbSelf.Instance.itemHandler.GetCountByPropTid(m_data.PropList[i].Id) > 0;
				zbItemData data = new zbItemData(EResourceType.Prop, m_data.PropList[i].Id, zbSelf.Instance.itemHandler.GetCountByPropTid(m_data.PropList[i].Id));
				m_zbItems[i].Refresh(data, isNameVis: false, flag2);
				m_zbItems[i].SetGray(!flag2);
			}
		}
		UpdateListView();
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnClickProba()
	{
		List<Dictionary<string, float>> list = new List<Dictionary<string, float>>();
		foreach (OperateJiZiPropDetail prop in m_data.PropList)
		{
			Dictionary<string, float> dictionary = new Dictionary<string, float>();
			dictionary["id"] = prop.Id;
			dictionary["weight"] = prop.Weight;
			list.Add(dictionary);
		}
		zbUIMgr.Instance.ShowJiziProbability(list.ToArray());
	}

	private void UpdateListView()
	{
		((VisualElement)m_listView).Clear();
		m_listView.makeItem = () => (VisualElement)(object)new zbJiziItem(this, m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbJiziItem).Init(m_data.ExchangeList[index], m_data);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = (IList)m_data.ExchangeList;
	}

	private void OnUpdateEventOperate(object data)
	{
		JiZiDetail jiZiYouLiByUUID = zbSelf.Instance.operateHandler.GetJiZiYouLiByUUID(m_data.BaseInfo.UUID);
		RefreshCustom(jiZiYouLiByUUID);
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

	public int GetBatchCount(int index)
	{
		if (m_batchCountMap.TryGetValue(index, out var value))
		{
			return value;
		}
		return 1;
	}

	public void SetBatchCount(int index, int count)
	{
		m_batchCountMap[index] = count;
	}
}
