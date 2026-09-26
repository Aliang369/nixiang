using System;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbLegionBossWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnRank;

	private Button m_btnAward;

	private Button m_btnRecord;

	private Button m_btnRule;

	private Button m_btnSweep;

	private Button m_btnFight;

	private Button m_btnAdd;

	private Button m_btnPreview;

	private VisualElement m_progress;

	private VisualElement m_awardPreview;

	private VisualElement m_redDot;

	private VisualElement m_vsSweep;

	private Label m_lbTitle;

	private Label m_lbInfo;

	private Label m_lbEndTime;

	private Label m_lbCountVal;

	private zbSpineScene m_spineScene;

	private Transform m_jianzhu;

	private SkeletonAnimation m_iconCurrent;

	private SkeletonAnimation[] m_jianzhuList = (SkeletonAnimation[])(object)new SkeletonAnimation[7];

	private zbItem[] m_items = new zbItem[6];

	private int m_dailyBuyNum;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnRank = UQueryExtensions.Q<Button>(m_RootElement, "btn_rank", (string)null);
		m_btnAward = UQueryExtensions.Q<Button>(m_RootElement, "btn_award", (string)null);
		m_btnRecord = UQueryExtensions.Q<Button>(m_RootElement, "btn_record", (string)null);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_btnSweep = UQueryExtensions.Q<Button>(m_RootElement, "btn_sweep", (string)null);
		m_btnFight = UQueryExtensions.Q<Button>(m_RootElement, "btn_fight", (string)null);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_redDot = UQueryExtensions.Q<VisualElement>(m_RootElement, "red_dot", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbEndTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_endTime", (string)null);
		m_lbCountVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_countVal", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_awardPreview = UQueryExtensions.Q<VisualElement>(m_RootElement, "awardPreview", (string)null);
		m_vsSweep = UQueryExtensions.Q<VisualElement>(m_RootElement, "sweep", (string)null);
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		for (int i = 0; i < 6; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_awardPreview, "item" + i, (string)null);
			m_items[i].OnEnable(this);
		}
		m_jianzhu = m_GameObjectSceneRoot.transform.Find("jianzhu");
		for (int j = 0; j < 7; j++)
		{
			m_jianzhuList[j] = ((Component)m_jianzhu.Find("jianzhu" + j)).GetComponent<SkeletonAnimation>();
			((SkeletonRenderer)m_jianzhuList[j]).Initialize(true, false);
		}
		m_iconCurrent = ((Component)((Component)m_jianzhu).transform.Find("icon")).GetComponent<SkeletonAnimation>();
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRank, (Action)OnClickRank, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAward, (Action)OnClickAward, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRecord, (Action)OnClickRecord, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnClickRule, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSweep, (Action)OnSweep, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action)OnFight, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAddCount, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Legion_BossAward, OnUpdateEventLegionBoss);
	}

	public override void Refresh()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Init();
		m_vsSweep.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbServerData.sGameData.LegionSelfInfo.BossInfo.CombatCount <= 0));
		if (!zbGuide.Instance.GetGuideBool(EGuideStep.guide_step_LegionBoss_2) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.LegionBoss))
		{
			zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_LegionBoss_2);
			zbGuide.Instance.ExecGuide(EGuideStep.guide_step_LegionBoss_2);
		}
		m_dailyBuyNum = zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.LegionBossBuy, zbSelf.Instance.GetVipLevel());
	}

	private void OnClose()
	{
		Hide();
	}

	private void Init()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		LegionBossInfo legionBossInfo = zbSelf.Instance.legionHandler.legionInfo.LegionBossInfo;
		zbLegionBossChallengeCsvRecord recordByKey = zbDataConfig.Instance.legionBossChallengeCsvTable.GetRecordByKey(legionBossInfo.BossId);
		((TextElement)m_lbTitle).text = recordByKey.name;
		int monsterKill = recordByKey.monsterKill;
		int num = Math.Min(legionBossInfo.KillTotal, monsterKill);
		int num2 = monsterKill - num;
		float num3 = ((monsterKill > 0) ? ((float)num2 / (float)monsterKill * 100f) : 0f);
		UQueryExtensions.Q<VisualElement>(m_progress, "progressVal", (string)null).style.width = new StyleLength(new Length(num3, (LengthUnit)1));
		((TextElement)UQueryExtensions.Q<Label>(m_progress, "lb_val", (string)null)).text = ((num2 == 0) ? "已通关" : num2.ToString());
		for (int i = 0; i < m_items.Length; i++)
		{
			bool flag = i < recordByKey.awardShow.Length;
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				m_items[i].Refresh(new zbItemData((EResourceType)recordByKey.awardShow[i][0], recordByKey.awardShow[i][1], recordByKey.awardShow[i][2]), isNameVis: false, isCountVis: false);
			}
		}
		LegionSelfBoss bossInfo = zbServerData.sGameData.LegionSelfInfo.BossInfo;
		string text = $"{bossInfo.DailyCount - bossInfo.CombatCount + bossInfo.BuyCombat}/{bossInfo.DailyCount + bossInfo.BuyCombat}";
		((TextElement)m_lbCountVal).text = zbUtil.Language("lang.10159") + ":" + text;
		InitScreen();
	}

	protected override void UpdateAtShow()
	{
		((TextElement)m_lbEndTime).text = zbUtil.FormatTimeHHMMSS(zbSelf.Instance.legionHandler.legionInfo.LegionBossInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		m_redDot.visible = zbSelf.Instance.legionHandler.HasCallengeAwardRedPoint();
	}

	private void InitScreen()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		LegionBossInfo legionBossInfo = zbSelf.Instance.legionHandler.legionInfo.LegionBossInfo;
		zbLegionBossChallengeCsvRecord recordByKey = zbDataConfig.Instance.legionBossChallengeCsvTable.GetRecordByKey(legionBossInfo.BossId);
		for (int i = 0; i < 7; i++)
		{
			int num = i + 1;
			bool flag = num == recordByKey.bossid;
			string text = ((num < recordByKey.bossid) ? "yellow" : (flag ? "red" : "normal"));
			m_jianzhuList[i].AnimationState.SetAnimation(0, text, true);
			if (flag)
			{
				_ = m_jianzhu.position;
				switch (recordByKey.bossid)
				{
				case 1:
					((Component)m_iconCurrent).transform.localPosition = new Vector3(0.46f, -0.4f, 0f);
					break;
				case 2:
					((Component)m_iconCurrent).transform.localPosition = new Vector3(0.96f, 0.65f, 0f);
					break;
				case 3:
					((Component)m_iconCurrent).transform.localPosition = new Vector3(-0.841f, 1.265f, 0f);
					break;
				case 4:
					((Component)m_iconCurrent).transform.localPosition = new Vector3(1.53f, 2.361f, 0f);
					break;
				case 5:
					((Component)m_iconCurrent).transform.localPosition = new Vector3(0.127f, 2.079f, 0f);
					break;
				case 6:
					((Component)m_iconCurrent).transform.localPosition = new Vector3(1.76f, 4.29f, 0f);
					break;
				case 7:
					((Component)m_iconCurrent).transform.localPosition = new Vector3(0.33f, 5.43f, 0f);
					break;
				}
			}
		}
	}

	private void OnAddCount()
	{
		zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.LegionBoss);
	}

	private void OnClickRank()
	{
		zbWsMsgHandler.Instance.LegionBoss_RankList_Req();
	}

	private void OnClickAward()
	{
		zbUIMgr.Instance.ShowLegionBossAward();
	}

	private void OnClickRecord()
	{
		zbWsMsgHandler.Instance.LegionBoss_RecordList_Req();
	}

	private void OnClickRule()
	{
		string[] langKeys = new string[3] { "lang.10379", "lang.10380", "lang.10381" };
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	private void OnUpdateEventLegionBoss(object data)
	{
		((VisualElement)m_btnSweep).pickingMode = (PickingMode)0;
		Init();
	}

	private bool CheckCanFightOrSweep()
	{
		LegionSelfBoss bossInfo = zbServerData.sGameData.LegionSelfInfo.BossInfo;
		if (bossInfo.DailyCount - bossInfo.CombatCount + bossInfo.BuyCombat == 0)
		{
			return false;
		}
		return true;
	}

	private void CheckCanBuyCount()
	{
		if (zbServerData.sGameData.LegionSelfInfo.BossInfo.BuyCombat < m_dailyBuyNum)
		{
			zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.LegionBoss);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10398"));
		}
	}

	private void OnFight()
	{
		if (CheckCanFightOrSweep())
		{
			zbWsMsgHandler.Instance.LegionBoss_FightStart_Req(zbSelf.Instance.legionHandler.legionInfo.LegionBossInfo.BossId);
		}
		else
		{
			CheckCanBuyCount();
		}
	}

	private void OnSweep()
	{
		if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.LegionBossSweep, zbSelf.Instance.GetVipLevel()) > 0)
		{
			if (CheckCanFightOrSweep())
			{
				zbWsMsgHandler.Instance.LegionBoss_SaoDang_Req(zbSelf.Instance.legionHandler.legionInfo.LegionBossInfo.BossId);
			}
			else
			{
				CheckCanBuyCount();
			}
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11492"), zbSelf.Instance.vipHandler.CheckVipFuncUnlock(EVipFuncType.LegionBossSweep)));
		}
	}
}
