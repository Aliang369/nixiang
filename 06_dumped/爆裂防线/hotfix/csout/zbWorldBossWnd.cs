using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbWorldBossWnd : zbBaseWnd
{
	private Button m_btnBack;

	private Button m_btnRank;

	private Button m_btnSweep;

	private Button m_btnLog;

	private Button m_btnRule;

	private Button m_btnFight;

	private Button m_btnAdd;

	private VisualElement m_progress;

	private VisualElement m_awardList;

	private VisualElement m_preBoss;

	private VisualElement m_currentBoss;

	private VisualElement m_nextBoss;

	private VisualElement m_vsSweep;

	private VisualElement m_time;

	private zbItem[] m_awardItems = new zbItem[6];

	private Label m_lbTitle;

	private Label m_lbDesc;

	private Label m_lbEndTime;

	private Label m_lbCountVal;

	private Label m_lbProgressTitle;

	private JToken m_worldBossBase;

	private zbMonsterPreview m_boss;

	private int m_dailyBuyNum;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnRank = UQueryExtensions.Q<Button>(m_RootElement, "btn_rank", (string)null);
		m_btnSweep = UQueryExtensions.Q<Button>(m_RootElement, "btn_sweep", (string)null);
		m_btnLog = UQueryExtensions.Q<Button>(m_RootElement, "btn_log", (string)null);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_btnFight = UQueryExtensions.Q<Button>(m_RootElement, "btn_fight", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_awardList = UQueryExtensions.Q<VisualElement>(m_RootElement, "awardList", (string)null);
		m_preBoss = UQueryExtensions.Q<VisualElement>(m_RootElement, "preBoss", (string)null);
		m_currentBoss = UQueryExtensions.Q<VisualElement>(m_RootElement, "currentBoss", (string)null);
		m_nextBoss = UQueryExtensions.Q<VisualElement>(m_RootElement, "nextBoss", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_lbProgressTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_progressTitle", (string)null);
		m_lbEndTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_endTime", (string)null);
		m_lbCountVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_countVal", (string)null);
		((TextElement)m_lbDesc).text = zbUtil.Language("lang.10869");
		m_boss = ((Component)m_GameObjectSceneRoot.transform.Find("monsterAvatar").Find("monsterPreview1")).GetComponent<zbMonsterPreview>();
		m_worldBossBase = (JToken)(object)zbDataConfig.Instance.worldBossBaseJsonTable.GetRecord();
		m_vsSweep = UQueryExtensions.Q<VisualElement>(m_RootElement, "sweep", (string)null);
		m_time = UQueryExtensions.Q<VisualElement>(m_RootElement, "time", (string)null);
		for (int i = 0; i < 6; i++)
		{
			m_awardItems[i] = UQueryExtensions.Q<zbItem>(m_awardList, "item" + i, (string)null);
			m_awardItems[i].OnEnable(this);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRank, (Action)OnToRank, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSweep, (Action)OnSweep, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLog, (Action)OnToLog, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action)OnFight, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAddCount, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_WorldBoss_Data, OnUpdateEventWorldBoss);
	}

	public override void Refresh()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Init();
		m_vsSweep.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbServerData.sGameData.WorldBossSelfInfo.CombatCount <= 0));
		if (!zbGuide.Instance.GetGuideBool(EGuideStep.guide_step_WorldBoss_2) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.WorldBoss))
		{
			zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_WorldBoss_2);
			zbGuide.Instance.ExecGuide(EGuideStep.guide_step_WorldBoss_2);
		}
		m_dailyBuyNum = zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.WorldBossBuy, zbSelf.Instance.GetVipLevel());
	}

	private void Init()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		WorldBossInfo worldBossInfo = zbSelf.Instance.worldBossHandler.worldBossInfo;
		zbWorldBossCsvRecord recordByKey = zbDataConfig.Instance.worldBossCsvTable.GetRecordByKey(worldBossInfo.BossId);
		WorldBossSelfInfo worldBossSelfInfo = zbServerData.sGameData.WorldBossSelfInfo;
		bool flag = worldBossInfo.PassState == EBoolState.True;
		((TextElement)UQueryExtensions.Q<Label>(m_progress, "lb_val", (string)null)).text = zbUtil.ConvertToTBMK(worldBossInfo.TotalHurt) + " / " + zbUtil.ConvertToTBMK(worldBossInfo.PassHurt, isFloor: false);
		UQueryExtensions.Q<VisualElement>(m_progress, "progressVal", (string)null).style.width = new StyleLength(new Length(flag ? 100f : ((float)worldBossInfo.TotalHurt / (float)worldBossInfo.PassHurt * 100f), (LengthUnit)1));
		for (int i = 0; i < 6; i++)
		{
			if (i < recordByKey.awardShow.Length)
			{
				int[] array = recordByKey.awardShow[i];
				if (array != null)
				{
					zbItemData data = new zbItemData((EResourceType)array[0], array[1], array[2]);
					m_awardItems[i].Refresh(data);
					((VisualElement)m_awardItems[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				}
			}
			else
			{
				((VisualElement)m_awardItems[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
		bool flag2 = recordByKey.bossid - 1 == 0;
		bool flag3 = recordByKey.bossid == zbDataConfig.Instance.worldBossCsvTable.GetRecords().records.ToArray().Length;
		m_preBoss.visible = !flag2;
		m_nextBoss.visible = !flag3;
		UQueryExtensions.Q<VisualElement>(m_nextBoss, "icon_boss", (string)null).visible = !flag3;
		m_time.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag3 ? 1 : 0));
		SetBossIcon(m_currentBoss, recordByKey.icon);
		if (!flag2)
		{
			zbWorldBossCsvRecord recordByKey2 = zbDataConfig.Instance.worldBossCsvTable.GetRecordByKey(recordByKey.bossid - 1);
			SetBossIcon(m_preBoss, recordByKey2.icon);
		}
		if (!flag3)
		{
			zbWorldBossCsvRecord recordByKey3 = zbDataConfig.Instance.worldBossCsvTable.GetRecordByKey(recordByKey.bossid + 1);
			SetBossIcon(m_nextBoss, recordByKey3.icon);
			long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
			long num = ((worldBossInfo.PassState != EBoolState.True) ? zbUtil.GetTimestampOfDayInCurrentMonthByServerTime(serverTime, recordByKey3.openMaxDay) : zbUtil.GetTimestampOfDayInCurrentMonthByServerTime(serverTime, recordByKey3.openMinDay));
			((TextElement)UQueryExtensions.Q<Label>(m_time, "lb_time", (string)null)).text = $"{(num - serverTime) / 3600}H";
		}
		m_boss.Refresh(recordByKey.boss);
		((TextElement)m_lbCountVal).text = zbUtil.Language("lang.10159") + ":" + $"{worldBossSelfInfo.DailyCount - worldBossSelfInfo.CombatCount}/{worldBossSelfInfo.DailyCount}";
	}

	private void SetBossIcon(VisualElement root, string iconPath)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(iconPath);
		UQueryExtensions.Q<VisualElement>(root, "icon_boss", (string)null).style.backgroundImage = new StyleBackground(val);
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnAddCount()
	{
		zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.WorldBoss);
	}

	private void OnUpdateEventWorldBoss(object data)
	{
		Refresh();
	}

	private bool CheckCanFightOrSweep()
	{
		if (zbServerData.sGameData.WorldBossSelfInfo.DailyCount - zbServerData.sGameData.WorldBossSelfInfo.CombatCount == 0)
		{
			return false;
		}
		return true;
	}

	private bool CheckWorlBossDurationTime()
	{
		if (zbServerData.sGameData.WorldBossSelfInfo.StartTm < zbSelf.Instance.timeHandler.GetServerTime() && zbServerData.sGameData.WorldBossSelfInfo.EndTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			return true;
		}
		zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10306"));
		return false;
	}

	private void OnSweep()
	{
		if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.WorldBossSweep, zbSelf.Instance.GetVipLevel()) > 0)
		{
			if (CheckWorlBossDurationTime())
			{
				if (CheckCanFightOrSweep())
				{
					int uUID = zbSelf.Instance.worldBossHandler.worldBossInfo.UUID;
					zbWsMsgHandler.Instance.WorldBoss_SaoDang_Req(uUID);
				}
				else if (m_dailyBuyNum - zbServerData.sGameData.WorldBossSelfInfo.BuyCombat > 0)
				{
					zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.WorldBoss);
				}
			}
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11492"), zbSelf.Instance.vipHandler.CheckVipFuncUnlock(EVipFuncType.WorldBossSweep)));
		}
	}

	private void OnFight()
	{
		if (CheckWorlBossDurationTime())
		{
			if (CheckCanFightOrSweep())
			{
				zbWsMsgHandler.Instance.WorldBoss_FightStart_Req(zbSelf.Instance.worldBossHandler.worldBossInfo.UUID);
			}
			else if (m_dailyBuyNum - zbServerData.sGameData.WorldBossSelfInfo.BuyCombat > 0)
			{
				zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.WorldBoss);
			}
		}
	}

	private void OnToRank()
	{
		int uUID = zbSelf.Instance.worldBossHandler.worldBossInfo.UUID;
		zbWsMsgHandler.Instance.WorldBoss_RankList_Req(uUID);
	}

	private void OnToLog()
	{
		zbWsMsgHandler.Instance.WorldBoss_RecordList_Req();
	}

	private void OnRule()
	{
		string[] langKeys = new string[5] { "lang.10382", "lang.10383", "lang.10384", "lang.10385", "lang.10386" };
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	protected override void UpdateAtShow()
	{
		if (zbServerData.sGameData.WorldBossSelfInfo.StartTm < zbSelf.Instance.timeHandler.GetServerTime() && zbServerData.sGameData.WorldBossSelfInfo.EndTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			((TextElement)m_lbEndTime).text = zbUtil.FormatTimeHHMMSS(zbServerData.sGameData.WorldBossSelfInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
		else
		{
			((VisualElement)m_lbEndTime).visible = false;
		}
	}
}
