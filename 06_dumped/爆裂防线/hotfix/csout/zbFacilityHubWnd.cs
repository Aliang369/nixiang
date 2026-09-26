using System;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbFacilityHubWnd : zbTabWnd
{
	public VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	public Button[] m_tabButtons = (Button[])(object)new Button[2];

	private VisualElement[] m_notifys = (VisualElement[])(object)new VisualElement[2];

	private Button m_btnRule;

	private Button m_btnWeekCard;

	private Button m_btnZhanLing;

	private Button m_btnClose;

	private VisualElement m_notifyWeekCard;

	private VisualElement m_notifyZhanLing;

	private VisualElement m_spriteTabSel;

	public SpriteRenderer m_bg;

	public SkeletonAnimation m_bgAnim;

	[NonSerialized]
	public zbMineFacility m_moduleMineFacility;

	public zbSearchFacility m_moduleSearchFacility;

	private VisualElement m_notifyMine;

	private VisualElement m_notifySearch;

	[NonSerialized]
	public int m_facilityType = 1;

	protected override void OnEnable()
	{
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		for (int i = 0; i < 2; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_tab" + i, (string)null);
			m_notifys[i] = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[i]);
		}
		m_spriteTabSel = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTab", (string)null);
		((Component)m_GameObjectSceneRoot.transform).GetComponent<SpriteRenderer>();
		m_bg = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponentInChildren<SpriteRenderer>();
		m_bgAnim = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponentInChildren<SkeletonAnimation>();
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_btnWeekCard = UQueryExtensions.Q<Button>(m_RootElement, "btn_weekCard", (string)null);
		m_btnZhanLing = UQueryExtensions.Q<Button>(m_RootElement, "btn_zhanLing", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnWeekCard, (Action)OnWeekCard, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnZhanLing, (Action)OnZhanLing, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_notifyWeekCard = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnWeekCard, (Vector2?)new Vector2(1f, 7f), 1f, true);
		m_notifyZhanLing = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnZhanLing, (Vector2?)new Vector2(1f, 7f), 1f, true);
		m_notifyMine = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[0], (Vector2?)new Vector2(-3f, -10f), 1f, true);
		m_notifySearch = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[1], (Vector2?)new Vector2(-3f, -10f), 1f, true);
		InitTab(m_tabButtons, m_tabLayers, null);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.FacilityChestKey,
			EMoneyType.Tili
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Facility, OnUpdateEvent);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_JourneyInfo, delegate
		{
			RefreshZhanLing();
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Item, OnUpdateEvent);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnUpdateEvent);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Activity, delegate
		{
			RefreshNotify();
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_SuccessOrder_Ack, OnUpdateEvent);
	}

	public void RefreshCustom(int tabIndex = 0, int facilityType = 1)
	{
		SelectTab(tabIndex);
		RefreshZhanLing();
		RefreshNotify();
		CheckHasJoined();
	}

	public override void DoSelectTab()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		base.DoSelectTab();
		switch (m_currTab)
		{
		case 0:
			if (m_moduleMineFacility == null)
			{
				m_moduleMineFacility = new zbMineFacility();
				m_moduleMineFacility.OnEnable(this, m_tabLayers[0]);
			}
			zbWsMsgHandler.Instance.Facility_Data_Req();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			break;
		case 1:
			if (m_moduleSearchFacility == null)
			{
				m_moduleSearchFacility = new zbSearchFacility();
				m_moduleSearchFacility.OnEnable(this, m_tabLayers[1]);
			}
			zbWsMsgHandler.Instance.Facility_Recommend_Req();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(360f, (LengthUnit)0));
			break;
		}
		RefreshNotify();
	}

	private void OnUpdateEvent(object data)
	{
		RefreshModule();
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		switch (m_currTab)
		{
		case 0:
			zbWsMsgHandler.Instance.Facility_Data_Req();
			break;
		case 1:
			zbWsMsgHandler.Instance.Facility_Recommend_Req();
			break;
		}
	}

	private void RefreshModule()
	{
		switch (m_currTab)
		{
		case 0:
			m_moduleMineFacility.Refresh();
			break;
		case 1:
			m_moduleSearchFacility.Refresh();
			break;
		}
		RefreshNotify();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		if (m_currTab == 0)
		{
			m_moduleMineFacility.UpdateAtShow();
		}
	}

	private void CheckHasJoined()
	{
		FacilitySelfInfo facilitySelfInfo = zbServerData.sGameData.FacilitySelfInfo;
		if (facilitySelfInfo != null && facilitySelfInfo.Join != null && facilitySelfInfo.Join.ExpireTm >= zbSelf.Instance.timeHandler.GetServerTime())
		{
			zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.11236"), zbUtil.Language("lang.11282"), ContinueExplore, zbUtil.Language("lang.11283"), zbUtil.Language("lang.11284"));
		}
	}

	private void ContinueExplore(bool isOk)
	{
		FacilitySelfInfo facilitySelfInfo = zbServerData.sGameData.FacilitySelfInfo;
		if (isOk)
		{
			zbWsMsgHandler.Instance.Facility_View_Req(facilitySelfInfo.Join.FacilityId, openFacilityWnd: true);
		}
		else
		{
			zbWsMsgHandler.Instance.Facility_Quit_Req(facilitySelfInfo.Join.FacilityId);
		}
	}

	private void OnClose()
	{
		Hide();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		if (m_currTab == 1)
		{
			m_moduleSearchFacility.UpdatePerSecond();
		}
	}

	private void RefreshZhanLing()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnZhanLing).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyThreeHandler.IsActive()));
		m_notifyZhanLing.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyThreeHandler.HasZhanLingNotify()));
	}

	private void RefreshNotify()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		m_notifyMine.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.facilityHandler.HasCreateNotify()));
		m_notifySearch.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.facilityHandler.HasSearchNotify()));
		m_notifyWeekCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.vipHandler.HasVipCardNotifyByTab(EVipCardTab.Week)));
	}

	private void OnRule()
	{
		string[] langKeys = new string[28]
		{
			"lang.11313", "lang.11314", "lang.11315", "lang.11316", "", "lang.11317", "lang.11318", "lang.11319", "lang.11320", "",
			"lang.11321", "lang.11322", "lang.11323", "lang.11324", "", "lang.11325", "lang.11326", "lang.11327", "lang.11328", "lang.11329",
			"lang.11330", "", "lang.11331", "lang.11332", "lang.11333", "lang.11334", "lang.11335", ""
		};
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	private void OnZhanLing()
	{
		if (!zbSelf.Instance.journeyThreeHandler.IsActive())
		{
			RefreshZhanLing();
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10156"));
			zbUIMgr.Instance.HideWnd(EWndId.JourneyThreeMain);
		}
		else
		{
			zbUIMgr.Instance.ShowJourneyThreeZhanLing();
		}
	}

	private void OnWeekCard()
	{
		zbUIMgr.Instance.ShowWeekCardWnd();
	}
}
