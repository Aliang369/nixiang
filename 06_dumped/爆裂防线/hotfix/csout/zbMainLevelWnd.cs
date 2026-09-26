using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbMainLevelWnd : zbTabWnd
{
	public SkeletonAnimation m_spineDikuai;

	public Transform m_monsterAvatars;

	public SkeletonAnimation m_spineChallenge;

	private Sprite m_selectedLevelIcon;

	private Sprite m_unSelectedLevelIcon;

	private Sprite m_selectedChallengeIcon;

	private Sprite m_unSelectedChallengeIcon;

	private VisualElement m_nodeTabs;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	public Button[] m_tabButtons = (Button[])(object)new Button[2];

	public zbMainTabLevel m_moduleLevel;

	public zbMainTabChallenge m_moduleChallenge;

	private VisualElement m_doubleLevel;

	private VisualElement m_doubleChallenge;

	private Coroutine m_autoGetLevelAwardCoroutine;

	public Button m_btnSweep;

	private SpineVisualElement m_spineSweep;

	private int lastShowSweepState;

	private VisualElement m_nodeSweep;

	private Button m_btnBar;

	private SpineVisualElement m_spineBar;

	private string m_curBarAnim;

	private VisualElement m_nodeJourney;

	private SpineVisualElement m_spineJourney1;

	private SpineVisualElement m_spineJourney2;

	private SpineVisualElement m_spineJourney3;

	private Button m_btnJourney;

	private Label m_lbJourney;

	private VisualElement m_nodeTask;

	private zbTopLeftPlayerAvatar m_playerAvatar;

	public zbTopBar m_topBar;

	private Transform m_transformRank;

	private zbPlayerAvatar m_spineRankAvatar;

	private VisualElement m_nodeRank;

	private Label m_lbRankName;

	private Button m_btnRankTouch;

	private Button m_btnFuncPreview;

	private Button m_btnChat;

	private VisualElement m_nodeLeft;

	private zbMainLevelLeftPanel m_leftPanel = new zbMainLevelLeftPanel();

	private VisualElement m_nodeActivity;

	private ScrollView m_nodeRight;

	private zbMainLevelRightPanel m_rightPanel = new zbMainLevelRightPanel();

	public Button m_btnGrowthGift;

	private Label m_lbGrowthGiftTime;

	private SpineVisualElement m_growthGiftEffect;

	private Coroutine m_autoShowGrowthGiftCoroutine;

	[NonSerialized]
	public zbMainLevelTaskPanel m_taskPanel = new zbMainLevelTaskPanel();

	private Coroutine m_autoShowGuardActiveLevelCoroutine;

	public bool m_hasSendGuardActiveLevelReq;

	public bool m_waitShowGuardActiveLevel;

	private Button m_btnVipRefund;

	private ScrollView m_scrollView;

	private Button m_btnHoliday;

	private Button m_btnOpenServerRank;

	private VisualElement m_openServerRank;

	private Label m_openServerRankTime;

	private VisualElement m_notifyLevel;

	private VisualElement m_notifyChallenge;

	private VisualElement m_notifyTask;

	private VisualElement m_notifyBar;

	private VisualElement m_notifySweep;

	private VisualElement m_notifyOpenServerRank;

	private VisualElement m_notifyJourney;

	private VisualElement m_notifyGrowthGift;

	private VisualElement m_notifyHoliday;

	private VisualElement m_notifyFuncPreview;

	private int m_settleLimit = 86400;

	private bool m_isAchiveReport;

	private int m_scrollViewMaxHeight = 480;

	private float m_lastNoticReqSendTm;

	private bool m_lastIsRestaurant;

	private bool m_lastIsEndlessMine;

	private bool m_lastIsSnowVeil;

	private int m_lastIsBossSubjectID;

	private int m_lastWallSubjectID;

	private int m_lastMechaSubjectID;

	private RankDetail m_rankDetail;

	public int m_curLevel;

	private Button m_btnVipTq;

	private Label m_lblViplv;

	private VisualElement m_vipTqNotify;

	private Button m_btnRebate;

	protected override void OnEnable()
	{
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_080d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0838: Unknown result type (might be due to invalid IL or missing references)
		//IL_0863: Unknown result type (might be due to invalid IL or missing references)
		//IL_088e: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b9: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		GameObject gameObject = ((Component)GetMyGameObjectSceneRoot().transform.Find("spine_dikuai")).gameObject;
		m_spineDikuai = gameObject.GetComponent<SkeletonAnimation>();
		m_monsterAvatars = ((Component)m_GameObjectSceneRoot.transform.Find("monsterAvatar")).GetComponent<Transform>();
		GameObject gameObject2 = ((Component)GetMyGameObjectSceneRoot().transform.Find("spine_challenge")).gameObject;
		m_spineChallenge = gameObject2.GetComponent<SkeletonAnimation>();
		m_selectedLevelIcon = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/main/tabs/main_tab0_sel.png");
		m_unSelectedLevelIcon = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/main/tabs/main_tab0.png");
		m_selectedChallengeIcon = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/main/tabs/main_tab1_sel.png");
		m_unSelectedChallengeIcon = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/main/tabs/main_tab1.png");
		m_nodeTabs = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_tabs", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_tab" + i, (string)null);
		}
		m_notifyLevel = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[0], (Vector2?)new Vector2(-10f, -10f), 1f, true);
		m_notifyChallenge = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[1], (Vector2?)new Vector2(-10f, -10f), 1f, true);
		m_doubleLevel = UQueryExtensions.Q<VisualElement>(m_RootElement, "double_tab0", (string)null);
		m_doubleChallenge = UQueryExtensions.Q<VisualElement>(m_RootElement, "double_tab1", (string)null);
		m_btnGrowthGift = UQueryExtensions.Q<Button>(m_RootElement, "btn_growthGift", (string)null);
		m_lbGrowthGiftTime = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnGrowthGift, "lb_time", (string)null);
		m_growthGiftEffect = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)m_btnGrowthGift, "spine_effect", (string)null);
		m_topBar = new zbTopBar();
		m_topBar.OnEnable(this);
		m_playerAvatar = new zbTopLeftPlayerAvatar();
		m_playerAvatar.OnEnable(this);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, "scrollView", (string)null);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_scrollView, "unity-content-and-vertical-scroll-container", (string)null).pickingMode = (PickingMode)1;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_scrollView, "unity-content-viewport", (string)null).pickingMode = (PickingMode)1;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_scrollView, "unity-content-container", (string)null).pickingMode = (PickingMode)1;
		m_btnHoliday = UQueryExtensions.Q<Button>(m_RootElement, "btn_holiday", (string)null);
		((VisualElement)m_btnHoliday).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_btnOpenServerRank = UQueryExtensions.Q<Button>(m_RootElement, "btn_openServerRank", (string)null);
		m_openServerRank = UQueryExtensions.Q<VisualElement>(m_RootElement, "openServerRank", (string)null);
		m_openServerRankTime = UQueryExtensions.Q<Label>(m_openServerRank, "lb_time", (string)null);
		m_btnSweep = UQueryExtensions.Q<Button>(m_RootElement, "btn_sweep", (string)null);
		m_spineSweep = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_sweep", (string)null);
		m_nodeSweep = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_sweep", (string)null);
		m_btnBar = UQueryExtensions.Q<Button>(m_RootElement, "btn_bar", (string)null);
		m_spineBar = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_bar", (string)null);
		m_nodeJourney = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_journey", (string)null);
		m_spineJourney1 = UQueryExtensions.Q<SpineVisualElement>(m_nodeJourney, "spine_journey1", (string)null);
		m_spineJourney2 = UQueryExtensions.Q<SpineVisualElement>(m_nodeJourney, "spine_journey2", (string)null);
		m_spineJourney3 = UQueryExtensions.Q<SpineVisualElement>(m_nodeJourney, "spine_journey3", (string)null);
		m_btnJourney = UQueryExtensions.Q<Button>(m_nodeJourney, "btn_journey", (string)null);
		m_lbJourney = UQueryExtensions.Q<Label>(m_nodeJourney, "lb_journey", (string)null);
		m_nodeTask = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_task", (string)null);
		m_taskPanel.OnEnable(this, m_nodeTask);
		m_nodeLeft = UQueryExtensions.Q<VisualElement>(m_RootElement, "left", (string)null);
		m_leftPanel.OnEnable(this, m_nodeLeft);
		m_nodeActivity = UQueryExtensions.Q<VisualElement>(m_RootElement, "activity", (string)null);
		m_nodeRight = UQueryExtensions.Q<ScrollView>(m_RootElement, "scrollView", (string)null);
		Button btnUp = UQueryExtensions.Q<Button>(m_RootElement, "btn_up", (string)null);
		Button btnDown = UQueryExtensions.Q<Button>(m_RootElement, "btn_down", (string)null);
		m_rightPanel.OnEnable(this, m_nodeRight, btnUp, btnDown);
		m_btnChat = UQueryExtensions.Q<Button>(m_RootElement, "btn_chat", (string)null);
		m_btnFuncPreview = UQueryExtensions.Q<Button>(m_RootElement, "btn_funcPreview", (string)null);
		m_transformRank = ((Component)m_GameObjectSceneRoot.transform.Find("emptyObject").Find("rankPlayer")).GetComponent<Transform>();
		m_spineRankAvatar = ((Component)((Component)m_transformRank).transform.Find("rank")).GetComponent<zbPlayerAvatar>();
		m_nodeRank = UQueryExtensions.Q<VisualElement>(m_RootElement, "rank", (string)null);
		m_lbRankName = UQueryExtensions.Q<Label>(m_nodeRank, "rank_name", (string)null);
		m_btnRankTouch = UQueryExtensions.Q<Button>(m_nodeRank, "rank_touch", (string)null);
		m_btnVipRefund = UQueryExtensions.Q<Button>(m_RootElement, "btn_viprefund", (string)null);
		m_btnVipTq = UQueryExtensions.Q<Button>(m_RootElement, "btn_viptq", (string)null);
		m_lblViplv = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnVipTq, "viplv", (string)null);
		m_vipTqNotify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnVipTq, (Vector2?)new Vector2(-6f, -6f), 1f, true);
		m_btnRebate = UQueryExtensions.Q<Button>(m_RootElement, "btn_rebate", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRebate, (Action)OnRebate, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnVipTq, (Action)OnVipTq, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRankTouch, (Action)OnRankTouch, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSweep, (Action)OnSweep, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBar, (Action)OnBar, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnJourney, (Action)OnJourney, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHoliday, (Action)OnHoliday, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnChat, (Action<ClickEvent>)OnChat, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOpenServerRank, (Action)OnOpenServerRank, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnVipRefund, (Action)OnVipRefund, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGrowthGift, (Action)OnGrowthGift, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFuncPreview, (Action)OnFuncPreview, "", (TrickleDown)0);
		m_notifyTask = zbUtil.CreateNotifyTag((VisualElement)(object)m_taskPanel.m_btnTask, (Vector2?)new Vector2(10f, 10f), 1f, true);
		m_notifyBar = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnBar, (Vector2?)new Vector2(8f, 5f), 1f, true);
		m_notifySweep = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnSweep, (Vector2?)new Vector2(8f, 5f), 1f, true);
		m_notifyOpenServerRank = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnOpenServerRank, (Vector2?)new Vector2(0f, -2f), 1f, true);
		m_notifyJourney = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnJourney, (Vector2?)new Vector2(-3f, 8f), 1f, true);
		m_notifyGrowthGift = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGrowthGift, (Vector2?)new Vector2(0f, 10f), 1f, true);
		m_notifyHoliday = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnHoliday, (Vector2?)new Vector2(0f, -2f), 1f, true);
		m_notifyFuncPreview = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnFuncPreview);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Tili
		});
		CreateMainBottom(3);
		IosCheckHide();
		JObject record = zbDataConfig.Instance.idleBaseJsonTable.GetRecord();
		JToken val = ((record != null) ? record["gain"] : null);
		if (val != null)
		{
			m_settleLimit = Extensions.Value<int>((IEnumerable<JToken>)val[(object)"settleLimit"]);
		}
		m_curLevel = zbServerData.sGameData.MainQuestInfo.NextLevel;
		m_gameEevent.RegisterEvent(EGameEvent.Event_Quest_Refresh, OnUpdateEventQuestRefresh);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_RoleBaseInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_RoleCommonInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Suit_Refresh, OnEventUpdateAvatars);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Peerage_Data, OnUpdateEventPeerageData);
		m_gameEevent.RegisterEvent(EGameEvent.Event_SuccessOrder_Ack, RefreshMainWnd);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Activity, RefreshMainWnd);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, RefreshMainWnd);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_MainQuestRank, RefreshMainQuestRank);
		m_gameEevent.RegisterEvent(EGameEvent.Event_RebateAward_Ack, OnRebateAck);
		zbGuide.Instance.CheckNewGuideStep();
		m_lastNoticReqSendTm = Time.unscaledTime;
		InitTab(m_tabButtons, m_tabLayers, null);
	}

	private void IosCheckHide()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Application.platform == 8)
		{
			m_nodeLeft.style.top = StyleLength.op_Implicit(260f);
			m_nodeActivity.style.top = StyleLength.op_Implicit(260f);
		}
		((VisualElement)m_btnChat).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbChannel_Login.IsOpenTSCheck() ? 1 : 0));
	}

	private void OnUpdateEventQuestRefresh(object data)
	{
		m_moduleLevel?.Refresh();
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(m_currTab, m_curLevel);
	}

	private void OnEventUpdateAvatars(object data)
	{
		zbSelf.Instance.headHandler.localOwnedAvatars = zbSelf.Instance.headHandler.LoadLocalOwnedAvatars();
		zbSelf.Instance.headHandler.SyncAvatarsFromServer(((IEnumerable<int>)zbServerData.sGameData.HeadList).ToList());
	}

	private void OnUpdateEventPeerageData(object obj)
	{
		m_taskPanel.Refresh();
		RefreshDetail();
	}

	private void OnRebateAck(object obj)
	{
		RefreshRebate();
	}

	public override void Refresh()
	{
		zbLoginUtil.NoticListReq(open: false);
	}

	public void RefreshCustom(int tabIndex, int level)
	{
		if (level != -1)
		{
			m_curLevel = level;
		}
		SelectTab(tabIndex);
		AutoShowGrowthGift();
		RefreshDetail();
		RefreshFuncPreview();
		RefreshMainQuestRank();
		UpdateRedPoint();
		RefreshJourney();
		CheckVisible();
		RefreshGrowthGift();
		RefreshRebate();
		float unscaledTime = Time.unscaledTime;
		if (unscaledTime - m_lastNoticReqSendTm > 60f)
		{
			zbLoginUtil.NoticListReq(open: false);
			m_lastNoticReqSendTm = unscaledTime;
		}
	}

	private void RefreshRebate()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnRebate).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (zbServerData.sGameData.AuxAwardInfo != null)
		{
			RechargeRebate rechargeRebate = zbServerData.sGameData.AuxAwardInfo.AccountAwardInfo.RechargeRebate;
			if (rechargeRebate != null && rechargeRebate.State == ERebateState.WaitRebate && rechargeRebate.Amount > 0f)
			{
				((VisualElement)m_btnRebate).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
		}
	}

	public override void DoSelectTab()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		((Component)m_spineDikuai).gameObject.SetActive(m_currTab == 0);
		((Component)m_monsterAvatars).gameObject.SetActive(m_currTab == 0);
		((Component)m_spineChallenge).gameObject.SetActive(m_currTab == 1);
		((VisualElement)m_tabButtons[0]).style.backgroundImage = new StyleBackground((m_currTab == 0) ? m_selectedLevelIcon : m_unSelectedLevelIcon);
		((VisualElement)m_tabButtons[1]).style.backgroundImage = new StyleBackground((m_currTab == 1) ? m_selectedChallengeIcon : m_unSelectedChallengeIcon);
		switch (m_currTab)
		{
		case 0:
			if (m_moduleLevel == null)
			{
				m_moduleLevel = new zbMainTabLevel();
				m_moduleLevel.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleLevel.Refresh();
			break;
		case 1:
			if (m_moduleChallenge == null)
			{
				m_moduleChallenge = new zbMainTabChallenge();
				m_moduleChallenge.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleChallenge.Refresh();
			break;
		}
		RefreshMainQuestRank();
	}

	private void CheckVisible()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnVipTq).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbServerData.sGameData.MainQuestInfo.MainLevel < 2));
		((VisualElement)m_btnChat).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbChannel_Login.IsOpenTSCheck() || zbServerData.sGameData.MainQuestInfo.MainLevel < 5) ? 1 : 0));
		m_nodeLeft.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbServerData.sGameData.MainQuestInfo.MainLevel < 2));
		m_nodeActivity.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbServerData.sGameData.MainQuestInfo.MainLevel < 5));
	}

	public void AutoGetLevelAward(int level)
	{
		CheckVisible();
		if (m_autoGetLevelAwardCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_autoGetLevelAwardCoroutine);
			m_autoGetLevelAwardCoroutine = null;
		}
		m_autoGetLevelAwardCoroutine = ((MonoBehaviour)this).StartCoroutine(AutoGetLevelAwardCoroutine(level));
	}

	public void AutoShowGrowthGift()
	{
		if (zbSelf.Instance.growthGiftHandler.CheckCanPopupGrowthGift())
		{
			if (m_autoShowGrowthGiftCoroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(m_autoShowGrowthGiftCoroutine);
				m_autoShowGrowthGiftCoroutine = null;
			}
			m_autoShowGrowthGiftCoroutine = ((MonoBehaviour)this).StartCoroutine(AutoShowGrowthGiftCoroutine());
		}
	}

	private IEnumerator AutoShowGrowthGiftCoroutine()
	{
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		while (!zbUIMgr.Instance.IsTopCascadWnd(EWndId.MainLevel) || zbUIMgr.Instance.IsShow(EWndId.Guide) || zbUIMgr.Instance.IsShow(EWndId.GuideWeak) || zbUIMgr.Instance.IsShow(EWndId.LevelUp) || zbUIMgr.Instance.IsShow(EWndId.Func) || zbUIMgr.Instance.IsShow(EWndId.MainQuestReceiveAward))
		{
			yield return null;
		}
		GrowthGiftShopDetail growthGiftShopDetail = (zbSelf.Instance.growthGiftHandler.isNewGrowPack ? zbSelf.Instance.growthGiftHandler.FindNewActiveGrowthGift() : zbSelf.Instance.growthGiftHandler.FindEarliestExpiringGrowthGift());
		if (growthGiftShopDetail != null)
		{
			((VisualElement)m_btnGrowthGift).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (!zbUIMgr.Instance.IsShow(EWndId.GrowthGift) && zbSelf.Instance.growthGiftHandler.CheckCanPopupGrowthGift())
			{
				zbUIMgr.Instance.ShowGrowthGift(growthGiftShopDetail.GoodsId);
				zbSelf.Instance.growthGiftHandler.tmpRechargeShopGrowthGiftAck = null;
				zbSelf.Instance.growthGiftHandler.isNewGrowPack = false;
			}
		}
	}

	private IEnumerator AutoGetLevelAwardCoroutine(int level)
	{
		yield return null;
		yield return null;
		while (zbUIMgr.Instance.IsShow(EWndId.LevelUp))
		{
			yield return null;
		}
		yield return null;
		yield return null;
		while (zbUIMgr.Instance.IsShow(EWndId.Func))
		{
			yield return null;
		}
		if (zbSelf.Instance.rankHandler.isMainQuestRankUp())
		{
			zbUIMgr.Instance.ShowRank(0, delegate(bool isOK)
			{
				OnCallBackMainQuestReceiveAward(isOK, level);
			});
		}
		else
		{
			OnCallBackMainQuestReceiveAward(isOK: true, level);
		}
		yield return null;
	}

	private void OnCallBackMainQuestReceiveAward(bool isOK, int level)
	{
		bool flag = false;
		for (int i = 0; i < 3; i++)
		{
			if (zbSelf.Instance.mainQuestHandler.CanClaimReward(level, i))
			{
				flag = true;
				zbUIMgr.Instance.ShowMainQuestReceiveAward(isMain: true, level, OnCallBackLevel2FirstResult);
				break;
			}
		}
		if (!flag)
		{
			OnCallBackLevel2FirstResult(isOK: true);
		}
	}

	private void OnCallBackLevel2FirstResult(bool isOK)
	{
		if (zbSelf.Instance.mainQuestHandler.isLevel2FirstResult)
		{
			zbSelf.Instance.mainQuestHandler.isLevel2FirstResult = false;
			if (!zbSelf.Instance.activityHandler.IsFirstRechargeEnd())
			{
				zbUIMgr.Instance.ShowFirstRecharge();
			}
		}
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (m_autoGetLevelAwardCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_autoGetLevelAwardCoroutine);
			m_autoGetLevelAwardCoroutine = null;
		}
		if (m_autoShowGrowthGiftCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_autoShowGrowthGiftCoroutine);
			m_autoShowGrowthGiftCoroutine = null;
		}
	}

	private void RefreshDetail()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		RefreshMainWnd();
		RefreshRightTop();
		m_nodeTabs.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Explore) ? 1 : 0));
		m_nodeSweep.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Idle) ? 1 : 0));
		m_topBar.Refresh();
		((TextElement)m_lblViplv).text = $"VIP{zbSelf.Instance.GetVipLevel()}";
	}

	public void RefreshGrowthGift()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		GrowthGiftShopDetail detail = zbSelf.Instance.growthGiftHandler.FindEarliestExpiringGrowthGift();
		bool flag = detail != null;
		((VisualElement)m_btnGrowthGift).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		if (flag)
		{
			zbGiftTaskCsvRecord zbGiftTaskCsvRecord2 = zbDataConfig.Instance.giftTaskCsvTable.GetRecords().records.FirstOrDefault((zbGiftTaskCsvRecord record) => record.goodsId == detail.GoodsId);
			m_growthGiftEffect.SkeletonDataAsset = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(zbGiftTaskCsvRecord2.iconPath);
			m_growthGiftEffect.Initialize(true);
			m_growthGiftEffect.AnimationState.ClearTrack(0);
			m_growthGiftEffect.AnimationState.SetAnimation(0, "animation2", true);
		}
	}

	private void RefreshRightTop()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_045c: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		bool flag = zbSelf.Instance.rankHandler.IsOpenServerOpen();
		m_openServerRank.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		if (flag)
		{
			bool flag2 = zbServerData.sGameData.OpenServerInfo.RankBaseList[0].SettleTm - zbSelf.Instance.timeHandler.GetServerTime() < 0;
			((TextElement)m_openServerRankTime).text = (flag2 ? zbUtil.Language("lang.10397") : zbUtil.FormatTimeHHMMSS(zbServerData.sGameData.OpenServerInfo.RankBaseList[0].SettleTm - zbSelf.Instance.timeHandler.GetServerTime()));
		}
		bool flag3 = zbSelf.Instance.restaurantHandler.GetRestaurantDetail() != null;
		bool flag4 = zbSelf.Instance.endlessMineHandler.GetEndlessMineDetail() != null;
		bool flag5 = zbSelf.Instance.snowVeilHandler.GetSnowVeilDetail() != null;
		BossSubjectDetail bossSubjectDetail = zbSelf.Instance.bossSubjectHandler.GetBossSubjectDetail();
		WallSubjectDetail wallSubjectDetail = zbSelf.Instance.wallSubjectHandler.GetWallSubjectDetail();
		MechaSubjectDetail mechaSubjectDetail = zbSelf.Instance.mechaSubjectHandler.GetMechaSubjectDetail();
		if (flag3 != m_lastIsRestaurant || flag4 != m_lastIsEndlessMine || flag5 != m_lastIsSnowVeil || (bossSubjectDetail != null && m_lastIsBossSubjectID != bossSubjectDetail.BossSubject.SubjectId) || (wallSubjectDetail != null && m_lastWallSubjectID != wallSubjectDetail.SubjectId) || (mechaSubjectDetail != null && m_lastMechaSubjectID != mechaSubjectDetail.SubjectId))
		{
			m_lastIsRestaurant = flag3;
			m_lastIsEndlessMine = flag4;
			m_lastIsSnowVeil = flag5;
			if (bossSubjectDetail != null && m_lastIsBossSubjectID != bossSubjectDetail.BossSubject.SubjectId)
			{
				m_lastIsBossSubjectID = bossSubjectDetail.BossSubject.SubjectId;
			}
			if (wallSubjectDetail != null && m_lastWallSubjectID != wallSubjectDetail.SubjectId)
			{
				m_lastWallSubjectID = wallSubjectDetail.SubjectId;
			}
			if (mechaSubjectDetail != null && m_lastMechaSubjectID != mechaSubjectDetail.SubjectId)
			{
				m_lastMechaSubjectID = mechaSubjectDetail.SubjectId;
			}
			bool flag6 = (flag3 | flag4 | flag5) || bossSubjectDetail != null || wallSubjectDetail != null || mechaSubjectDetail != null;
			((VisualElement)m_btnHoliday).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag6));
			if (flag6)
			{
				if (flag3)
				{
					((VisualElement)m_btnHoliday).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/main/right/main_restaurant.png"));
				}
				else if (flag4)
				{
					((VisualElement)m_btnHoliday).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/main/right/main_endlessMine.png"));
				}
				else if (flag5)
				{
					((VisualElement)m_btnHoliday).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/main/right/main_snowVeil.png"));
				}
				else if (bossSubjectDetail != null)
				{
					zbBossSubjectCsvRecord recordByKey = zbDataConfig.Instance.bossSubjectCsvTable.GetRecordByKey(bossSubjectDetail.BossSubject.SubjectId);
					zbSelf.Instance.bossSubjectHandler.pathName = "bossSubject_" + recordByKey.boss + "/";
					((VisualElement)m_btnHoliday).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.banerIcon));
				}
				else if (wallSubjectDetail != null)
				{
					zbWallSubjectCsvRecord recordByKey2 = zbDataConfig.Instance.wallSubjectCsvTable.GetRecordByKey(wallSubjectDetail.SubjectId);
					zbSelf.Instance.wallSubjectHandler.pathName = "wallSubject_" + wallSubjectDetail.SubjectId + "/";
					((VisualElement)m_btnHoliday).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.banerIcon));
				}
				else if (mechaSubjectDetail != null)
				{
					zbMechaSubjectCsvRecord recordByKey3 = zbDataConfig.Instance.mechaSubjectCsvTable.GetRecordByKey(mechaSubjectDetail.SubjectId);
					zbSelf.Instance.mechaSubjectHandler.pathName = "mechaSubject_" + mechaSubjectDetail.SubjectId + "/";
					((VisualElement)m_btnHoliday).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey3.banerIcon));
				}
			}
		}
		int num = ((flag && ((flag3 | flag4 | flag5) || bossSubjectDetail != null || wallSubjectDetail != null || mechaSubjectDetail != null)) ? 320 : 535);
		if (!Mathf.Approximately((float)num, (float)m_scrollViewMaxHeight))
		{
			m_scrollViewMaxHeight = num;
			((VisualElement)m_nodeRight).style.maxHeight = StyleLength.op_Implicit((float)num);
		}
	}

	private void RefreshFuncPreview()
	{
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		int peerageId = zbServerData.sGameData.PeerageInfo.PeerageId;
		zbFunctionUnlockCsvRecord nextRecordByPeerageId = zbSelf.Instance.funcUnLockHandler.GetNextRecordByPeerageId(peerageId);
		int[] array = (from r in zbDataConfig.Instance.functionUnlockCsvTable.GetRecords().records.Values
			where r.unlockType == 2 && r.unlockVal <= zbServerData.sGameData.PeerageInfo.PeerageId
			select r.id into id
			where !zbServerData.sGameData.AuxAwardInfo.FuncList.Contains(id)
			select id).ToArray();
		bool flag = nextRecordByPeerageId != null || array.Length != 0;
		((VisualElement)m_btnFuncPreview).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!flag || zbServerData.sGameData.MainQuestInfo.MainLevel < 2) ? 1 : 0));
		if (flag)
		{
			StyleBackground backgroundImage = default(StyleBackground);
			if (nextRecordByPeerageId != null)
			{
				((StyleBackground)(ref backgroundImage))._002Ector(zbAssetLoader.Instance.LoadResouce<Sprite>(nextRecordByPeerageId.icon));
			}
			else
			{
				((StyleBackground)(ref backgroundImage))._002Ector(zbAssetLoader.Instance.LoadResouce<Sprite>(zbSelf.Instance.funcUnLockHandler.GetLastRecord().icon));
			}
			UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnFuncPreview, "icon_func", (string)null).style.backgroundImage = backgroundImage;
		}
	}

	private void RefreshMainQuestRank(object obj = null)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		m_rankDetail = zbSelf.Instance.rankHandler.GetLastMainQuestRankDetail();
		((Component)m_transformRank).gameObject.SetActive(m_rankDetail != null && m_currTab == 0);
		m_nodeRank.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_rankDetail == null || m_currTab != 0) ? 1 : 0));
		if (m_rankDetail != null)
		{
			if (m_rankDetail.RoleBaseInfo != null)
			{
				m_spineRankAvatar.RefreshCustom(m_rankDetail.RoleBaseInfo.AvatarId, m_rankDetail.RoleBaseInfo.GunId);
				m_spineRankAvatar.RandomAnimation();
			}
			if (m_rankDetail.RankData != null)
			{
				((TextElement)m_lbRankName).text = zbUtil.Language("lang.10824") + ": " + m_rankDetail.RankData.Rank;
			}
			if (((VisualElement)m_btnFuncPreview).style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
			{
				m_transformRank.position = new Vector3(2f, -1.2f, -1.19f);
			}
			else
			{
				m_transformRank.position = new Vector3(2.9f, -1.2f, -1.19f);
			}
		}
	}

	public void RefreshMainWnd(object data = null)
	{
		m_playerAvatar.Refresh();
		m_leftPanel.Refresh();
		m_rightPanel.Refresh();
		m_taskPanel.Refresh();
		RefreshVipRefund();
	}

	private void RefreshVipRefund()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnVipRefund).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (zbServerData.sGameData.RechargeActiveInfo != null && zbServerData.sGameData.RechargeActiveInfo.ChargebackInfo != null)
		{
			ChargebackInfo chargebackInfo = zbServerData.sGameData.RechargeActiveInfo.ChargebackInfo;
			if (chargebackInfo.State == EChargebackState.Warning || chargebackInfo.State == EChargebackState.Danger)
			{
				((VisualElement)m_btnVipRefund).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
		}
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		m_taskPanel.OnUpdateTask();
		if (zbGuide.Instance.canGuideStep != EGuideStep.guide_step_None && !zbGuide.Instance.IsGuiding() && zbUIMgr.Instance.IsTopCascadWnd(GetWndId()))
		{
			zbGuide.Instance.ExecGuide(zbGuide.Instance.canGuideStep);
		}
	}

	private void UpdateGrowthGift()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		GrowthGiftShopDetail growthGiftShopDetail = zbSelf.Instance.growthGiftHandler.FindEarliestExpiringGrowthGift();
		bool flag = (int)((VisualElement)m_btnGrowthGift).resolvedStyle.display == 0;
		if (growthGiftShopDetail == null)
		{
			if (flag)
			{
				((VisualElement)m_btnGrowthGift).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			return;
		}
		long num = growthGiftShopDetail.ExpireTm - zbSelf.Instance.timeHandler.GetServerTime();
		if (num <= 0)
		{
			if (flag)
			{
				((VisualElement)m_btnGrowthGift).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
		else
		{
			((TextElement)m_lbGrowthGiftTime).text = zbUtil.FormatTimeHHMMSS(num);
		}
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		zbWsMsgHandler.Instance.Rank_Top_Req();
		ReqOpenServerRankTop();
		UpdateRedPoint();
		AutoShowGrowthGift();
		RefreshGrowthGift();
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		UpdateRedPoint();
		AutoShowGrowthGift();
		RefreshGrowthGift();
	}

	private void ReqOpenServerRankTop()
	{
		if (zbSelf.Instance.rankHandler.IsOpenServerOpen())
		{
			zbWsMsgHandler.Instance.OpenServer_RankTop_Req();
		}
	}

	private void UpdateRedPoint()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		m_notifyLevel.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.MainQuest)));
		m_notifyChallenge.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Challenge)));
		m_notifyTask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Task)));
		m_notifyOpenServerRank.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.OpenServerRank)));
		m_notifyBar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Bar)));
		m_notifyJourney.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Journey)));
		m_notifyGrowthGift.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.GrowthGift)));
		m_notifyHoliday.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Holiday)));
		m_notifyFuncPreview.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.funcUnLockHandler.HasRedPoint()));
		m_doubleLevel.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.vipHandler.GetDoubleAwardFunType() != EFuncType.MainQuest));
		m_doubleChallenge.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.vipHandler.GetDoubleAwardFunType() != EFuncType.EliteQuest && zbSelf.Instance.vipHandler.GetDoubleAwardFunType() != EFuncType.Legendary && (!zbSelf.Instance.journeyThreeHandler.IsEnd() || zbSelf.Instance.vipHandler.GetDoubleAwardFunType() != EFuncType.Facility)) ? 1 : 0));
		m_playerAvatar.UpdateRedPoint();
		m_topBar.UpdateRedPoint();
		m_leftPanel.UpdateRedPoint();
		m_rightPanel.RefreshDetail();
		m_moduleLevel?.RefreshNotify();
		m_moduleChallenge?.RefreshNotify();
		m_vipTqNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!zbSelf.Instance.vipHandler.hasVipFreeExp() && !zbSelf.Instance.vipHandler.hasDailyFreeGift()) ? 1 : 0));
	}

	private void OnFuncPreview()
	{
		zbUIMgr.Instance.ShowFuncPreview();
	}

	private void RefreshJourney()
	{
		long endLeftTime = zbSelf.Instance.journeyOneHandler.GetEndLeftTime();
		long endLeftTime2 = zbSelf.Instance.journeyTwoHandler.GetEndLeftTime();
		long endLeftTime3 = zbSelf.Instance.journeyThreeHandler.GetEndLeftTime();
		bool flag = endLeftTime > 0;
		bool flag2 = endLeftTime2 > 0;
		bool flag3 = endLeftTime3 > 0;
		zbUtil.SetDisplayIfChanged((VisualElement)(object)m_spineJourney1, (DisplayStyle)(!flag));
		zbUtil.SetDisplayIfChanged((VisualElement)(object)m_spineJourney2, (DisplayStyle)(!flag2));
		zbUtil.SetDisplayIfChanged((VisualElement)(object)m_spineJourney3, (DisplayStyle)(!flag3));
		long num = (flag ? endLeftTime : (flag2 ? endLeftTime2 : (flag3 ? endLeftTime3 : 0)));
		if (num > 0)
		{
			((TextElement)m_lbJourney).text = zbUtil.FormatTimeHHMMSS(num);
		}
		zbUtil.SetDisplayIfChanged(m_nodeJourney, (DisplayStyle)((num <= 0) ? 1 : 0));
	}

	protected override void UpdatePerSecond()
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		base.UpdatePerSecond();
		UpdateRedPoint();
		UpdateGrowthGift();
		m_taskPanel.RefreshGuideFinger();
		RefreshRightTop();
		RefreshJourney();
		float num = (float)(zbSelf.Instance.timeHandler.GetServerTime() - zbServerData.sGameData.IdleInfo.GainTm) / (float)m_settleLimit;
		int num2 = 0;
		num2 = (((double)num <= 0.05) ? 1 : (((double)num <= 0.2) ? 2 : ((!((double)num <= 0.5)) ? 4 : 3)));
		if (lastShowSweepState != num2)
		{
			m_spineSweep.AnimationState.SetAnimation(0, num2.ToString(), true);
			lastShowSweepState = num2;
			m_notifySweep.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(num2 < 3));
		}
		string text = ((zbSelf.Instance.operateHandler.GetTimeHeroCount() > 0) ? "animation3" : (zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Bar) ? "animation2" : "animation"));
		if (m_curBarAnim != text)
		{
			m_spineBar.AnimationState.SetAnimation(0, text, true);
			m_curBarAnim = text;
		}
		if (!m_isAchiveReport)
		{
			m_isAchiveReport = true;
			zbChannel_Achive.ReportAchive_Channel();
		}
	}

	private void OnVipTq()
	{
		zbSelf.Instance.vipHandler.openVipTqWnd();
	}

	private void OnRebate()
	{
		zbUIMgr.Instance.ShowRebateAwardWnd();
	}

	private void OnRankTouch()
	{
		if (m_rankDetail != null)
		{
			zbLinkHelper.OpenRank();
		}
	}

	public void OnChat(ClickEvent evt)
	{
		zbLinkHelper.OpenChat();
	}

	public void OnHoliday()
	{
		zbSelf.Instance.holidayHandler.OpenHoliday();
	}

	public void OnOpenServerRank()
	{
		zbLinkHelper.OpenServerRank();
	}

	public void OnVipRefund()
	{
		zbUIMgr.Instance.ShowVipRefund();
	}

	public void OnSweep()
	{
		zbLinkHelper.OpenWallIdle();
	}

	public void ShowSweepFinger()
	{
		zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)m_btnSweep);
	}

	public void OnBar()
	{
		zbUIMgr.Instance.ShowLottery(2);
	}

	public void OnJourney()
	{
		zbLinkHelper.OpenJourneyMain();
	}

	private void OnGrowthGift()
	{
		GrowthGiftShopDetail growthGiftShopDetail = zbSelf.Instance.growthGiftHandler.FindEarliestExpiringGrowthGift();
		zbUIMgr.Instance.ShowGrowthGift(growthGiftShopDetail.GoodsId);
	}
}
