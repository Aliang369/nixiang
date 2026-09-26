using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbEndlessHordeWnd : zbBaseWnd
{
	private SkeletonAnimation m_spineCar;

	private VisualElement m_nodeRecord;

	private Label m_lbHistory;

	private Label m_lbLast;

	private zbItem[] m_items = new zbItem[4];

	private Button m_btnMecha;

	public Button m_btnRule;

	private Button m_btnRank;

	private Button m_btnLeft;

	private Button m_btnRight;

	private Label m_lbZhanlingLv;

	private Button m_btnBuy;

	private ScrollView m_scrollView;

	private zbEndlessHordeItem m_grandPrize;

	private List<int> m_grandIndexes;

	private List<zbZhanLingEndlessCsvRecord> m_records;

	private int m_currentGrandIndex = -1;

	private Label m_lbCount;

	private Button m_btnAdd;

	private VisualElement m_spriteSweep;

	private Button m_btnSweep;

	private Label m_lbSweep;

	private Button m_btnFight;

	private Button m_btnClose;

	private VisualElement m_notifyMecha;

	private int m_curZaiju;

	private IVisualElementScheduledItem m_scrollSchedule;

	private const int SCROLL_OFFSET = 3;

	protected override void OnEnable()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineCar = ((Component)GetMyGameObjectSceneRoot().transform.Find("zhuangjiache")).GetComponent<SkeletonAnimation>();
		m_nodeRecord = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_record", (string)null);
		m_nodeRecord.style.width = StyleLength.op_Implicit((float)((zbLanguage.Instance.GetLanguageType() == ELanguageType.Fr || zbLanguage.Instance.GetLanguageType() == ELanguageType.Pt) ? 300 : 260));
		m_lbHistory = UQueryExtensions.Q<Label>(m_RootElement, "lb_history", (string)null);
		m_lbLast = UQueryExtensions.Q<Label>(m_RootElement, "lb_last", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(this);
		}
		m_btnMecha = UQueryExtensions.Q<Button>(m_RootElement, "btn_mecha", (string)null);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_btnRank = UQueryExtensions.Q<Button>(m_RootElement, "btn_rank", (string)null);
		m_btnLeft = UQueryExtensions.Q<Button>(m_RootElement, "btn_left", (string)null);
		m_btnRight = UQueryExtensions.Q<Button>(m_RootElement, "btn_right", (string)null);
		m_lbZhanlingLv = UQueryExtensions.Q<Label>(m_RootElement, "lb_zhanlingLv", (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>(m_RootElement, "btn_buy", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, "list", (string)null);
		m_scrollView.mode = (ScrollViewMode)1;
		((VisualElement)m_scrollView).contentContainer.style.flexDirection = StyleEnum<FlexDirection>.op_Implicit((FlexDirection)2);
		m_grandPrize = new zbEndlessHordeItem();
		m_grandPrize = UQueryExtensions.Q<zbEndlessHordeItem>(m_RootElement, "grandPrize", (string)null);
		m_grandPrize.OnEnable(this);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_btnFight = UQueryExtensions.Q<Button>(m_RootElement, "btn_fight", (string)null);
		m_spriteSweep = UQueryExtensions.Q<VisualElement>(m_RootElement, "sweep", (string)null);
		m_btnSweep = UQueryExtensions.Q<Button>(m_RootElement, "btn_sweep", (string)null);
		m_lbSweep = UQueryExtensions.Q<Label>(m_RootElement, "lb_sweep", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMecha, (Action)OnMecha, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRank, (Action)OnRank, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLeft, (Action<ClickEvent>)delegate
		{
			OnGotoNext(-1);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRight, (Action<ClickEvent>)delegate
		{
			OnGotoNext(1);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action)OnBuy, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAdd, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action)OnFight, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSweep, (Action)OnSweep, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_notifyMecha = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnMecha, (Vector2?)new Vector2(1f, 7f), 1f, true);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Tili
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_EndlessHorde, OnUpdateEvent);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Activity, OnUpdateEvent);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_MechaInfo, delegate
		{
			RefreshNotify();
		});
		InitRecord();
	}

	private void InitRecord()
	{
		m_records = zbDataConfig.Instance.zhanLingEndlessCsvTable.GetRecords().records.Values.ToList();
		m_grandIndexes = new List<int>();
		for (int i = 0; i < m_records.Count; i++)
		{
			if (m_records[i].isGrand > 0)
			{
				m_grandIndexes.Add(i);
			}
		}
	}

	public override void Refresh()
	{
		RefreshAwards();
		RefreshZaiju();
		RefreshZhanLing();
		RefreshPreGrandPrizeByScroll(needRefresh: true);
		RefreshFightNum();
		RefreshNotify();
		if (!zbGuide.Instance.GetGuideBool(EGuideStep.guide_step_EndlessHorde_2) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.EndlessHorde))
		{
			zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_EndlessHorde_2);
			zbGuide.Instance.ExecGuide(EGuideStep.guide_step_EndlessHorde_2);
		}
		zbWallCsvRecord recordByKey = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(zbServerData.sGameData.WallInfo.WallId);
		zbUtil.RefreshSpine(m_spineCar, recordByKey.carAnimation + "_SkeletonData.asset", "stand", loop: true);
	}

	private void RefreshAwards()
	{
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		var (zbEndlessHordeLevelCsvRecord2, zbEndlessHordeLevelCsvRecord3, num) = zbSelf.Instance.endlessHordeHandler.GetRecord(zbServerData.sGameData.EndlessHordeInfo.Level + 1);
		((TextElement)m_lbZhanlingLv).text = zbUtil.Language("lang.10792") + ((float)zbServerData.sGameData.EndlessHordeInfo.PassDistance / 1000f).ToString("0.#") + "km";
		((TextElement)m_lbHistory).text = zbUtil.Language("lang.10793") + ((float)zbServerData.sGameData.EndlessHordeInfo.PassDistance / 1000f).ToString("0.#") + "km";
		((TextElement)m_lbLast).text = zbUtil.Language("lang.10794") + ((float)zbSelf.Instance.endlessHordeHandler.GetDistance(zbServerData.sGameData.EndlessHordeInfo.Level) / 1000f).ToString("0.#") + "km";
		for (int i = 0; i < 4; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		zbEndlessHordeLevelCsvRecord obj = ((num > 0) ? zbEndlessHordeLevelCsvRecord3 : zbEndlessHordeLevelCsvRecord2);
		int num2 = 0;
		int[][] awardShow = obj.awardShow;
		foreach (int[] array in awardShow)
		{
			if (num2 < m_items.Length)
			{
				zbItemData data = new zbItemData((EResourceType)array[0], array[1], array[2]);
				((VisualElement)m_items[num2]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num2].Refresh(data);
			}
			num2++;
		}
	}

	private void RefreshZaiju()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnLeft).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnRight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private void RefreshZhanLing()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		IVisualElementScheduledItem scrollSchedule = m_scrollSchedule;
		if (scrollSchedule != null)
		{
			scrollSchedule.Pause();
		}
		m_scrollSchedule = null;
		ZhanLingCommonInfo zhanLingEndlessInfo = zbServerData.sGameData.RechargeActiveInfo.ZhanLingEndlessInfo;
		((VisualElement)m_btnBuy).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zhanLingEndlessInfo.Active == EBoolState.True));
		((TextElement)m_btnBuy).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(zhanLingEndlessInfo.GoodsId);
		zbZhanLingEndlessCsvTableData records = zbDataConfig.Instance.zhanLingEndlessCsvTable.GetRecords();
		((VisualElement)m_scrollView).Clear();
		VisualElement val = null;
		List<VisualElement> list = new List<VisualElement>();
		foreach (KeyValuePair<int, zbZhanLingEndlessCsvRecord> record in records.records)
		{
			zbEndlessHordeItem zbEndlessHordeItem2 = new zbEndlessHordeItem();
			zbEndlessHordeItem2.OnEnable(this);
			zbEndlessHordeItem2.Init(record.Value);
			((VisualElement)m_scrollView).Add((VisualElement)(object)zbEndlessHordeItem2);
			list.Add((VisualElement)(object)zbEndlessHordeItem2);
			if (canReceive(record.Value))
			{
				val = (VisualElement)(object)zbEndlessHordeItem2;
			}
		}
		if (val == null)
		{
			return;
		}
		int num = list.IndexOf(val) + 3;
		int num2 = list.Count - 1;
		num = Mathf.Clamp(num, 0, num2);
		VisualElement targetElementToScroll = list[num];
		if (targetElementToScroll != null)
		{
			m_scrollSchedule = m_RootElement.schedule.Execute((Action)delegate
			{
				m_scrollView.ScrollTo(targetElementToScroll);
			});
			m_scrollSchedule.ExecuteLater(32L);
		}
	}

	private bool canReceive(zbZhanLingEndlessCsvRecord record)
	{
		return record.distance <= zbServerData.sGameData.EndlessHordeInfo.PassDistance;
	}

	private void RefreshPreGrandPrizeByScroll(bool needRefresh = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.FloorToInt(m_scrollView.scrollOffset.x / 96f);
		int num2 = -1;
		for (int i = 0; i < m_grandIndexes.Count; i++)
		{
			if (m_grandIndexes[i] > num + 5)
			{
				num2 = i;
				break;
			}
		}
		if (num2 != -1 && ((num2 != m_currentGrandIndex) | needRefresh))
		{
			m_currentGrandIndex = num2;
			int index = m_grandIndexes[m_currentGrandIndex];
			zbZhanLingEndlessCsvRecord record = m_records[index];
			m_grandPrize.Init(record);
			m_grandPrize.SetMaskVisible(vis: false);
		}
	}

	private void RefreshFightNum()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbCount).text = zbUtil.Language("lang.10159") + ":" + (zbServerData.sGameData.EndlessHordeInfo.DailyCount - zbServerData.sGameData.EndlessHordeInfo.CombatCount) + "/" + zbServerData.sGameData.EndlessHordeInfo.DailyCount;
		m_spriteSweep.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbServerData.sGameData.EndlessHordeInfo.Level <= 0));
		((VisualElement)m_lbSweep).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbServerData.sGameData.EndlessHordeInfo.Level <= 0));
	}

	private void RefreshNotify()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		m_notifyMecha.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.mechaHandler.HasRedPoint()));
	}

	private void OnMecha()
	{
		zbMechaCsvRecord mechaRecord = zbSelf.Instance.mechaHandler.GetMechaRecord(102);
		if (zbSelf.Instance.GetMechaId() > 0)
		{
			mechaRecord = zbSelf.Instance.mechaHandler.GetMechaRecord(zbSelf.Instance.GetMechaId());
		}
		zbUIMgr.Instance.ShowMecha(mechaRecord);
	}

	public void OnRule()
	{
		string[] langKeys = new string[3] { "lang.10818", "lang.10819", "lang.10820" };
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	private void OnRank()
	{
		zbWsMsgHandler.Instance.Rank_List_Req(ERankType.EndlessHorde, openRank: true);
	}

	public void OnGotoNext(int index)
	{
		m_curZaiju += index;
		RefreshZaiju();
	}

	private void OnBuy()
	{
		if (zbServerData.sGameData.RechargeActiveInfo.ZhanLingEndlessInfo.Active == EBoolState.False)
		{
			zbChannel_Pay.OnPay(zbServerData.sGameData.RechargeActiveInfo.ZhanLingEndlessInfo.GoodsId);
		}
	}

	private void OnAdd()
	{
		zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.EndlessHorde);
	}

	private bool CanFighitOrSweep()
	{
		if (zbServerData.sGameData == null || zbServerData.sGameData.EndlessHordeInfo == null || zbSelf.Instance == null || zbSelf.Instance.vipHandler == null)
		{
			return false;
		}
		if (zbServerData.sGameData.EndlessHordeInfo.DailyCount <= zbServerData.sGameData.EndlessHordeInfo.CombatCount)
		{
			if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.EndlessHordeBuy, zbSelf.Instance.GetVipLevel()) - zbServerData.sGameData.EndlessHordeInfo.BuyCombat > 0)
			{
				zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.EndlessHorde);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10398"));
			}
			return false;
		}
		return true;
	}

	private void OnSweep()
	{
		if (CanFighitOrSweep())
		{
			zbWsMsgHandler.Instance.EndlessHorde_SaoDang_Req(1);
		}
	}

	public void OnFight()
	{
		if (CanFighitOrSweep())
		{
			zbWsMsgHandler.Instance.EndlessHorde_FightStart_Req(zbServerData.sGameData.EndlessHordeInfo.Level + 1);
		}
	}

	private void OnClose()
	{
		Hide();
		zbUIMgr.Instance.ShowChallenge(EFuncType.EndlessHorde);
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		RefreshPreGrandPrizeByScroll();
	}

	private void OnUpdateEvent(object data)
	{
		Refresh();
	}
}
