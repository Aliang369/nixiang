using System;
using System.Collections;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTruckMainWnd : zbTabWnd
{
	private GameObject m_spriteBottom;

	private SpriteRenderer m_spriteFoothold;

	private zbPlayerAvatar m_spineAvatar;

	private zbArenaBottom m_bottomPanel;

	private Transform bg1;

	private Transform bg2;

	private float bgHeight;

	private Label m_lbTitle;

	private Button m_btnAdd;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	private Button[] m_tabButtons = (Button[])(object)new Button[2];

	private VisualElement m_goldHead;

	private VisualElement m_detail;

	private Button m_btnDetailClose;

	private SpineVisualElement m_spineTruck;

	private Button m_btnHead;

	private Button m_btnFight;

	private Button m_btnGet;

	public VisualElement m_selectBg;

	private Label m_lbTruckTime;

	private VisualElement m_spriteTabSel;

	private zbTruckMainTabRob m_moduleRob;

	private zbTruckMainTabSelf m_moduleSelf;

	private Button m_btnBack;

	private Button m_btnRule;

	private Button m_btnRecord;

	private Button m_btnRefresh;

	private Label m_lbRefresh;

	private Button m_btnCard;

	private Button m_btnZhanLing;

	private SpineVisualElement m_spineCard;

	private VisualElement m_notifyCard;

	private VisualElement m_notifyZhanLing;

	private VisualElement m_notifyRecord;

	private VisualElement m_notifyOther;

	private VisualElement m_notifySelf;

	private RoleBaseInfo roleBaseInfo;

	private TruckTransportDetail truckTransportDetail;

	private TruckMatchDetail truckMatchDetail;

	private zbItem[] m_items = new zbItem[10];

	private readonly int m_refreshInterval = 10;

	private long m_nextRefreshTm;

	private int currTruckId;

	[NonSerialized]
	public bool b_btnTab;

	protected override void OnEnable()
	{
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0438: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spriteBottom = ((Component)m_GameObjectSceneRoot.transform.Find("bottomBg")).gameObject;
		m_spriteFoothold = ((Component)m_GameObjectSceneRoot.transform.Find("bottomBg").Find("foothold")).GetComponent<SpriteRenderer>();
		m_spineAvatar = ((Component)((Component)m_spriteFoothold).transform.Find("avatar")).GetComponent<zbPlayerAvatar>();
		m_bottomPanel = new zbArenaBottom();
		m_bottomPanel = UQueryExtensions.Q<zbArenaBottom>(m_RootElement, "arenaBottom", (string)null);
		m_bottomPanel.OnEnable(this, m_spriteFoothold, m_spineAvatar);
		bg1 = GetMyGameObjectSceneRoot().transform.Find("scrollScene").Find("bg1");
		bg2 = GetMyGameObjectSceneRoot().transform.Find("scrollScene").Find("bg2");
		Bounds bounds = ((Renderer)((Component)bg1).GetComponent<SpriteRenderer>()).bounds;
		bgHeight = ((Bounds)(ref bounds)).size.y;
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		for (int i = 0; i < 2; i++)
		{
			int idx = i;
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_tab" + i, (string)null);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_tabButtons[i], (Action<ClickEvent>)delegate
			{
				onGoBind(idx);
			}, "", (TrickleDown)0);
		}
		m_goldHead = UQueryExtensions.Q<VisualElement>(m_RootElement, "goldHead", (string)null);
		m_detail = UQueryExtensions.Q<VisualElement>(m_RootElement, "detail", (string)null);
		m_btnDetailClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_detailClose", (string)null);
		m_lbTruckTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_spineTruck = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_che", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>(m_RootElement, "btn_head", (string)null);
		m_btnFight = UQueryExtensions.Q<Button>(m_RootElement, "btn_one", (string)null);
		m_btnGet = UQueryExtensions.Q<Button>(m_RootElement, "btn_get", (string)null);
		m_selectBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "select_bg", (string)null);
		m_spriteTabSel = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTab", (string)null);
		m_selectBg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnRecord = UQueryExtensions.Q<Button>(m_RootElement, "btn_record", (string)null);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_btnRefresh = UQueryExtensions.Q<Button>(m_RootElement, "btn_refresh", (string)null);
		m_lbRefresh = UQueryExtensions.Q<Label>(m_RootElement, "lb_refresh", (string)null);
		m_btnCard = UQueryExtensions.Q<Button>(m_RootElement, "btn_card", (string)null);
		m_btnZhanLing = UQueryExtensions.Q<Button>(m_RootElement, "btn_zhanLing", (string)null);
		m_spineCard = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)m_btnCard, "spine_baofenka", (string)null);
		m_notifyCard = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnCard, (Vector2?)new Vector2(-5f, 0f), 1f, true);
		m_notifyCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_notifyZhanLing = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnZhanLing, (Vector2?)new Vector2(1f, 7f), 1f, true);
		m_notifyRecord = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnRecord, (Vector2?)new Vector2(1f, 7f), 1f, true);
		m_notifyOther = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[0], (Vector2?)new Vector2(1f, 7f), 1f, true);
		m_notifySelf = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[1], (Vector2?)new Vector2(1f, 7f), 1f, true);
		for (int num = 0; num < 10; num++)
		{
			m_items[num] = UQueryExtensions.Q<zbItem>(m_RootElement, $"item{num}", (string)null);
			m_items[num]?.OnEnable(this);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAdd, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRefresh, (Action)OnRefresh, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDetailClose, (Action)OnDetailClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnDetailHead, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action)OnDetailFight, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGet, (Action)OnDetailGet, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRecord, (Action)OnRecord, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCard, (Action)OnCard, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnZhanLing, (Action)OnZhanLing, "", (TrickleDown)0);
		InitTab(m_tabButtons, m_tabLayers, null, 1);
		zbLanguage.Instance.SetButtonTextVisible((Button[])(object)new Button[3] { m_btnRecord, m_btnRule, m_btnZhanLing });
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_TruckSelfInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_TruckMatchRefresh, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_JourneyInfo, delegate
		{
			RefreshZhanLing();
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_Aux_FreeGiftAward, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_ReminderList, RefreshNotify);
		m_gameEevent.RegisterEvent(EGameEvent.Event_TrainData_Ack, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_TrainTransportOpen_Ack, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_TrainRobMatchAck_Ack, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_TrainTransportAward_Ack, TrainTransportAward);
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(m_currTab, fromEvent: true);
	}

	public void RefreshCustom(int tabIndex = 1, bool fromEvent = false, bool fromTask = false)
	{
		zbSelf.Instance.truckHandler.bBestPrize = false;
		m_currTab = tabIndex;
		if (zbSelf.Instance.truckHandler.bFromBalance)
		{
			m_currTab = 0;
		}
		else if (!fromEvent)
		{
			zbWsMsgHandler.Instance.Train_Data_Req();
			if (zbSelf.Instance.truckHandler.GetTransportCombatLimit() - zbServerData.sGameData.TruckSelfInfo.TransportUsed > 0)
			{
				m_currTab = 1;
			}
			else
			{
				m_currTab = 0;
			}
		}
		if (fromTask)
		{
			m_currTab = tabIndex;
		}
		SelectTab(m_currTab);
		UpdatePerSecond();
		m_bottomPanel.Refresh();
		RefreshZhanLing();
		RefreshTruckCard();
		RefreshNotify();
		AutoFontSize();
		((MonoBehaviour)this).StartCoroutine(OnTransportEnumerator());
	}

	private void RefreshNotify(object data = null)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		m_notifyRecord.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.reminderHandler.HasReminderNotify(EReminderType.TruckRecord)));
		m_notifyOther.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.truckHandler.currRobTrain == null || zbSelf.Instance.truckHandler.currRobTrain.TransportId <= 0) ? 1 : 0));
		m_notifySelf.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.truckHandler.SelfTrainRedPoint()));
	}

	public IEnumerator OnTransportEnumerator()
	{
		yield return null;
		yield return null;
		yield return null;
		zbSelf.Instance.truckHandler.bFromBalance = false;
	}

	private void AutoFontSize()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (zbLanguage.Instance.GetLanguageType() == ELanguageType.Pt || zbLanguage.Instance.GetLanguageType() == ELanguageType.Ru || zbLanguage.Instance.GetLanguageType() == ELanguageType.Spa || zbLanguage.Instance.GetLanguageType() == ELanguageType.Vi || zbLanguage.Instance.GetLanguageType() == ELanguageType.Fr)
		{
			((VisualElement)m_lbTitle).style.fontSize = StyleLength.op_Implicit(26f);
		}
		else
		{
			((VisualElement)m_lbTitle).style.fontSize = StyleLength.op_Implicit(30f);
		}
	}

	private void RefreshZhanLing()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnZhanLing).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyTwoHandler.IsActive()));
		m_notifyZhanLing.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyTwoHandler.HasZhanLingNotify()));
	}

	public override void DoSelectTab()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		switch (m_currTab)
		{
		case 0:
		{
			if (m_moduleRob == null)
			{
				m_moduleRob = new zbTruckMainTabRob();
				m_moduleRob.OnEnable(this, m_tabLayers[0]);
			}
			m_moduleRob.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			m_goldHead.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			bool flag2 = zbSelf.Instance.truckHandler.GetRobBuyLimit() - zbServerData.sGameData.TruckSelfInfo.RobBought > 0;
			((VisualElement)m_btnAdd).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
			((TextElement)m_lbTitle).text = zbUtil.Language("lang.11127") + ":" + (zbSelf.Instance.truckHandler.GetRobCombatLimit() - zbServerData.sGameData.TruckSelfInfo.RobUsed);
			break;
		}
		case 1:
		{
			if (m_moduleSelf == null)
			{
				m_moduleSelf = new zbTruckMainTabSelf();
				m_moduleSelf.OnEnable(this, m_tabLayers[1]);
			}
			m_moduleSelf.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(360f, (LengthUnit)0));
			m_goldHead.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			bool flag = zbSelf.Instance.truckHandler.GetTransportCombatLimit() - zbServerData.sGameData.TruckSelfInfo.TransportUsed <= 0 && zbSelf.Instance.truckHandler.GetTransportBuyLimit() - zbServerData.sGameData.TruckSelfInfo.TransportBought > 0 && zbSelf.Instance.truckHandler.HaveTruckListTransport();
			((VisualElement)m_btnAdd).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			((TextElement)m_lbTitle).text = zbUtil.Language("lang.11121") + ":" + (zbSelf.Instance.truckHandler.GetTransportCombatLimit() - zbServerData.sGameData.TruckSelfInfo.TransportUsed);
			break;
		}
		}
		OnDetailClose();
		b_btnTab = false;
	}

	public void onGoBind(int tabIndex)
	{
		if (m_currTab != tabIndex)
		{
			b_btnTab = true;
		}
	}

	public void ShowDetail(int fromIndex, TruckMatchDetail _truckMatchDetail, TruckTransportDetail _truckTransportDetail, VisualElement itemElement, int TruckId = 0)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		itemElement.Add(m_selectBg);
		m_selectBg.SendToBack();
		((VisualElement)m_bottomPanel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_spriteBottom.SetActive(false);
		((VisualElement)m_btnCard).style.bottom = StyleLength.op_Implicit(630f);
		m_detail.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_selectBg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_btnRefresh).style.bottom = StyleLength.op_Implicit(740f);
		if (fromIndex == 1)
		{
			truckMatchDetail = _truckMatchDetail;
			roleBaseInfo = truckMatchDetail.RoleBaseInfo;
			truckTransportDetail = truckMatchDetail.TransportInfo;
			((VisualElement)m_btnFight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnGet).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			roleBaseInfo = zbServerData.sGameData.RoleBaseInfo;
			truckTransportDetail = _truckTransportDetail;
			((VisualElement)m_btnFight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnGet).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		currTruckId = TruckId;
		InitPlayerInfo();
		InitTruckInfo();
		RefreshPauseAward();
		UpdatePerSecond();
	}

	private void InitPlayerInfo()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)UQueryExtensions.Q<Label>(m_detail, "lb_name", (string)null)).text = roleBaseInfo.RoleName;
		string icon = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(roleBaseInfo.HeadId).icon;
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(icon);
		UQueryExtensions.Q<VisualElement>(m_detail, "header", (string)null).style.backgroundImage = new StyleBackground(val);
		string icon2 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(roleBaseInfo.HeadFrameId).icon;
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(icon2);
		UQueryExtensions.Q<VisualElement>(m_detail, "frame", (string)null).style.backgroundImage = new StyleBackground(val2);
		((TextElement)UQueryExtensions.Q<Label>(m_detail, "lb_power", (string)null)).text = zbUtil.ConvertToTBMK(roleBaseInfo.TeamAttack);
	}

	private void InitTruckInfo()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		m_spineTruck.AnimationState.SetAnimation(0, "che_" + (int)truckTransportDetail.Quality, true);
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truck_title" + (int)truckTransportDetail.Quality + ".png");
		UQueryExtensions.Q<VisualElement>(m_detail, "bg_quality", (string)null).style.backgroundImage = new StyleBackground(val);
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truck_middbg" + (int)truckTransportDetail.Quality + ".png");
		m_detail.style.backgroundImage = new StyleBackground(val2);
	}

	public void RefreshPauseAward()
	{
		int num = 0;
		foreach (AwardDetail award in truckTransportDetail.AwardList)
		{
			zbItemData data = new zbItemData(award.Type, award.Tid, award.Count);
			m_items[num].Refresh(data);
			if (num == 0)
			{
				m_items[num].AddBestPrizeTag(vis: true);
			}
			else
			{
				m_items[num].AddBestPrizeTag(vis: false);
			}
			m_items[num].SetTruckLossItemVisible(truckTransportDetail.RobList.Contains(num));
			num++;
		}
	}

	private void RefreshTruckCard()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		m_notifyCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.truckHandler.HasFreePackRedDot()));
		if (zbServerData.sGameData.TruckSelfInfo.Privilege == EBoolState.True)
		{
			m_spineCard.AnimationState.SetAnimation(0, "loop", true);
		}
		else
		{
			m_spineCard.AnimationState.SetAnimation(0, "normal", false);
		}
	}

	protected override void UpdateAtShow()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		bg1.Translate(Vector3.down * Time.deltaTime);
		bg2.Translate(Vector3.down * Time.deltaTime);
		if (bg1.position.y <= 0f - bgHeight)
		{
			Transform obj = bg1;
			obj.position += new Vector3(0f, 2f * bgHeight, 0f);
		}
		if (bg2.position.y <= 0f - bgHeight)
		{
			Transform obj2 = bg2;
			obj2.position += new Vector3(0f, 2f * bgHeight, 0f);
		}
	}

	protected override void UpdatePerSecond()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (m_detail.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			if (m_currTab == 0)
			{
				if (truckTransportDetail.ArriveTm - zbSelf.Instance.timeHandler.GetServerTime() < 600)
				{
					((TextElement)m_lbTruckTime).text = zbUtil.Language("lang.11128");
				}
				else
				{
					((TextElement)m_lbTruckTime).text = zbUtil.Language("lang.11122") + ":" + zbUtil.FormatTimeHHMMSS(truckTransportDetail.ArriveTm - zbSelf.Instance.timeHandler.GetServerTime());
				}
			}
			else if (truckTransportDetail.ArriveTm - zbSelf.Instance.timeHandler.GetServerTime() > 0)
			{
				((TextElement)m_lbTruckTime).text = zbUtil.Language("lang.11122") + ":" + zbUtil.FormatTimeHHMMSS(truckTransportDetail.ArriveTm - zbSelf.Instance.timeHandler.GetServerTime());
				((TextElement)m_btnGet).text = zbUtil.Language("lang.10215");
			}
			else
			{
				((TextElement)m_lbTruckTime).text = zbUtil.Language("lang.11136");
				((TextElement)m_btnGet).text = zbUtil.Language("lang.10090");
			}
		}
		RefreshTime();
		if (m_moduleSelf != null)
		{
			m_moduleSelf.UpdatePerSecond();
		}
		if (m_moduleRob != null)
		{
			m_moduleRob.UpdatePerSecond();
		}
	}

	private void RefreshTime()
	{
		long num = m_nextRefreshTm - zbSelf.Instance.timeHandler.GetServerTime();
		if (num > 0)
		{
			((TextElement)m_lbRefresh).text = zbUtil.FormatTimeMMSS(num);
			((VisualElement)m_btnRefresh).SetEnabled(false);
		}
		else
		{
			((TextElement)m_lbRefresh).text = "";
			((VisualElement)m_btnRefresh).SetEnabled(true);
		}
	}

	public void OnDetailClose()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		m_selectBg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_detail.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnCard).style.bottom = StyleLength.op_Implicit(470f);
		((VisualElement)m_btnRefresh).style.bottom = StyleLength.op_Implicit(580f);
		m_spriteBottom.SetActive(true);
		((VisualElement)m_bottomPanel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_btnRefresh).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_currTab != 0) ? 1 : 0));
	}

	public void OnDetailHead()
	{
		if (!zbUtil.IsNpc(roleBaseInfo.RoleId))
		{
			zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)roleBaseInfo.RoleId);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10882"));
		}
	}

	public void OnDetailFight()
	{
		zbUIMgr.Instance.ShowArenaDetail(EBattleFuncType.Truck, roleBaseInfo, null, null, 0, truckMatchDetail.TransportInfo);
	}

	public void OnDetailGet()
	{
		if (truckTransportDetail.ArriveTm - zbSelf.Instance.timeHandler.GetServerTime() > 0)
		{
			zbWsMsgHandler.Instance.Truck_TransportView_Req(currTruckId);
		}
		else
		{
			zbWsMsgHandler.Instance.Truck_TransportComplete_Req(currTruckId);
		}
	}

	public void OnClose()
	{
		zbSelf.Instance.truckHandler.bFromBalance = false;
		Hide();
	}

	private void OnRecord()
	{
		if (m_currTab == 0)
		{
			zbWsMsgHandler.Instance.Truck_RobRecordList_Req(openWnd: true);
		}
		else
		{
			zbWsMsgHandler.Instance.Truck_TransportRecordList_Req(openWnd: true);
		}
	}

	public void OnRule()
	{
		if (zbSelf.Instance.journeyTwoHandler.IsActive())
		{
			string[] langKeys = new string[19]
			{
				"lang.11379", "lang.11129", "lang.11130", "lang.11131", "lang.11132", "lang.11133", "lang.11146", "lang.11149", "lang.11134", "lang.11135",
				"", "lang.11380", "lang.11374", "", "lang.11375", "", "lang.11381", "", "lang.11382"
			};
			zbUIMgr.Instance.ShowRule(langKeys);
		}
		else
		{
			string[] langKeys2 = new string[18]
			{
				"lang.11379", "lang.11129", "lang.11130", "lang.11131", "lang.11132", "lang.11133", "lang.11146", "lang.11134", "lang.11135", "",
				"lang.11380", "lang.11374", "", "lang.11375", "", "lang.11381", "", "lang.11382"
			};
			zbUIMgr.Instance.ShowRule(langKeys2);
		}
	}

	private void OnCard()
	{
		zbUIMgr.Instance.ShowTruckHelpCardWnd();
	}

	private void OnZhanLing()
	{
		if (!zbSelf.Instance.journeyTwoHandler.IsActive())
		{
			RefreshZhanLing();
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10156"));
			zbUIMgr.Instance.HideWnd(EWndId.JourneyTwoMain);
		}
		else
		{
			zbUIMgr.Instance.ShowJourneyTwoZhanLing();
		}
	}

	private void OnRefresh()
	{
		if (m_nextRefreshTm <= zbSelf.Instance.timeHandler.GetServerTime())
		{
			m_nextRefreshTm = zbSelf.Instance.timeHandler.GetServerTime() + m_refreshInterval;
			zbWsMsgHandler.Instance.Truck_MatchList_Req();
		}
	}

	private void OnAdd()
	{
		if (m_currTab == 0)
		{
			zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.Truck, 1);
		}
		else
		{
			zbUIMgr.Instance.ShowMessageBox("Tip", string.Format(zbUtil.Language("lang.10736"), zbSelf.Instance.truckHandler.dailyTransportBuyDiamond, zbUtil.Language("钻石")) + string.Format(zbUtil.Language("lang.10487"), zbSelf.Instance.truckHandler.GetTransportBuyLimit() - zbServerData.sGameData.TruckSelfInfo.TransportBought), OnConfirm);
		}
	}

	private void OnConfirm(bool isOK)
	{
		if (isOK)
		{
			zbWsMsgHandler.Instance.Truck_BuyTransport_Req(1);
		}
	}

	public void TrainTransportAward(object data)
	{
		m_moduleSelf.TrainTransportAward(data);
	}
}
