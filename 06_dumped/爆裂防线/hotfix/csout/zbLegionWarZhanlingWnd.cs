using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarZhanlingWnd : zbBaseWnd
{
	private Label m_lbTitle;

	private Label m_lbTime;

	private VisualElement m_time;

	private Label m_lbLv;

	private Label m_lbDesc;

	private Label m_lbCount;

	private VisualElement m_progressVal;

	private VisualElement m_banner;

	private VisualElement m_header;

	private ListView m_listView;

	private VisualElement m_btns;

	private Button m_btnReceive;

	private Button m_btnBuyMid;

	private Button m_btnBuyHigh;

	private Button m_btnBack;

	private Button m_btnLevelUp;

	private Button m_btnModList;

	private VisualElement m_zhanlingUpgradeNotify;

	private Button m_btnLegionWarTask;

	private VisualElement m_bottom;

	private VisualElement m_iconArrowHigh;

	private VisualElement m_progressInner;

	private float m_timer;

	private IVisualElementScheduledItem m_arrowAnim;

	protected override void OnEnable()
	{
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_time = UQueryExtensions.Q<VisualElement>(m_RootElement, "time", (string)null);
		m_lbLv = UQueryExtensions.Q<Label>(m_RootElement, "lb_lv", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_progressVal = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_progressInner = UQueryExtensions.Q<VisualElement>(m_RootElement, "progressinner", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		m_banner = UQueryExtensions.Q<VisualElement>(m_RootElement, "banner", (string)null);
		m_header = UQueryExtensions.Q<VisualElement>(m_RootElement, "header", (string)null);
		m_bottom = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_bottom", (string)null);
		EnableListView();
		m_btns = UQueryExtensions.Q<VisualElement>(m_RootElement, "btns", (string)null);
		m_btnReceive = UQueryExtensions.Q<Button>(m_RootElement, "btn_receive", (string)null);
		m_btnBuyHigh = UQueryExtensions.Q<Button>(m_RootElement, "btn_buyHigh", (string)null);
		m_btnBuyMid = UQueryExtensions.Q<Button>(m_RootElement, "btn_buyMid", (string)null);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnLevelUp = UQueryExtensions.Q<Button>(m_RootElement, "btn_levelup", (string)null);
		m_btnModList = UQueryExtensions.Q<Button>(m_RootElement, "btn_modList", (string)null);
		m_btnLegionWarTask = UQueryExtensions.Q<Button>(m_RootElement, "btn_legionWarTask", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLegionWarTask, (Action)OnTask, "", (TrickleDown)0);
		m_iconArrowHigh = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_arrowHigh", (string)null);
		m_arrowAnim = m_iconArrowHigh.schedule.Execute((Action)delegate
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			m_timer += 0.05f;
			float num = Mathf.Sin(m_timer * 3f) * 10f;
			m_iconArrowHigh.style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(0f), Length.op_Implicit(num), 0f));
		}).Every(16L);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Honor });
		m_zhanlingUpgradeNotify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnLevelUp, (Vector2?)new Vector2(1f, 7f), 1f, true);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnReceive, (Action)OnReceive, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuyMid, (Action)OnBuyMid, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuyHigh, (Action)OnBuyHigh, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLevelUp, (Action)OnLevelUp, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnModList, (Action)OnModList, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_LegionWarData, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_LegionWarZhanlingLevelUp, OnEventUpdateZhanlingLevelUp);
	}

	public void RefreshCustom()
	{
		InitBtns();
		InitBanner();
		UpdateNotify();
		UpdateListView();
		RefreshLeftTime();
	}

	private void EnableListView()
	{
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 110f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_listView.makeItem = delegate
		{
			zbLegionWarZhanlingItem obj = new zbLegionWarZhanlingItem();
			obj.OnEnable(this);
			return (VisualElement)(object)obj;
		};
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			List<zbLegionWarLevelCsvRecord> list = ((BaseVerticalCollectionView)m_listView).itemsSource as List<zbLegionWarLevelCsvRecord>;
			(element as zbLegionWarZhanlingItem).Init(list[index]);
		};
	}

	private void UpdateListView()
	{
		LegionWarZhanLingInfo legionWarZhanLingInfo = zbServerData.sGameData.LegionWarBaseInfo.LegionWarZhanLingInfo;
		List<zbLegionWarLevelCsvRecord> list = zbDataConfig.Instance.legionWarLevelCsvTable.GetRecords().records.Values.ToList();
		((BaseVerticalCollectionView)m_listView).itemsSource = list;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
		((BaseVerticalCollectionView)m_listView).ScrollToItemById(Mathf.Min((legionWarZhanLingInfo.Level > 2) ? (legionWarZhanLingInfo.Level + 2) : legionWarZhanLingInfo.Level, list.Count - 4));
	}

	private void InitBanner()
	{
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		LegionWarZhanLingInfo legionWarZhanLingInfo = zbServerData.sGameData.LegionWarBaseInfo.LegionWarZhanLingInfo;
		LegionWarSelfInfo legionWarSelfInfo = zbSelf.Instance.legionWarHandler.legionWarSelfInfo;
		Dictionary<int, zbLegionWarLevelCsvRecord> records = zbDataConfig.Instance.legionWarLevelCsvTable.GetRecords().records;
		zbLegionWarLevelCsvRecord recordByKey = zbDataConfig.Instance.legionWarLevelCsvTable.GetRecordByKey(legionWarZhanLingInfo.Level + 1);
		bool flag = legionWarZhanLingInfo.Level == records.Count || recordByKey == null;
		ELegionWarState legionWarState = zbSelf.Instance.legionWarHandler.GetLegionWarState();
		((TextElement)m_lbLv).text = legionWarZhanLingInfo.Level.ToString();
		((TextElement)m_lbCount).text = (flag ? "Max" : (((legionWarState == ELegionWarState.Fighting) ? legionWarSelfInfo.SelfScore : legionWarZhanLingInfo.Score) + "/" + recordByKey.score));
		float num = (flag ? 1f : Mathf.Clamp01((float)((legionWarState == ELegionWarState.Fighting) ? legionWarSelfInfo.SelfScore : legionWarZhanLingInfo.Score) / (float)recordByKey.score));
		bool flag2 = num * 100f >= 100f;
		m_zhanlingUpgradeNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!flag2 || zbSelf.Instance.legionWarHandler.IsZhanlingMax()) ? 1 : 0));
		m_progressVal.style.width = new StyleLength(Length.Percent(num * 100f));
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/peerage/" + (flag2 ? "progress388x24_1.png" : "progress388x24_2.jpg"));
		if ((Object)(object)val != (Object)null && (Object)(object)val.texture != (Object)null)
		{
			m_progressInner.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val.texture));
			m_progressInner.MarkDirtyRepaint();
		}
	}

	private void InitBtns()
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		JObject record = zbDataConfig.Instance.vipBaseJsonTable.GetRecord();
		int goodId = Extensions.Value<int>((IEnumerable<JToken>)record["legionWar"][(object)"mediumGoodsId"]);
		int goodId2 = Extensions.Value<int>((IEnumerable<JToken>)record["legionWar"][(object)"highGoodsId"]);
		bool flag = zbServerData.sGameData.LegionWarBaseInfo.LegionWarZhanLingInfo.MediumActive == EBoolState.True;
		bool flag2 = zbServerData.sGameData.LegionWarBaseInfo.LegionWarZhanLingInfo.HighActive == EBoolState.True;
		ELegionWarState legionWarState = zbSelf.Instance.legionWarHandler.GetLegionWarState();
		((VisualElement)m_btnBuyMid).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		((VisualElement)m_btnBuyHigh).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag2 ? 1 : 0));
		((VisualElement)m_btnReceive).style.left = StyleLength.op_Implicit((flag & flag2) ? 247f : 13f);
		((VisualElement)m_btnModList).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(legionWarState != ELegionWarState.Fighting));
		((TextElement)m_btnBuyMid).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(goodId);
		((TextElement)m_btnBuyHigh).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(goodId2);
	}

	private void OnBuyMid()
	{
		int goodsId = Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.vipBaseJsonTable.GetRecord()["legionWar"][(object)"mediumGoodsId"]);
		if (zbServerData.sGameData.LegionWarBaseInfo.LegionWarZhanLingInfo.MediumActive != EBoolState.True)
		{
			zbChannel_Pay.OnPay(goodsId);
		}
	}

	private void OnBuyHigh()
	{
		int goodsId = Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.vipBaseJsonTable.GetRecord()["legionWar"][(object)"highGoodsId"]);
		if (zbServerData.sGameData.LegionWarBaseInfo.LegionWarZhanLingInfo.HighActive != EBoolState.True)
		{
			zbChannel_Pay.OnPay(goodsId);
		}
	}

	private void OnReceive()
	{
		if (zbSelf.Instance.legionWarHandler.CanReceiveZhanling())
		{
			zbWsMsgHandler.Instance.LegionWar_ZhanLingAward_Req();
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10391"));
		}
	}

	private void OnTask()
	{
		zbUIMgr.Instance.ShowLegionWarTask();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		UpdateNotify();
		RefreshLeftTime();
	}

	private void UpdateNotify()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		m_iconArrowHigh.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.legionWarHandler.CanUpgradeZhanling()));
	}

	private void RefreshLeftTime()
	{
		if (zbSelf.Instance.legionWarHandler.GetLegionWarState() == ELegionWarState.Fighting)
		{
			long seconds = zbSelf.Instance.legionWarHandler.getLegionWarOpenInfo().FightingEndTm - zbSelf.Instance.timeHandler.GetServerTime();
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(seconds);
		}
		else
		{
			((TextElement)m_lbTime).text = zbUtil.Language("lang.10397");
		}
	}

	public void SetVisibleUI(bool flag)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		m_banner.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_header.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((VisualElement)m_listView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((VisualElement)m_lbTitle).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_time.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_btns.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_bottom.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom();
	}

	private void OnEventUpdateZhanlingLevelUp(object data)
	{
		RefreshCustom();
		if (zbSelf.Instance.legionWarHandler.GetLegionWarState() == ELegionWarState.Fighting)
		{
			zbUIMgr.Instance.ShowLegionWarMod();
		}
	}

	private void OnLevelUp()
	{
		LegionWarZhanLingInfo legionWarZhanLingInfo = zbServerData.sGameData.LegionWarBaseInfo.LegionWarZhanLingInfo;
		LegionWarSelfInfo legionWarSelfInfo = zbSelf.Instance.legionWarHandler.legionWarSelfInfo;
		zbLegionWarLevelCsvRecord recordByKey = zbDataConfig.Instance.legionWarLevelCsvTable.GetRecordByKey(legionWarZhanLingInfo.Level + 1);
		if (recordByKey == null)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11215"));
		}
		else if ((zbSelf.Instance.legionWarHandler.GetLegionWarState() == ELegionWarState.Fighting) ? (legionWarSelfInfo.SelfScore >= recordByKey.score) : (legionWarZhanLingInfo.Score >= recordByKey.score))
		{
			zbSelf.Instance.legionWarHandler.PreZhanlingInfo = zbServerData.sGameData.LegionWarBaseInfo.LegionWarZhanLingInfo;
			zbWsMsgHandler.Instance.LegionWar_ZhanLingLevel_Req();
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11204"));
		}
	}

	private void OnModList()
	{
		if (zbSelf.Instance.legionWarHandler.GetLegionWarState() == ELegionWarState.Fighting)
		{
			zbSelf.Instance.legionWarHandler.OnModList();
		}
	}

	private void OnClose()
	{
		Hide();
	}
}
