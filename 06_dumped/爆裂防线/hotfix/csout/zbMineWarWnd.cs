using System;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMineWarWnd : zbBaseWnd
{
	private SpineVisualElement m_spineHong;

	private VisualElement m_crossTitle;

	private Label m_lbCrossTemp;

	private Label m_lbCrossTime;

	private Button m_btnServer;

	private Button m_btnServer2;

	private SpineVisualElement m_spineServer;

	private Button m_btnPrivilege;

	private SpineVisualElement m_spinePrivilege;

	private VisualElement m_notifyPrivilege;

	private EBoolState m_lastPrivilegeState = (EBoolState)(-1);

	private Coroutine m_coroutine;

	private Coroutine m_buildCoroutune;

	private VisualElement m_bgBottom;

	private Button m_btnClose;

	private Button m_btnClose_2;

	private Button m_btnBiaoji;

	[NonSerialized]
	public VisualElement hudRoot;

	private Label m_lbServer;

	private Label m_lbNewProtect;

	private Button m_btnRule;

	private Button m_btnLog;

	private Button m_btnTreasure;

	private VisualElement m_notifyTreasure;

	private Button m_btnLock;

	[NonSerialized]
	private zbPlayerAvatarTarget renderTarget;

	private Image m_descImg;

	private VisualElement foldIcon;

	private VisualElement nodePlayer;

	private zbArenaBottom m_bottomPanel;

	private int heartBeat;

	private bool mineWarCrossRed;

	private bool lastMineWarCrossRed;

	private VisualElement m_notifyServer;

	private VisualElement m_notifyRecord;

	public zbMineWarMap m_homeMap { get; set; }

	public zbMineWarMapCinema m_homeMapCinema { get; set; }

	protected override void Awake()
	{
		base.Awake();
		m_homeMapCinema = ((Component)this).GetComponent<zbMineWarMapCinema>();
		m_homeMapCinema.Init(this);
		m_homeMap = new zbMineWarMap(this, m_homeMapCinema);
		zbMineWarMgr.Instance.Init(m_GameObjectSceneRoot);
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
		base.OnDestroy();
	}

	protected override void OnEnable()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0599: Unknown result type (might be due to invalid IL or missing references)
		//IL_059f: Invalid comparison between Unknown and I4
		//IL_044c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Expected O, but got Unknown
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_0480: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cb: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_homeMap.OnEnable();
		m_LastCameraPosition = new Vector3(0f, 0f, ((Component)zbCameraHelper.Instance.mainCamera).transform.position.z);
		m_LastCameraOrthographicSize = 10f;
		((Component)zbCameraHelper.Instance.mainCamera).transform.position = m_LastCameraPosition;
		zbCameraHelper.Instance.mainCamera.orthographicSize = m_LastCameraOrthographicSize;
		m_lbServer = UQueryExtensions.Q<Label>(m_RootElement, "lb_server", (string)null);
		hudRoot = UQueryExtensions.Q<VisualElement>(m_RootElement, "hudRoot", (string)null);
		m_btnLock = UQueryExtensions.Q<Button>(m_RootElement, "btn_lock", (string)null);
		m_crossTitle = UQueryExtensions.Q<VisualElement>(m_RootElement, "crossTitle", (string)null);
		m_lbCrossTemp = UQueryExtensions.Q<Label>(m_RootElement, "lb_CrossTemp", (string)null);
		m_lbCrossTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_CrossTime", (string)null);
		m_spineHong = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spineHong", (string)null);
		m_bgBottom = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_bottom", (string)null);
		m_btnBiaoji = UQueryExtensions.Q<Button>(m_RootElement, "btnBiaoji", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBiaoji, (Action<ClickEvent>)OnClickBiaoji, "", (TrickleDown)0);
		m_btnServer = UQueryExtensions.Q<Button>(m_RootElement, "btnServer", (string)null);
		m_btnServer2 = UQueryExtensions.Q<Button>((VisualElement)(object)m_btnServer, "btnServer2", (string)null);
		m_spineServer = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)m_btnServer, "spineServer", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnServer2, (Action<ClickEvent>)OnClickServer, "", (TrickleDown)0);
		m_btnPrivilege = UQueryExtensions.Q<Button>(m_RootElement, "btn_privilege", (string)null);
		m_spinePrivilege = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)m_btnPrivilege, "spine", (string)null);
		m_notifyPrivilege = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnPrivilege, (Vector2?)new Vector2(0f, 0f), 1f, true);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPrivilege, (Action<ClickEvent>)OnPrivilege, "", (TrickleDown)0);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_btnClose_2 = UQueryExtensions.Q<Button>(m_RootElement, "btn_back_2", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose_2, (Action<ClickEvent>)OnClose_2, "", (TrickleDown)0);
		m_btnLog = UQueryExtensions.Q<Button>(m_RootElement, "btn_log", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLog, (Action)OnLog, "", (TrickleDown)0);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_btnTreasure = UQueryExtensions.Q<Button>(m_RootElement, "btn_treasure", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTreasure, (Action<ClickEvent>)OnTreasure, "", (TrickleDown)0);
		m_notifyTreasure = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTreasure, (Vector2?)new Vector2(1f, 7f), 1f, true);
		m_notifyServer = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnServer2, (Vector2?)new Vector2(-3f, 0f), 1f, true);
		m_notifyRecord = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnLog, (Vector2?)new Vector2(10f, 10f), 1f, true);
		foldIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "foldIcon", (string)null);
		nodePlayer = UQueryExtensions.Q<VisualElement>(m_RootElement, "nodePlayer", (string)null);
		renderTarget = ((Component)m_GameObjectSceneRoot.transform.Find("playerAvatarTarget")).GetComponent<zbPlayerAvatarTarget>();
		if (m_descImg == null)
		{
			m_descImg = new Image();
			((VisualElement)m_descImg).style.left = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImg).style.top = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImg).style.width = StyleLength.op_Implicit(240f);
			((VisualElement)m_descImg).style.height = StyleLength.op_Implicit(240f);
			((VisualElement)m_descImg).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		}
		foldIcon.Add((VisualElement)(object)m_descImg);
		m_bottomPanel = new zbArenaBottom();
		m_bottomPanel = UQueryExtensions.Q<zbArenaBottom>(m_RootElement, "zbArenaBottom", (string)null);
		m_bottomPanel.OnEnable(this);
		m_lbNewProtect = UQueryExtensions.Q<Label>(m_RootElement, "lb_newProtect", (string)null);
		zbLanguage.Instance.SetButtonTextVisible((Button[])(object)new Button[1] { m_btnTreasure });
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_MineWarBaseInfo, OnEventUpdateMineWarMapInfo);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Activity, delegate
		{
			RefreshPrivilege();
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_Aux_FreeGiftAward, delegate
		{
			RefreshPrivilege();
		});
		if ((int)Application.platform == 8)
		{
			((VisualElement)m_btnLog).style.top = StyleLength.op_Implicit(-1260f);
			((VisualElement)m_btnRule).style.top = StyleLength.op_Implicit(-1260f);
		}
	}

	protected override void Start()
	{
		base.Start();
	}

	public override void Refresh()
	{
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		heartBeat = 0;
		if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.CrossOpenTm <= zbSelf.Instance.timeHandler.GetServerTime())
		{
			((TextElement)m_lbNewProtect).text = "";
			((VisualElement)m_btnServer).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_spineServer).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_spineHong).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_crossTitle.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossStart < zbSelf.Instance.timeHandler.GetServerTime() && zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossEnd > zbSelf.Instance.timeHandler.GetServerTime())
			{
				((TextElement)m_btnServer2).text = zbUtil.Language("lang.10999");
				((VisualElement)m_spineServer).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_crossTitle.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			else if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossStart > zbSelf.Instance.timeHandler.GetServerTime())
			{
				((TextElement)m_btnServer2).text = "\n" + zbUtil.Language("lang.10999");
			}
		}
		else
		{
			((VisualElement)m_btnServer).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_spineServer).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_spineHong).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_crossTitle.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_bottomPanel.Refresh();
		RefreshMoneyPanel();
		RefreshPrivilege();
		UpdatePerSecond();
		RefreshCross();
		RefreshNotify();
		renderTarget.StartRender(m_descImg);
		renderTarget.RefreshAvatar(zbSelf.Instance.GetAvatarId(), zbSelf.Instance.GetGunId());
		if (!zbGuide.Instance.GetGuideBool(EGuideStep.guide_step_MineWar_2) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.MineWar))
		{
			zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_MineWar_2);
			zbGuide.Instance.ExecGuide(EGuideStep.guide_step_MineWar_2);
		}
	}

	private void RefreshNotify()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		m_notifyTreasure.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.treasureHandler.HasRedPoint()));
		m_notifyServer.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!mineWarCrossRed));
		m_notifyRecord.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!zbSelf.Instance.reminderHandler.HasReminderNotify(EReminderType.MineRecord) && !zbSelf.Instance.reminderHandler.HasReminderNotify(EReminderType.CrossMineRecord)) ? 1 : 0));
	}

	public void RefreshCross()
	{
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		if (zbSelf.Instance.mineWarHandler.currServerId != zbSelf.Instance.GetServerId())
		{
			((TextElement)m_lbCrossTemp).text = "S-" + zbSelf.Instance.mineWarHandler.currServerId % 10000;
			((VisualElement)m_btnRule).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnLog).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnPrivilege).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnBiaoji).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			nodePlayer.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnLock).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbServer).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			nodePlayer.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnTreasure).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbNewProtect).text = "";
			m_bgBottom.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnClose_2).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnServer).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/mineWar/serverlistbg_red.png"));
			return;
		}
		((TextElement)m_lbCrossTemp).text = zbUtil.Language("跨服袭击开启中");
		((VisualElement)m_btnRule).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_btnLog).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_btnPrivilege).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		nodePlayer.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_btnLock).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_lbServer).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_btnTreasure).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_bgBottom.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_btnClose_2).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (zbSelf.Instance.mineWarHandler.selfPointId > 0)
		{
			((VisualElement)m_btnBiaoji).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			((VisualElement)m_btnBiaoji).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossStart < zbSelf.Instance.timeHandler.GetServerTime() && zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossEnd > zbSelf.Instance.timeHandler.GetServerTime())
		{
			((VisualElement)m_btnLock).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbServer).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		((VisualElement)m_btnServer).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/mineWar/serverlistbg.png"));
	}

	public void RefreshCustom()
	{
		if (zbSelf.Instance.mineWarHandler.changeInfo.changePlayerId > 0)
		{
			zbUIMgr.Instance.ShowMineWarOccupyWnd();
		}
	}

	private void RefreshMoneyPanel()
	{
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.BarTreasure,
			EMoneyType.Gold
		});
		m_moneyPanel.RefreshOther(1, "ui/image/mineWar/combatNum.png", zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarSelfInfo.CombatNum, delegate
		{
			BuyCount();
		});
	}

	public void BuyCount()
	{
		if (zbSelf.Instance.mineWarHandler.GetBuyLimit() > zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarSelfInfo.BuyCombat)
		{
			zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.MineWar);
		}
		else if (zbServerData.sGameData.RechargeActiveInfo.TreasurePrivilegeInfo.Active == EBoolState.False)
		{
			zbUIMgr.Instance.ShowTreasurePrivilege();
		}
		else
		{
			zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.MineWar);
		}
	}

	private void RefreshPrivilege()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		m_notifyPrivilege.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.treasureHandler.HasFreePackRedDot()));
		EBoolState active = zbServerData.sGameData.RechargeActiveInfo.TreasurePrivilegeInfo.Active;
		if (active != m_lastPrivilegeState)
		{
			m_lastPrivilegeState = active;
			if (active == EBoolState.True)
			{
				m_spinePrivilege.AnimationState.SetAnimation(0, "loop", true);
			}
			else
			{
				m_spinePrivilege.AnimationState.SetAnimation(0, "normal", false);
			}
		}
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		m_homeMap.Update();
		zbMineWarMgr.Instance.mineWarAwardHud.Update();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		heartBeat++;
		if (heartBeat >= 10)
		{
			heartBeat = 0;
			if (zbSelf.Instance.mineWarHandler.currServerId != zbSelf.Instance.GetServerId())
			{
				zbWsMsgHandler.Instance.CrossMine_Data_Req(zbSelf.Instance.mineWarHandler.currServerId);
			}
			else
			{
				zbWsMsgHandler.Instance.MineWar_Data_Req();
			}
		}
		if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossStart < zbSelf.Instance.timeHandler.GetServerTime() && zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossEnd > zbSelf.Instance.timeHandler.GetServerTime())
		{
			if (!lastMineWarCrossRed)
			{
				mineWarCrossRed = true;
				lastMineWarCrossRed = true;
			}
			((TextElement)m_lbServer).text = zbUtil.FormatTimeSmart(zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossEnd - zbSelf.Instance.timeHandler.GetServerTime());
			((TextElement)m_lbCrossTime).text = zbUtil.FormatTimeHHMMSS(zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossEnd - zbSelf.Instance.timeHandler.GetServerTime());
		}
		else if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossStart > zbSelf.Instance.timeHandler.GetServerTime())
		{
			((TextElement)m_lbServer).text = zbUtil.FormatTimeHHMMSS(zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossStart - zbSelf.Instance.timeHandler.GetServerTime());
		}
		else if (zbSelf.Instance.mineWarHandler.currServerId != zbSelf.Instance.GetServerId())
		{
			zbWsMsgHandler.Instance.MineWar_Data_Req();
		}
		if (zbSelf.Instance.mineWarHandler.currServerId == zbSelf.Instance.GetServerId() && zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.CrossOpenTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			((TextElement)m_lbNewProtect).text = $"跨服倒计时：{zbUtil.FormatTimeSmart(zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.CrossOpenTm - zbSelf.Instance.timeHandler.GetServerTime())}";
		}
	}

	public void OnClickServer(ClickEvent evt)
	{
		if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossStart >= zbSelf.Instance.timeHandler.GetServerTime() || zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossEnd <= zbSelf.Instance.timeHandler.GetServerTime())
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10990"));
			return;
		}
		if (zbSelf.Instance.mineWarHandler.selfLayerNum > 4)
		{
			zbUIMgr.Instance.ShowMessageConfirm(zbUtil.Language("lang.11002"), null);
			return;
		}
		mineWarCrossRed = false;
		zbWsMsgHandler.Instance.CrossMine_ServerList_Req();
	}

	private void OnEdit()
	{
	}

	private void OnTouchs(ClickEvent evt, int index)
	{
	}

	public void OnClickBiaoji(ClickEvent evt)
	{
		m_homeMap.OnClickBiaoji();
	}

	public void OnPrivilege(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowTreasurePrivilege();
	}

	public void OnRule()
	{
		string[] langKeys = new string[23]
		{
			"lang.11004", "lang.11005", "lang.11006", "lang.11007", "", "lang.11008", "lang.11009", "lang.11010", "lang.11011", "lang.11012",
			"lang.11013", "lang.11014", "lang.11015", "", "lang.11016", "lang.11017", "lang.11018", "lang.11019", "lang.11020", "lang.11021",
			"lang.11022", "lang.11023", "lang.11024"
		};
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	public void OnLog()
	{
		zbWsMsgHandler.Instance.MineWar_RecordList_Req();
	}

	public void OnLock()
	{
		if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossStart < zbSelf.Instance.timeHandler.GetServerTime() && zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossEnd > zbSelf.Instance.timeHandler.GetServerTime())
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10981") + ":" + zbUtil.FormatTimeSmart(zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossEnd - zbSelf.Instance.timeHandler.GetServerTime()));
			((TextElement)m_lbServer).text = zbUtil.Language("lang.10981") + ":" + zbUtil.FormatTimeSmart(zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossEnd - zbSelf.Instance.timeHandler.GetServerTime());
		}
		else if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossStart > zbSelf.Instance.timeHandler.GetServerTime())
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10982") + ":" + zbUtil.FormatTimeSmart(zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossStart - zbSelf.Instance.timeHandler.GetServerTime()));
		}
	}

	public void OnClose(ClickEvent evt)
	{
		if (zbSelf.Instance.mineWarHandler.currServerId != zbSelf.Instance.GetServerId() && zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossStart < zbSelf.Instance.timeHandler.GetServerTime() && zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossEnd > zbSelf.Instance.timeHandler.GetServerTime())
		{
			OnClickServer(null);
		}
		else
		{
			Hide();
		}
	}

	public void OnClose_2(ClickEvent evt)
	{
		if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossStart < zbSelf.Instance.timeHandler.GetServerTime() && zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossEnd > zbSelf.Instance.timeHandler.GetServerTime())
		{
			OnClickServer(null);
		}
		else
		{
			zbWsMsgHandler.Instance.MineWar_Data_Req();
		}
	}

	public void OnTreasure(ClickEvent evt)
	{
		zbLinkHelper.OpenTreasure();
	}

	private void OnEventUpdateMineWarMapInfo(object data)
	{
		Refresh();
		m_homeMap.OnEventUpdateMineWarMapInfo(data);
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		RefreshNotify();
	}
}
