using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbDungeonWnd : zbBaseWnd
{
	private VisualElement m_bgTouch;

	private VisualElement m_root;

	public zbDungeonJoystick joyStick;

	private VisualElement m_actionUi;

	private zbDungeonPanel m_panel;

	private Button m_btnClose;

	public Button m_btnLog;

	private Button m_btnMap;

	private zbMiniMapCanvas m_miniMap;

	private Button m_btnMapMask;

	private Label m_iconHealth;

	private Label m_iconWarning;

	private VisualElement m_ui;

	private VisualElement m_gun;

	private VisualElement m_actionProgressRoot;

	private zbPieProgress m_actionProgress;

	private Button m_InfoMask;

	private Button m_btnRole;

	private zbDungeonHandler m_dungeonHandler;

	protected override void Awake()
	{
		base.Awake();
		joyStick = new zbDungeonJoystick();
		joyStick.Init(this);
		m_panel = new zbDungeonPanel();
		zbDungeonMgr.Instance.Init(this);
	}

	protected override void OnDestroy()
	{
		zbAudioMgr.Instance.StopAllAudio();
		zbDungeonMgr.Instance.Free();
		base.OnDestroy();
	}

	protected override void OnEnable()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		joyStick.OnEnable();
		m_panel.OnEnable(this);
		m_actionProgressRoot = UQueryExtensions.Q<VisualElement>(m_RootElement, "actionprogressroot", (string)null);
		m_actionProgress = UQueryExtensions.Q<zbPieProgress>(m_actionProgressRoot, "actionprogress", (string)null);
		m_actionProgressRoot.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_actionUi = UQueryExtensions.Q<VisualElement>(m_RootElement, "actionui", (string)null);
		m_ui = UQueryExtensions.Q<VisualElement>(m_RootElement, "ui", (string)null);
		m_iconHealth = UQueryExtensions.Q<Label>(m_RootElement, "icon_health", (string)null);
		m_iconWarning = UQueryExtensions.Q<Label>(m_RootElement, "icon_warning", (string)null);
		m_gun = UQueryExtensions.Q<VisualElement>(m_RootElement, "gun", (string)null);
		m_miniMap = UQueryExtensions.Q<zbMiniMapCanvas>(m_RootElement, (string)null, (string)null);
		m_miniMap.OnEnable();
		m_btnMap = UQueryExtensions.Q<Button>(m_RootElement, "btn_map", (string)null);
		((VisualElement)m_btnMap).pickingMode = (PickingMode)0;
		((Focusable)m_btnMap).focusable = false;
		m_bgTouch = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnLog = UQueryExtensions.Q<Button>(m_RootElement, "btn_log", (string)null);
		m_btnMapMask = UQueryExtensions.Q<Button>(m_RootElement, "btn_mapMask", (string)null);
		m_btnRole = UQueryExtensions.Q<Button>(m_RootElement, "btn_role", (string)null);
		m_InfoMask = UQueryExtensions.Q<Button>(m_RootElement, "info_mask", (string)null);
		((VisualElement)m_InfoMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_InfoMask, "lb_warning", (string)null)).text = zbUtil.Language("lang.10660");
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_InfoMask, "lb_health", (string)null)).text = zbUtil.Language("lang.10661");
		m_uiEvent.RegisterCallback<PointerDownEvent>(m_bgTouch, (Action<PointerDownEvent>)OnPointerDown, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<PointerMoveEvent>(m_bgTouch, (Action<PointerMoveEvent>)OnPointerMove, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<PointerUpEvent>(m_bgTouch, (Action<PointerUpEvent>)OnPointerUp, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLog, (Action)OnLog, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMap, (Action<ClickEvent>)OnMap, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMapMask, (Action)OnCloseMap, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_InfoMask, (Action)OnInfoMaskClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_iconHealth, (Action)OnInfoMask, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_iconWarning, (Action)OnInfoMask, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRole, (Action)OnRole, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Dungeon_UpdateMap, OnUpdateMap);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Dungeon_Close_Ack, OnDungeonClose);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Dungeon_Trigger, OnDungeonTrigger);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Dungeon_Move, OnRefreshMap);
	}

	private void OnInfoMask()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_InfoMask).SetEnabled(true);
		((VisualElement)m_InfoMask).pickingMode = (PickingMode)0;
		((VisualElement)m_InfoMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	private void OnInfoMaskClose()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_InfoMask).SetEnabled(false);
		((VisualElement)m_InfoMask).pickingMode = (PickingMode)1;
		((VisualElement)m_InfoMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private void OnRole()
	{
		zbUIMgr.Instance.ShowGuideStatic(new List<int> { 3 });
	}

	private void OnCloseMap()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnMapMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnMap).style.overflow = StyleEnum<Overflow>.op_Implicit((Overflow)1);
		SetBorder((VisualElement)(object)m_btnMap, visible: true, Color.gray);
		((VisualElement)m_miniMap).style.width = StyleLength.op_Implicit(240f);
		((VisualElement)m_miniMap).style.height = StyleLength.op_Implicit(160f);
		((VisualElement)m_miniMap).style.left = StyleLength.op_Implicit((StyleKeyword)2);
		((VisualElement)m_miniMap).style.top = StyleLength.op_Implicit((StyleKeyword)2);
		((VisualElement)m_miniMap).style.translate = StyleTranslate.op_Implicit(new Translate(new Length(0f, (LengthUnit)1), new Length(0f, (LengthUnit)1)));
		((VisualElement)m_miniMap).style.scale = StyleScale.op_Implicit(new Vector2(0.8f, 0.8f));
		m_actionUi.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_ui.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		joyStick.SetVisible(vis: true);
		m_miniMap.SetRenderMod(byPlayerPos: true);
	}

	private void OnMap(ClickEvent @event)
	{
	}

	private void SetBorder(VisualElement vs, bool visible, Color color)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		vs.style.borderTopWidth = StyleFloat.op_Implicit((float)(visible ? 1 : 0));
		vs.style.borderRightWidth = StyleFloat.op_Implicit((float)(visible ? 1 : 0));
		vs.style.borderBottomWidth = StyleFloat.op_Implicit((float)(visible ? 1 : 0));
		vs.style.borderLeftWidth = StyleFloat.op_Implicit((float)(visible ? 1 : 0));
		vs.style.borderTopColor = StyleColor.op_Implicit(color);
		vs.style.borderRightColor = StyleColor.op_Implicit(color);
		vs.style.borderBottomColor = StyleColor.op_Implicit(color);
		vs.style.borderLeftColor = StyleColor.op_Implicit(color);
	}

	private void OnRefreshMap(object data)
	{
		m_miniMap.ConvertToMiniMap(m_dungeonHandler.dungeonMapInfo);
	}

	public void RefreshCustom(zbDungeonHandler dungeonBaseHandler)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		m_dungeonHandler = dungeonBaseHandler;
		zbDungeonMgr.Instance.Refresh(m_dungeonHandler);
		m_miniMap.ConvertToMiniMap(dungeonBaseHandler.dungeonMapInfo);
		m_panel.Refresh(m_dungeonHandler.dungeonBaseInfo);
		zbPlayerGunCsvRecord recordByKey = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(zbSelf.Instance.GetGunId());
		m_gun.style.backgroundImage = StyleBackground.op_Implicit(zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.battleBanner));
		if (zbSelf.Instance.newTagHandler.IsFirstEnterDungeon())
		{
			zbSelf.Instance.newTagHandler.SetDungeonEntered();
			zbUIMgr.Instance.ShowGuideStatic(new List<int> { 3 });
		}
	}

	protected override void UpdateAtShow()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateAtShow();
		Vector2Int cellPosition = GetCellPosition(m_dungeonHandler.dungeonBaseInfo.RoleSiteInfo.CellId);
		float num = MathF.Floor(((Component)zbDungeonMgr.Instance.hero).transform.position.x / 3.2f);
		m_miniMap.Update((int)num, ((Vector2Int)(ref cellPosition)).y - 1);
		zbDungeonMgr.Instance.Update();
	}

	private Vector2Int GetCellPosition(string cellId)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		string[] array = cellId.Split(':');
		if (array.Length == 2 && int.TryParse(array[0], out var result) && int.TryParse(array[1], out var result2))
		{
			return new Vector2Int(result, result2);
		}
		throw new FormatException("CellId 格式不正确: " + cellId);
	}

	private void OnPointerDown(PointerDownEvent evt)
	{
		joyStick.OnPointerDown(evt);
		PointerCaptureHelper.CapturePointer((IEventHandler)(object)m_bgTouch, ((PointerEventBase<PointerDownEvent>)(object)evt).pointerId);
	}

	private void OnPointerMove(PointerMoveEvent evt)
	{
		joyStick.OnPointerMove(evt);
	}

	private void OnPointerUp(PointerUpEvent evt)
	{
		joyStick.OnPointerUp(evt);
		PointerCaptureHelper.ReleasePointer((IEventHandler)(object)m_bgTouch, ((PointerEventBase<PointerUpEvent>)(object)evt).pointerId);
	}

	public void SetActionUiDisplay(bool vis)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnMap).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
		m_actionUi.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
		m_ui.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
		joyStick.SetVisible(vis);
	}

	private void OnClose(ClickEvent evt)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (((VisualElement)m_btnMapMask).style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			OnCloseMap();
			return;
		}
		string message = zbUtil.Language("lang.10318");
		if (m_dungeonHandler != null && m_dungeonHandler.dungeonBaseInfo != null && m_dungeonHandler.dungeonBaseInfo.PassState == EPassState.Passed)
		{
			message = zbUtil.Language("lang.10319");
		}
		zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), message, OnConfirmCloseDungeon);
	}

	private void OnConfirmCloseDungeon(bool isOk)
	{
		if (isOk)
		{
			m_dungeonHandler.CloseDungeon_Req();
		}
	}

	public void SetActionProgress(bool visible, float value, Vector3 position)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		if (m_actionProgress != null)
		{
			m_actionProgressRoot.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!visible));
			if (visible)
			{
				m_actionProgress.progress = value;
				Vector2 val = RuntimePanelUtils.CameraTransformWorldToPanel(m_actionProgressRoot.panel, position, zbCameraHelper.Instance.mainCamera);
				Rect worldBound = m_actionProgressRoot.parent.worldBound;
				Vector2 position2 = ((Rect)(ref worldBound)).position;
				Vector2 val2 = val - position2;
				m_actionProgressRoot.style.left = StyleLength.op_Implicit(val2.x - 32f);
				m_actionProgressRoot.style.top = StyleLength.op_Implicit(val2.y - 32f);
			}
		}
	}

	private void OnUpdateMap(object data)
	{
		zbDungeonMgr.Instance.Build();
		m_panel.Refresh(m_dungeonHandler.dungeonBaseInfo);
	}

	private void OnDungeonClose(object data)
	{
		Hide();
		zbDungeonMgr.Instance.ResetDungeonInfo();
		zbUIMgr.Instance.ShowDungeonBalance(m_dungeonHandler);
	}

	private void OnDungeonTrigger(object data)
	{
		zbDungeonMgr.Instance.OnTriggerEvent();
	}

	private void OnLog()
	{
		if (m_dungeonHandler.IsWorldDungeon())
		{
			zbWsMsgHandler.Instance.WorldDungeon_LogInfo_Req();
		}
	}
}
