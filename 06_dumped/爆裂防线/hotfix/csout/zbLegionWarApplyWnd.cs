using System;
using System.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarApplyWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private ListView m_eastListView;

	private ListView m_westListView;

	private ScrollView m_eastScrollView;

	private VisualElement m_noListView;

	private Button m_btnBack;

	private Button m_btnRule;

	private Button m_btnApply;

	private Button m_btnAward;

	private Label m_lbState;

	private Label m_lbTime;

	private Button m_btnLegionWarTask;

	private VisualElement m_taskNotify;

	private Label m_lbLegionWarLeftTime;

	private int m_lastDataTime = 5;

	private int m_maxDataTime = 5;

	private float lastValue;

	public bool isLoading;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/legionWar/tb_legionWarApply.uxml");
		m_eastListView = UQueryExtensions.Q<ListView>(m_RootElement, "listViewEast", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_eastListView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_eastListView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_eastListView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_westListView = UQueryExtensions.Q<ListView>(m_RootElement, "listViewWest", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_westListView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_westListView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_westListView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_noListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "noListView", (string)null);
		m_eastScrollView = UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_eastListView, (string)null, (string)null);
		m_btnLegionWarTask = UQueryExtensions.Q<Button>(m_RootElement, "btn_legionWarTask", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLegionWarTask, (Action)OnTask, "", (TrickleDown)0);
		m_taskNotify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnLegionWarTask);
		m_lbLegionWarLeftTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_legionWarLeftTime", (string)null);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_btnApply = UQueryExtensions.Q<Button>(m_RootElement, "btn_apply", (string)null);
		m_btnAward = UQueryExtensions.Q<Button>(m_RootElement, "btn_award", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAward, (Action)OnAward, "", (TrickleDown)0);
		m_lbState = UQueryExtensions.Q<Label>(m_RootElement, "lb_state", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnApply, (Action)OnApply, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_LegionWarSelfInfo, OnEventUpdate);
		InitListView();
	}

	public override void Refresh()
	{
		isLoading = false;
		m_lastDataTime = m_maxDataTime;
		zbWsMsgHandler.Instance.LegionWar_SignUpList_Req(0);
		RefreshNotify();
	}

	public void UpdateView()
	{
		UpdateBottom();
		UpdateListView();
		UpdateLeftTime();
	}

	private void RefreshNotify()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		m_taskNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.legionWarHandler.HasTaskNotify()));
	}

	private void UpdateBottom()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		zbLegionWarHandler legionWarHandler = zbSelf.Instance.legionWarHandler;
		_ = zbSelf.Instance.legionHandler;
		ELegionWarState legionWarState = legionWarHandler.GetLegionWarState();
		bool flag = legionWarHandler.legionWarLegionInfo.Signup == EBoolState.True;
		((VisualElement)m_btnApply).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_lbState).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((TextElement)m_lbLegionWarLeftTime).text = zbUtil.Language(flag ? "lang.11287" : "lang.11286");
		switch (legionWarState)
		{
		case ELegionWarState.SignUp:
			if (flag)
			{
				((TextElement)m_lbState).text = zbUtil.Language("lang.11164");
				((VisualElement)m_lbState).style.color = StyleColor.op_Implicit(Color.green);
			}
			else
			{
				((VisualElement)m_btnApply).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_lbState).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			break;
		case ELegionWarState.Prepare:
			if (flag)
			{
				((TextElement)m_lbState).text = zbUtil.Language("lang.11165");
				((VisualElement)m_lbState).style.color = StyleColor.op_Implicit(Color.green);
			}
			else
			{
				((TextElement)m_lbState).text = zbUtil.Language("lang.11261");
				((VisualElement)m_lbState).style.color = StyleColor.op_Implicit(Color.red);
			}
			break;
		}
	}

	private void UpdateLeftTime()
	{
		ELegionWarState legionWarState = zbSelf.Instance.legionWarHandler.GetLegionWarState();
		if (legionWarState == ELegionWarState.Fighting)
		{
			OnClose();
			zbUIMgr.Instance.ShowLegionWarEnter();
		}
		if (legionWarState == ELegionWarState.SignUp || legionWarState == ELegionWarState.Prepare)
		{
			((TextElement)m_lbTime).text = ((zbSelf.Instance.legionWarHandler.GetLeftTimeByZoneAndState(legionWarState) > 0) ? zbUtil.FormatTimeHHMMSS(zbSelf.Instance.legionWarHandler.GetLeftTimeByZoneAndState(legionWarState)) : zbUtil.Language("lang.11228"));
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		UpdateLeftTime();
		m_lastDataTime++;
	}

	private void OnTask()
	{
		zbUIMgr.Instance.ShowLegionWarTask();
	}

	private void InitListView()
	{
		m_eastListView.makeItem = () => (VisualElement)(object)new zbLegionWarApplyItem(m_itemTA, this);
		m_eastListView.bindItem = delegate(VisualElement element, int index)
		{
			IList itemsSource = ((BaseVerticalCollectionView)m_eastListView).itemsSource;
			if (itemsSource != null && index >= 0 && index < itemsSource.Count)
			{
				(element as zbLegionWarApplyItem).Init(itemsSource[index] as LegionBaseInfo);
			}
		};
		m_eastScrollView.verticalScroller.valueChanged += OnScrollEast;
		((BaseVerticalCollectionView)m_eastListView).itemsSource = zbSelf.Instance.legionWarHandler.LegionWarSignUpList;
	}

	private void OnScrollEast(float value)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (value < lastValue)
		{
			lastValue = value;
			return;
		}
		lastValue = value;
		if (!isLoading && !(value < 0f))
		{
			Rect layout = ((VisualElement)m_eastScrollView).contentContainer.layout;
			float height = ((Rect)(ref layout)).height;
			layout = ((VisualElement)m_eastScrollView).layout;
			float num = height - ((Rect)(ref layout)).height;
			if (value >= num - 50f)
			{
				RequestMoreEast();
			}
		}
	}

	private void RequestMoreEast()
	{
		if (m_lastDataTime >= m_maxDataTime)
		{
			m_lastDataTime = 0;
			isLoading = true;
			int num = zbSelf.Instance.legionWarHandler.LegionWarSignUpList.Count / 20;
			zbWsMsgHandler.Instance.LegionWar_SignUpList_Req(num + 1);
		}
	}

	private void UpdateListView()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		((BaseVerticalCollectionView)m_eastListView).RefreshItems();
		flag = zbSelf.Instance.legionWarHandler.LegionWarSignUpList.Count > 0;
		m_noListView.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
	}

	public void OnRule()
	{
		string[] langKeys = new string[1] { "lang.11207" };
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	private void OnApply()
	{
		zbWsMsgHandler.Instance.LegionWar_SignUp_Req();
	}

	private void OnAward()
	{
		long num = 0L;
		long num2 = 0L;
		if (zbSelf.Instance.legionWarHandler.legionWarSettlePlayerInfoAck != null)
		{
			foreach (LegionWarSettlePlayerInfo force in zbSelf.Instance.legionWarHandler.legionWarSettlePlayerInfoAck.ForceList)
			{
				foreach (LegionWarPlayerRankDetail rank in force.RankList)
				{
					if (zbSelf.Instance.GetRoleId() == rank.RoleBaseInfo.RoleId)
					{
						num = rank.RankData.Rank;
						break;
					}
				}
			}
		}
		if (zbSelf.Instance.legionWarHandler.legionWarSettleLegionInfoAck != null)
		{
			foreach (LegionWarSettleLegionInfo force2 in zbSelf.Instance.legionWarHandler.legionWarSettleLegionInfoAck.ForceList)
			{
				foreach (LegionWarLegionRankDetail rank2 in force2.RankList)
				{
					if (zbServerData.sGameData.LegionSelfInfo.LegionId == rank2.LegionFlagInfo.LegionId)
					{
						num2 = rank2.RankData.Rank;
						break;
					}
				}
			}
		}
		zbUIMgr.Instance.ShowLegionWarRankAward((int)num, (int)num2);
	}

	private void OnClose()
	{
		Hide();
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		RefreshNotify();
	}

	private void OnEventUpdate(object data)
	{
		UpdateView();
	}
}
