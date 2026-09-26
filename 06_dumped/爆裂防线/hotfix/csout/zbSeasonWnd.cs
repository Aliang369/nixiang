using System;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonWnd : zbBaseWnd
{
	public zbSeasonMap m_seasonMap;

	public zbSeasonMgr m_seasonMgr;

	private VisualElement m_bgBottom;

	private VisualElement m_miniMap;

	private VisualElement m_ui;

	private Button m_btnForce;

	private Button m_btnClose;

	private Button m_btnSearch;

	private zbSeasonTeam m_team;

	private zbTopLeftPlayerAvatar m_playerAvatar;

	private zbSeasonLeftPanel m_leftPanel;

	private zbSeasonRightPanel m_rightPanel;

	private zbSeasonTopPanel m_topPanel;

	private bool m_initHud;

	private float m_seasonDataTimer;

	private const float seasonDataInterval = 30f;

	protected override void Awake()
	{
		base.Awake();
		m_seasonMap = new zbSeasonMap(this);
	}

	protected override void OnEnable()
	{
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_ui = UQueryExtensions.Q<VisualElement>(m_RootElement, "ui", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "close", (string)null);
		m_bgBottom = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_bottom", (string)null);
		m_miniMap = UQueryExtensions.Q<VisualElement>(m_RootElement, "minimap", (string)null);
		m_btnForce = UQueryExtensions.Q<Button>(m_RootElement, "btn_force", (string)null);
		m_btnSearch = UQueryExtensions.Q<Button>(m_RootElement, "btn_search", (string)null);
		m_team = new zbSeasonTeam();
		m_team.OnEnable(this, m_RootElement);
		m_leftPanel = new zbSeasonLeftPanel();
		m_leftPanel.OnEnable(this, m_RootElement);
		m_playerAvatar = new zbTopLeftPlayerAvatar();
		m_playerAvatar.OnEnable(this);
		m_rightPanel = new zbSeasonRightPanel();
		m_rightPanel.OnEnable(this, m_RootElement);
		m_topPanel = new zbSeasonTopPanel();
		m_topPanel.OnEnable(this, m_RootElement);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnForce, (Action)OnForce, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSearch, (Action)OnSearch, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		zbSeasonMgr.Instance.OnEnable(this);
		zbSeasonHudMgr.Instance.OnEnable(this);
		if (m_seasonMap != null)
		{
			m_seasonMap.OnEnable();
		}
		m_LastCameraPosition = new Vector3(0f, 0f, ((Component)zbCameraHelper.Instance.mainCamera).transform.position.z);
		m_LastCameraOrthographicSize = zbSeasonMapDefine.maxOrthographicSize;
		((Component)zbCameraHelper.Instance.mainCamera).transform.position = m_LastCameraPosition;
		zbCameraHelper.Instance.mainCamera.orthographicSize = m_LastCameraOrthographicSize;
		m_gameEevent.RegisterEvent(EGameEvent.Event_MyTerritory_Ack, OnEventUpdateMyTerritory);
		m_gameEevent.RegisterEvent(EGameEvent.Event_SeasonMap_Ack, OnEventUpdataSeasonMap);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Season_Online, OnEventSeasonOnline);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DeclareList_Ack, OnEventDeclareList);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Team_Pos, OnEventTeamPos);
	}

	protected override void Start()
	{
		base.Start();
	}

	public void RefreshCustom()
	{
		zbWsMsgHandler.Instance.Season_Map_Info_Req();
		zbWsMsgHandler.Instance.Season_Data_Req();
		zbWsMsgHandler.Instance.Season_My_Territory_Req();
		zbWsMsgHandler.Instance.Season_Declare_List_Req();
		zbWsMsgHandler.Instance.Season_Team_Pos_Req();
		m_topPanel.Refresh();
		m_seasonDataTimer = 0f;
		m_playerAvatar.Refresh();
	}

	private void RefreshSeasonData()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		ESeasonForce force = zbSelf.Instance.seasonHandler.GetMySeasonForce();
		((VisualElement)m_btnForce).style.backgroundImage = zbSelf.Instance.seasonHandler.GetForceBtnFlagByForceId(force);
		if (m_seasonMap != null)
		{
			zbSeasonArea zbSeasonArea2 = m_seasonMap.m_seasonAreas.FirstOrDefault((zbSeasonArea area) => area.areaData.force == force && area.areaData.areaType == 0);
			if ((Object)(object)zbSeasonArea2 != (Object)null)
			{
				m_seasonMap.TrackGameObject(((Component)zbSeasonArea2).gameObject);
			}
		}
	}

	private void RefreshMyTerritory()
	{
		m_topPanel.RefreshTerritory();
	}

	public void SetUiElementVisible(bool vis)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		m_ui.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
		m_bgBottom.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
	}

	public zbSeasonMap GetSeasonMap()
	{
		return m_seasonMap;
	}

	private void RefreshRedPoint()
	{
		m_playerAvatar.UpdateRedPoint();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		RefreshRedPoint();
		if (m_team != null)
		{
			m_team.Update();
		}
		if (!m_initHud)
		{
			m_initHud = true;
			zbSeasonHudMgr.Instance.UpdateWorldToPanelCache();
			zbSeasonHudMgr.Instance.UpdateHudPos();
		}
		zbSeasonHudMgr.Instance.Update();
		zbSeasonMgr.Instance.Update();
		if (m_seasonMap != null)
		{
			m_seasonMap.Update();
		}
		m_seasonDataTimer += Time.deltaTime;
		if (m_seasonDataTimer >= 30f)
		{
			m_seasonDataTimer = 0f;
		}
	}

	[ContextMenu("Test Line")]
	private void TestLine()
	{
		zbSeasonLineMgr.Instance.ClearAll();
		int[] array = new int[1000];
		for (int i = 0; i < array.Length; i++)
		{
			_ = array[i];
			TestAddLine();
		}
	}

	private void TestAddLine()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)m_seasonMap.m_seasonAreas[Random.Range(1, m_seasonMap.m_seasonAreas.Count)].cityBuild).transform.position;
		Vector3 position2 = ((Component)m_seasonMap.m_seasonAreas[Random.Range(1, m_seasonMap.m_seasonAreas.Count)].cityBuild).transform.position;
		SeasonMoveDetail seasonMoveDetial = new SeasonMoveDetail
		{
			ForceId = (ESeasonForce)Random.Range(1, 5),
			TeamId = 1,
			FromX = position.x,
			FromY = position.y,
			ToX = position2.x,
			ToY = position2.y,
			StartTm = zbSelf.Instance.timeHandler.GetServerTime(),
			ArrivalTm = zbSelf.Instance.timeHandler.GetServerTime() + 30,
			MoveType = 0
		};
		zbSelf.Instance.seasonHandler.AddLine(seasonMoveDetial);
	}

	public void OnClose()
	{
		m_initHud = false;
		m_seasonDataTimer = 0f;
		Hide();
	}

	private void OnForce()
	{
		zbWsMsgHandler.Instance.Season_Force_Info_Req();
	}

	private void OnSearch()
	{
		zbUIMgr.Instance.ShowSeasonSearch();
	}

	private void OnEventDeclareList(object data)
	{
		m_leftPanel.RefreshDeclareList();
	}

	private void OnEventUpdateMyTerritory(object data)
	{
		RefreshMyTerritory();
	}

	private void OnEventTeamPos(object data)
	{
		m_team.Refresh();
	}

	private void OnEventUpdataSeasonMap(object data)
	{
		m_seasonMap.InitAreas();
		zbSeasonLineMgr.Instance.ClearAll();
		int[] array = new int[100];
		for (int i = 0; i < array.Length; i++)
		{
			_ = array[i];
			TestAddLine();
		}
	}

	private void OnEventSeasonOnline(object data)
	{
		RefreshSeasonData();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		zbSeasonHudMgr.Instance.Clear();
	}
}
