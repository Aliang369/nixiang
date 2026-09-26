using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbElementTrialWnd : zbBaseWnd
{
	private Label m_lbZhanlingLv;

	private Button m_btnBuy;

	private ScrollView m_scrollView;

	private Button m_btnBack;

	private Button m_btnBuyCount;

	private Label m_lbBuyCount;

	private ElementTrialInfo m_detail;

	private zbElementZhanLingItem m_grandPrize;

	private List<int> m_grandIndexes;

	private List<zbZhanlingElementCsvRecord> m_records;

	private int m_currentGrandIndex = -1;

	private const int SCROLL_OFFSET = 4;

	private IVisualElementScheduledItem m_scrollSchedule;

	private Button m_btnAuto;

	private VisualElement m_spriteOk;

	private VisualElement[] m_trials = (VisualElement[])(object)new VisualElement[2];

	private VisualElement[] m_bgs = (VisualElement[])(object)new VisualElement[2];

	private Label[] m_lbTrialNames = (Label[])(object)new Label[2];

	private Label[] m_lbLvs = (Label[])(object)new Label[2];

	private Button[] m_btnSweeps = (Button[])(object)new Button[2];

	private Button[] m_btnFights = (Button[])(object)new Button[2];

	private zbItem[] m_trial0AwardLists = new zbItem[3];

	private zbItem[] m_trial1AwardLists = new zbItem[3];

	private Sprite spriteGray;

	private Sprite spriteYellow;

	private Sprite spriteGreen;

	private Button m_btnRefresh;

	private Label m_lbRefresh;

	private readonly int m_refreshInterval = 30;

	private long m_nextRefreshTm;

	protected override void OnEnable()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_lbZhanlingLv = UQueryExtensions.Q<Label>(m_RootElement, "lb_zhanlingLv", (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>(m_RootElement, "btn_buy", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, "list", (string)null);
		m_scrollView.mode = (ScrollViewMode)1;
		((VisualElement)m_scrollView).contentContainer.style.flexDirection = StyleEnum<FlexDirection>.op_Implicit((FlexDirection)2);
		m_grandPrize = new zbElementZhanLingItem();
		m_grandPrize = UQueryExtensions.Q<zbElementZhanLingItem>(m_RootElement, "grandPrize", (string)null);
		m_grandPrize.OnEnable(this);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnBuyCount = UQueryExtensions.Q<Button>(m_RootElement, "btn_buyCount", (string)null);
		m_lbBuyCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_buyCount", (string)null);
		m_btnAuto = UQueryExtensions.Q<Button>(m_RootElement, "btn_auto", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_ok", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAuto, (Action)OnAuto, "", (TrickleDown)0);
		for (int i = 0; i < 2; i++)
		{
			int index = i;
			m_trials[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "trial" + i, (string)null);
			m_lbTrialNames[i] = UQueryExtensions.Q<Label>(m_trials[i], "lb_trialName", (string)null);
			m_lbLvs[i] = UQueryExtensions.Q<Label>(m_trials[i], "lb_lv", (string)null);
			m_btnSweeps[i] = UQueryExtensions.Q<Button>(m_trials[i], "btn_sweep", (string)null);
			m_btnFights[i] = UQueryExtensions.Q<Button>(m_trials[i], "btn_fight", (string)null);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFights[i], (Action)delegate
			{
				OnFight(index);
			}, "", (TrickleDown)0);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSweeps[i], (Action)delegate
			{
				OnSweep(index);
			}, "", (TrickleDown)0);
			m_bgs[i] = UQueryExtensions.Q<VisualElement>(m_trials[i], "bg", (string)null);
		}
		m_btnRefresh = UQueryExtensions.Q<Button>(m_RootElement, "btn_refresh", (string)null);
		m_lbRefresh = UQueryExtensions.Q<Label>(m_RootElement, "lb_refresh", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRefresh, (Action)OnRefresh, "", (TrickleDown)0);
		spriteGray = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_gray.png");
		spriteYellow = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_yellow.png");
		spriteGreen = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_green.png");
		for (int num = 0; num < 3; num++)
		{
			m_trial0AwardLists[num] = UQueryExtensions.Q<zbItem>(m_trials[0], "award" + num, (string)null);
			m_trial0AwardLists[num].OnEnable(this);
			m_trial1AwardLists[num] = UQueryExtensions.Q<zbItem>(m_trials[1], "award" + num, (string)null);
			m_trial1AwardLists[num].OnEnable(this);
		}
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
		m_records = zbDataConfig.Instance.zhanlingElementCsvTable.GetRecords().records.Values.ToList();
		m_grandIndexes = new List<int>();
		for (int num2 = 0; num2 < m_records.Count; num2++)
		{
			if (m_records[num2].isGrand > 0)
			{
				m_grandIndexes.Add(num2);
			}
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action)OnBuy, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuyCount, (Action)OnBuyCount, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_ElementTrial_Update, OnUpdateElementTrial);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Activity, OnUpdateEventActivityUpdate);
	}

	public void RefreshCustom()
	{
		m_detail = zbServerData.sGameData.ElementTrialInfo;
		if (m_detail.TrialList != null && m_detail.TrialList.Count == 0)
		{
			zbWsMsgHandler.Instance.ElementTrial_Data_Req();
			return;
		}
		InitZhanLing();
		RefreshAuto();
		InitGrandPrize();
		InitBuyCount();
		RefreshTrials();
		RefreshTime();
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_ElementTrial);
	}

	private void RefreshTrials()
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 2; i++)
		{
			zbElementTrialCsvRecord recordByKey = zbDataConfig.Instance.elementTrialCsvTable.GetRecordByKey(m_detail.TrialList[i].TrialType);
			TrialDetail trialInfoByAttackType = zbSelf.Instance.elementTrialHandler.GetTrialInfoByAttackType((ESkillAttackType)recordByKey.trialType);
			((TextElement)m_lbTrialNames[i]).text = zbUtil.Language(recordByKey.name);
			((TextElement)m_lbLvs[i]).text = trialInfoByAttackType.HistoryLevel.ToString();
			m_bgs[i].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.bg));
			((VisualElement)m_btnSweeps[i]).style.backgroundImage = new StyleBackground((trialInfoByAttackType.Level > 0) ? spriteGreen : spriteGray);
			((VisualElement)m_btnFights[i]).style.backgroundImage = new StyleBackground((!zbSelf.Instance.elementTrialHandler.CheckElementTrialIsUnLock((ESkillAttackType)recordByKey.trialType) || trialInfoByAttackType.Level >= zbSelf.Instance.elementTrialHandler.GetMaxLevel() || !zbSelf.Instance.elementTrialHandler.CheckCanFight(trialInfoByAttackType.Level + 1)) ? spriteGray : spriteYellow);
			for (int j = 0; j < 3; j++)
			{
				switch (i)
				{
				case 0:
				{
					bool flag2 = j < recordByKey.awardShow.Length;
					((VisualElement)m_trial0AwardLists[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
					if (flag2)
					{
						zbItemData data2 = new zbItemData((EResourceType)recordByKey.awardShow[j][0], recordByKey.awardShow[j][1], recordByKey.awardShow[j][2] + recordByKey.awardShow[j][3] * trialInfoByAttackType.Level);
						m_trial0AwardLists[j].Refresh(data2);
					}
					break;
				}
				case 1:
				{
					bool flag = j < recordByKey.awardShow.Length;
					((VisualElement)m_trial1AwardLists[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
					if (flag)
					{
						zbItemData data = new zbItemData((EResourceType)recordByKey.awardShow[j][0], recordByKey.awardShow[j][1], recordByKey.awardShow[j][2] + recordByKey.awardShow[j][3] * trialInfoByAttackType.Level);
						m_trial1AwardLists[j].Refresh(data);
					}
					break;
				}
				}
			}
		}
	}

	private void InitGrandPrize()
	{
		UpdatePreGrandPrizeByScroll(needRefresh: true);
	}

	private void UpdatePreGrandPrizeByScroll(bool needRefresh = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.FloorToInt(m_scrollView.scrollOffset.x / 96f);
		int num2 = -1;
		for (int i = 0; i < m_grandIndexes.Count; i++)
		{
			if (m_grandIndexes[i] >= num + 5)
			{
				num2 = i;
				break;
			}
		}
		if ((num2 != -1 && num2 != m_currentGrandIndex) | needRefresh)
		{
			m_currentGrandIndex = num2;
			int index = m_grandIndexes[m_currentGrandIndex];
			zbZhanlingElementCsvRecord record = m_records[index];
			m_grandPrize.Init(record);
			m_grandPrize.SetMaskVisible(vis: false);
		}
	}

	private void RefreshAuto()
	{
		zbSelf.Instance.combatHandler.SetAutoOKDisplay(EBattleFuncType.ElementTrialElec, m_spriteOk);
	}

	private void OnAuto()
	{
		if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.AutoFight, zbSelf.Instance.GetVipLevel()) <= 0)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11492"), zbSelf.Instance.vipHandler.CheckVipFuncUnlock(EVipFuncType.AutoFight)) + "【" + zbUtil.Language("lang.11494") + "】");
			return;
		}
		zbSelf.Instance.combatHandler.SetAutoSelectMod(EBattleFuncType.ElementTrialElec);
		RefreshAuto();
	}

	private bool CheckCanUpgrade(int index)
	{
		zbElementTrialCsvRecord recordByKey = zbDataConfig.Instance.elementTrialCsvTable.GetRecordByKey(m_detail.TrialList[index].TrialType);
		int key = zbSelf.Instance.elementTrialHandler.GetTrialInfoByAttackType((ESkillAttackType)recordByKey.trialType).Level + 1;
		zbElementTrialLevelCsvRecord recordByKey2 = zbDataConfig.Instance.elementTrialLevelCsvTable.GetRecordByKey(key);
		if (recordByKey2 == null)
		{
			return false;
		}
		return zbServerData.sGameData.MainQuestInfo.MainLevel >= recordByKey2.mainQuestOpen;
	}

	private void OnFight(int i)
	{
		zbElementTrialCsvRecord recordByKey = zbDataConfig.Instance.elementTrialCsvTable.GetRecordByKey(m_detail.TrialList[i].TrialType);
		TrialDetail trialInfoByAttackType = zbSelf.Instance.elementTrialHandler.GetTrialInfoByAttackType((ESkillAttackType)recordByKey.trialType);
		if (!zbSelf.Instance.elementTrialHandler.CheckElementTrialIsUnLock((ESkillAttackType)recordByKey.trialType))
		{
			switch ((ESkillAttackType)recordByKey.trialType)
			{
			case ESkillAttackType.Ice:
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10753"));
				break;
			case ESkillAttackType.Fire:
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10748"));
				break;
			case ESkillAttackType.Phy:
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10751"));
				break;
			case ESkillAttackType.Energy:
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10752"));
				break;
			case ESkillAttackType.Elec:
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10750"));
				break;
			case ESkillAttackType.Wind:
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10749"));
				break;
			}
		}
		else if (CheckCanUpgrade(i))
		{
			zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.11236"), zbUtil.Language("lang.11237"), OnUpgrade);
		}
		else if (trialInfoByAttackType.Level == zbDataConfig.Instance.elementTrialLevelCsvTable.GetRecords().records.Count)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11238"));
		}
		else if (!zbSelf.Instance.elementTrialHandler.CheckCanFight(trialInfoByAttackType.Level + 1))
		{
			zbElementTrialLevelCsvRecord recordByKey2 = zbDataConfig.Instance.elementTrialLevelCsvTable.GetRecordByKey(trialInfoByAttackType.Level + 1);
			if (recordByKey2 != null)
			{
				zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10754"), recordByKey2.unlockTotal));
			}
		}
		else if (zbSelf.Instance.elementTrialHandler.CanElementTrialCombat() && zbSelf.Instance.elementTrialHandler.CheckCanFight(trialInfoByAttackType.Level + 1))
		{
			zbWsMsgHandler.Instance.ElementTrial_FightStart_Req((ESkillAttackType)recordByKey.trialType, trialInfoByAttackType.Level + 1);
		}
		else if (zbSelf.Instance.elementTrialHandler.CanElementTrialBuyCount())
		{
			zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.ElementTrialPhy);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10700"));
		}
	}

	private void OnUpgrade(bool isOk)
	{
		if (isOk)
		{
			zbWsMsgHandler.Instance.ElementTrial_Upgrade_Req();
		}
	}

	private void OnSweep(int i)
	{
		zbElementTrialCsvRecord recordByKey = zbDataConfig.Instance.elementTrialCsvTable.GetRecordByKey(m_detail.TrialList[i].TrialType);
		if (zbSelf.Instance.elementTrialHandler.GetTrialInfoByAttackType((ESkillAttackType)recordByKey.trialType).Level <= 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10756"));
		}
		else if (CheckCanUpgrade(i))
		{
			zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.11236"), zbUtil.Language("lang.11237"), OnUpgrade);
		}
		else if (zbSelf.Instance.elementTrialHandler.CanElementTrialCombat())
		{
			zbWsMsgHandler.Instance.ElementTrial_SaoDang_Req((ESkillAttackType)recordByKey.trialType);
		}
		else if (zbSelf.Instance.elementTrialHandler.CanElementTrialBuyCount())
		{
			zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.ElementTrialPhy);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10700"));
		}
	}

	private void InitBuyCount()
	{
		int num = m_detail.DailyCount - m_detail.CombatCount;
		((TextElement)m_lbBuyCount).text = zbUtil.Language("lang.10466") + num;
	}

	private void OnRefresh()
	{
		if (m_nextRefreshTm <= zbSelf.Instance.timeHandler.GetServerTime())
		{
			m_nextRefreshTm = zbSelf.Instance.timeHandler.GetServerTime() + m_refreshInterval;
			zbWsMsgHandler.Instance.ElementTrial_Refresh_Req();
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

	private void InitZhanLing()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		IVisualElementScheduledItem scrollSchedule = m_scrollSchedule;
		if (scrollSchedule != null)
		{
			scrollSchedule.Pause();
		}
		m_scrollSchedule = null;
		ZhanLingCommonInfo zhanLingElementInfo = zbServerData.sGameData.RechargeActiveInfo.ZhanLingElementInfo;
		((VisualElement)m_btnBuy).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zhanLingElementInfo.Active == EBoolState.True));
		((TextElement)m_btnBuy).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(zhanLingElementInfo.GoodsId);
		((TextElement)m_lbZhanlingLv).text = string.Format(zbUtil.Language("lang.10734"), zbServerData.sGameData.RechargeActiveInfo.ZhanLingElementInfo.Reach);
		zbZhanlingElementCsvTableData records = zbDataConfig.Instance.zhanlingElementCsvTable.GetRecords();
		((VisualElement)m_scrollView).Clear();
		VisualElement val = null;
		List<VisualElement> list = new List<VisualElement>();
		foreach (KeyValuePair<int, zbZhanlingElementCsvRecord> record in records.records)
		{
			zbElementZhanLingItem zbElementZhanLingItem2 = new zbElementZhanLingItem();
			zbElementZhanLingItem2.OnEnable(this);
			zbElementZhanLingItem2.Init(record.Value);
			((VisualElement)m_scrollView).Add((VisualElement)(object)zbElementZhanLingItem2);
			list.Add((VisualElement)(object)zbElementZhanLingItem2);
			if (val == null && canReceive(record.Value))
			{
				val = (VisualElement)(object)zbElementZhanLingItem2;
			}
		}
		if (val == null)
		{
			return;
		}
		int num = list.IndexOf(val) + 4;
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

	private bool canReceive(zbZhanlingElementCsvRecord record)
	{
		if (record.trialLevel <= zbSelf.Instance.elementTrialHandler.GetElementZhanlingTotalLevel())
		{
			return zbServerData.sGameData.RechargeActiveInfo.ZhanLingElementInfo.LowAward < record.trialLevel;
		}
		return false;
	}

	private void OnBuy()
	{
		if (zbServerData.sGameData.RechargeActiveInfo.ZhanLingElementInfo.Active == EBoolState.False)
		{
			zbChannel_Pay.OnPay(zbServerData.sGameData.RechargeActiveInfo.ZhanLingElementInfo.GoodsId);
		}
	}

	private void OnBuyCount()
	{
		if (zbSelf.Instance.elementTrialHandler.CanElementTrialBuyCount())
		{
			zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.ElementTrialPhy);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10398"));
		}
	}

	private void OnUpdateElementTrial(object data)
	{
		m_detail = zbServerData.sGameData.ElementTrialInfo;
		if (m_detail.TrialList == null || m_detail.TrialList.Count != 0)
		{
			RefreshCustom();
		}
	}

	private void OnUpdateEventActivityUpdate(object data)
	{
		m_detail = zbServerData.sGameData.ElementTrialInfo;
		if (m_detail.TrialList == null || m_detail.TrialList.Count != 0)
		{
			RefreshCustom();
		}
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		UpdatePreGrandPrizeByScroll();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}
}
