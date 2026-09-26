using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHomeWnd : zbBaseWnd
{
	[NonSerialized]
	public zbHomeMap m_homeMap;

	[NonSerialized]
	public zbHomeMapCinema m_homeMapCinema;

	[NonSerialized]
	public VisualElement hudRoot;

	private VisualElement m_nodeMenu;

	private Button m_btnAttr;

	public Button m_btnUpgradeFast;

	private Button m_btnEdit;

	private Button m_btnCreate;

	private VisualElement m_nodeFunc;

	private Button m_btnHandbook;

	public Button m_btnWall;

	public Button m_btnTreasure;

	private Button m_btnServant;

	private Button m_btnServantIncubate;

	private zbTopBar m_topBar;

	private zbTopLeftPlayerAvatar m_playerAvatar;

	private VisualElement m_notifyUpgradeFast;

	private VisualElement m_notifyHandbook;

	private VisualElement m_notifyWall;

	private VisualElement m_notifyTreasure;

	private VisualElement m_notifyServantIncubate;

	private VisualElement m_notifyServant;

	private VisualElement m_newWall;

	private VisualElement m_notifyCreate;

	private Coroutine m_coroutine;

	private Coroutine m_buildCoroutune;

	private bool m_bCheckWarRoomUnlockFeiji;

	protected override void Awake()
	{
		base.Awake();
		m_homeMapCinema = ((Component)this).GetComponent<zbHomeMapCinema>();
		m_homeMapCinema.Init(this);
		m_homeMap = new zbHomeMap(this, m_homeMapCinema);
		zbHomeMgr.Instance.Init(m_GameObjectSceneRoot);
	}

	protected override void OnDestroy()
	{
		if (m_coroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_coroutine);
			m_coroutine = null;
		}
		if (m_buildCoroutune != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_buildCoroutune);
			m_buildCoroutune = null;
		}
		zbHomeMgr.Instance.Free();
		base.OnDestroy();
	}

	protected override void OnEnable()
	{
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0503: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_homeMap.OnEnable();
		hudRoot = UQueryExtensions.Q<VisualElement>(m_RootElement, "hudRoot", (string)null);
		m_nodeMenu = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_menu", (string)null);
		m_btnAttr = UQueryExtensions.Q<Button>(m_RootElement, "btn_attr", (string)null);
		m_btnUpgradeFast = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgradeFast", (string)null);
		m_btnEdit = UQueryExtensions.Q<Button>(m_RootElement, "btn_edit", (string)null);
		m_btnCreate = UQueryExtensions.Q<Button>(m_RootElement, "btn_create", (string)null);
		m_nodeFunc = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_func", (string)null);
		m_btnHandbook = UQueryExtensions.Q<Button>(m_RootElement, "btn_handbook", (string)null);
		m_btnWall = UQueryExtensions.Q<Button>(m_RootElement, "btn_wall", (string)null);
		m_btnTreasure = UQueryExtensions.Q<Button>(m_RootElement, "btn_treasure", (string)null);
		((TextElement)m_btnTreasure).text = zbUtil.Language("lang.11025");
		m_btnServant = UQueryExtensions.Q<Button>(m_RootElement, "btn_servant", (string)null);
		m_btnServantIncubate = UQueryExtensions.Q<Button>(m_RootElement, "btn_servantIncubate", (string)null);
		m_notifyUpgradeFast = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnUpgradeFast, (Vector2?)new Vector2(0f, 0f), 1f, true);
		m_notifyHandbook = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnHandbook, (Vector2?)new Vector2(22f, 14f), 1f, true);
		m_notifyWall = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnWall, (Vector2?)new Vector2(22f, 14f), 1f, true);
		m_notifyTreasure = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTreasure, (Vector2?)new Vector2(22f, 14f), 1f, true);
		m_notifyServantIncubate = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnServantIncubate, (Vector2?)new Vector2(22f, 14f), 1f, true);
		m_notifyServant = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnServant, (Vector2?)new Vector2(22f, 14f), 1f, true);
		m_notifyCreate = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnCreate, (Vector2?)new Vector2(0f, 0f), 1f, true);
		m_newWall = zbUtil.CreateNewTag((VisualElement)(object)m_btnWall, (Vector2?)new Vector2(8f, 3f), 0.9f);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Gold
		});
		CreateMainBottom(2);
		m_topBar = new zbTopBar();
		m_topBar.OnEnable(this);
		m_playerAvatar = new zbTopLeftPlayerAvatar();
		m_playerAvatar.OnEnable(this);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAttr, (Action)OnAttr, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgradeFast, (Action)OnUpgradeFast, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnEdit, (Action)OnEdit, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCreate, (Action)OnCreate, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHandbook, (Action)OnHandbook, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnWall, (Action<ClickEvent>)OnWall, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTreasure, (Action<ClickEvent>)OnTreasure, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnServant, (Action)OnServant, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnServantIncubate, (Action)OnServantIncubate, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_HomelandInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Item, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Handbook, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_MonsterActive_Ack, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_RoleBaseInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_RoleCommonInfo, OnEventUpdate);
		m_LastCameraPosition = new Vector3(1.2f, -11f, ((Component)zbCameraHelper.Instance.mainCamera).transform.position.z);
		m_LastCameraOrthographicSize = 11.6f;
		((Component)zbCameraHelper.Instance.mainCamera).transform.position = m_LastCameraPosition;
		zbCameraHelper.Instance.mainCamera.orthographicSize = m_LastCameraOrthographicSize;
	}

	public override void Refresh()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		if (zbHomeMgr.Instance.IsBuildOk())
		{
			zbHomeMgr.Instance.roomMgr.RefreshAllRoomState();
		}
		m_topBar.Refresh();
		m_playerAvatar.Refresh();
		((VisualElement)m_btnEdit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.homelandHandler.GetFloorState(-2) != EActiveState.Done));
		((VisualElement)m_btnCreate).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.homelandHandler.GetFloorState(-2) != EActiveState.Done));
		((VisualElement)m_btnWall).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.WallDefense) ? 1 : 0));
		((VisualElement)m_btnTreasure).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Treasure) ? 1 : 0));
		m_notifyUpgradeFast.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.homelandHandler.CanRoomUpgradeFast()));
		m_notifyHandbook.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!zbSelf.Instance.handbookHandler.HasRedPoint() && !zbSelf.Instance.handbookHandler.HasModRedPoint()) ? 1 : 0));
		m_notifyWall.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.wallHandler.HasRedPoint()));
		m_notifyTreasure.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.treasureHandler.HasRedPoint()));
		m_newWall.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.wallHandler.HasRedPoint() || !zbSelf.Instance.wallHandler.HasNewPoint()) ? 1 : 0));
		m_notifyCreate.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.homeEditHandler.CanRoomCreate()));
		((VisualElement)m_btnServant).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Servant) || zbServerData.sGameData.ServantInfo == null || zbServerData.sGameData.ServantInfo.ServantList == null || zbServerData.sGameData.ServantInfo.ServantList.Count <= 0) ? 1 : 0));
		m_notifyServant.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.servantHandler.HasServantRedPoint()));
		((VisualElement)m_btnServantIncubate).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Servant) ? 1 : 0));
		m_notifyServantIncubate.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.servantHandler.HasIncubateRedPoint()));
		m_bCheckWarRoomUnlockFeiji = false;
	}

	public void ExecGuide()
	{
		if (!zbGuide.Instance.GetGuideBool(EGuideStep.guide_step_Home2))
		{
			zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_Home2);
			zbGuide.Instance.ExecGuide(EGuideStep.guide_step_Home2);
		}
	}

	public void SetUIVisible(bool vis)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		m_nodeMenu.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
		m_nodeFunc.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
	}

	private void OnHandbook()
	{
		zbLinkHelper.OpenHandbook();
	}

	public void OnWall(ClickEvent evt)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		zbLinkHelper.OpenWall();
		m_newWall.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	public void OnTreasure(ClickEvent evt)
	{
		zbLinkHelper.OpenTreasure();
	}

	private void OnServant()
	{
		zbUIMgr.Instance.ShowServant();
	}

	private void OnServantIncubate()
	{
		zbUIMgr.Instance.ShowServantIncubate();
	}

	private void OnAttr()
	{
		zbUIMgr.Instance.ShowRoomAttrList();
	}

	private void OnUpgradeFast()
	{
		if (!zbSelf.Instance.homelandHandler.CanRoomUpgradeFast())
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10733"));
		}
		else
		{
			zbWsMsgHandler.Instance.HomelandV2_RoomUpgradeFast_Req();
		}
	}

	private void OnEdit()
	{
		zbUIMgr.Instance.ShowHomeEdit();
	}

	private void OnCreate()
	{
		zbUIMgr.Instance.ShowHomeCreate();
	}

	protected override void Start()
	{
		base.Start();
		if (m_buildCoroutune != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_buildCoroutune);
			m_buildCoroutune = null;
		}
		m_buildCoroutune = ((MonoBehaviour)this).StartCoroutine(zbHomeMgr.Instance.BuildAsync());
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		m_homeMap.Update();
		zbHomeMgr.Instance.Update();
		if (zbHomeMgr.Instance.IsBuildOk() && !m_bCheckWarRoomUnlockFeiji)
		{
			m_bCheckWarRoomUnlockFeiji = true;
			CheckWarRoomUnlock();
		}
		m_topBar.UpdateRedPoint();
		m_playerAvatar.UpdateRedPoint();
	}

	private void OnEventUpdate(object data)
	{
		if (IsShow())
		{
			Refresh();
		}
	}

	private void CheckWarRoomUnlock()
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (zbHomeMgr.Instance.roomHeroMgr.roomHeroWaitMoveList.Count > 0)
		{
			List<zbRoomHero> roomHeroWaitMoveList = zbHomeMgr.Instance.roomHeroMgr.roomHeroWaitMoveList;
			zbRoomHero zbRoomHero2 = roomHeroWaitMoveList[roomHeroWaitMoveList.Count - 1];
			PlayRoomHeroMoveToRoom();
			zbUIMgr.Instance.ShowRoomHeroFollow(zbRoomHero2.curHeroTid);
			return;
		}
		int canUnlockFloor = zbSelf.Instance.homelandHandler.GetCanUnlockFloor();
		if (canUnlockFloor < 0)
		{
			((Component)zbCameraHelper.Instance.mainCamera).transform.position = new Vector3(7.6f, -11f + (float)canUnlockFloor * 2.2f, ((Component)zbCameraHelper.Instance.mainCamera).transform.position.z);
			zbCameraHelper.Instance.mainCamera.orthographicSize = 12.3f;
		}
	}

	public void PlayRoomHeroMoveToRoom()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		((Component)zbCameraHelper.Instance.mainCamera).transform.position = new Vector3(9f, 3f, ((Component)zbCameraHelper.Instance.mainCamera).transform.position.z);
		zbAudioMgr.Instance.PlayAudio("audio/ui/feiji.ogg");
		float duration = (float)zbHomeMgr.Instance.roomHeroMgr.roomHeroWaitMoveList.Count * 0.5f + 2f;
		zbHomeMgr.Instance.roomHeroMgr.plane.PlayerArriveAnimation(duration);
		if (m_coroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_coroutine);
			m_coroutine = null;
		}
		m_coroutine = ((MonoBehaviour)this).StartCoroutine(PlayRoomHeroAnimListCoroutine());
	}

	private IEnumerator PlayRoomHeroAnimListCoroutine()
	{
		List<zbRoomHero> list = new List<zbRoomHero>(zbHomeMgr.Instance.roomHeroMgr.roomHeroWaitMoveList);
		foreach (zbRoomHero item in list)
		{
			item.fsm.SetState(ERoomHeroState.Plane);
			zbHomeMgr.Instance.roomHeroMgr.roomHeroList.Add(item);
			yield return (object)new WaitForSeconds(0.5f);
		}
		zbHomeMgr.Instance.roomHeroMgr.roomHeroWaitMoveList.Clear();
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		Refresh();
	}
}
