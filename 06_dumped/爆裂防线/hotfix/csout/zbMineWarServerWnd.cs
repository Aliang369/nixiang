using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbMineWarServerWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private readonly List<CrossMineServerDetail> m_sortFriendList = new List<CrossMineServerDetail>();

	private readonly List<zbMineWarServerItem> m_mineWarServerList = new List<zbMineWarServerItem>();

	private ListView m_listView;

	private Button m_btnClose;

	private Button m_btnRule;

	private Label m_lbCrossTime;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/mineWar/tb_mineWarServer.uxml");
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 140f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_lbCrossTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClickClose, "", (TrickleDown)0);
	}

	public void RefreshCustom(CrossMineServerListAck crossMineServerListAck)
	{
		m_sortFriendList.Clear();
		foreach (CrossMineServerDetail crossMineServer in crossMineServerListAck.CrossMineServerList)
		{
			if (Math.Abs(zbSelf.Instance.GetServerId() - crossMineServer.ServerId) <= 5)
			{
				m_sortFriendList.Add(crossMineServer);
			}
		}
		((TextElement)m_lbCrossTime).text = zbUtil.Language("lang.10999") + ":" + zbUtil.FormatUnixTimeToHHMM(zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossStart) + "~" + zbUtil.FormatUnixTimeToHHMM(zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossEnd);
		RefreshList();
		UpdatePerSecond();
		if (!zbGuide.Instance.GetGuideBool(EGuideStep.guide_step_MineWarCross) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.MineWar))
		{
			zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_MineWarCross);
			zbGuide.Instance.ExecGuide(EGuideStep.guide_step_MineWarCross);
		}
	}

	public void RefreshList()
	{
		m_mineWarServerList.Clear();
		m_listView.makeItem = () => (VisualElement)(object)new zbMineWarServerItem(this, m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			zbMineWarServerItem zbMineWarServerItem2 = element as zbMineWarServerItem;
			zbMineWarServerItem2.Init(m_sortFriendList[index]);
			m_mineWarServerList.Add(zbMineWarServerItem2);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = m_sortFriendList;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		foreach (zbMineWarServerItem mineWarServer in m_mineWarServerList)
		{
			mineWarServer.UpdatePerSecond();
		}
	}

	public void OnRule()
	{
		string[] langKeys = new string[9] { "lang.11016", "lang.11017", "lang.11018", "lang.11019", "lang.11020", "lang.11021", "lang.11022", "lang.11023", "lang.11024" };
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	private void OnClickClose(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.MineWar_Data_Req();
		Hide();
	}
}
