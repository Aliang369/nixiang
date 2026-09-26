using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCrossLadderWnd : zbBaseWnd
{
	private Label m_lbTitle;

	private Label m_lbTime;

	private VisualElement m_nodeMatch;

	private zbLadderItem m_itemLadder;

	private Button m_btnLadder;

	private Label m_lbDesc;

	private VisualElement m_spriteVictory;

	private VisualElement[] m_spriteVictoryCounts = (VisualElement[])(object)new VisualElement[3];

	private Button m_btnVictoryCount;

	private Button m_btnFight;

	private Label m_lbCount;

	private Button m_btnCrossPeak;

	private Label m_lbCrossPeak;

	private Label m_lbOpendCrossPeak;

	private Label m_lbScore;

	private ScrollView m_scrollView;

	private zbCrossLadderItem m_grandPrize;

	private List<int> m_grandIndexes;

	private List<zbLadderTargetAwardCsvRecord> m_records;

	private int m_currentGrandIndex = -1;

	private VisualElement m_nodeUpgrade;

	private zbLadderItem m_curLadder;

	private zbLadderItem m_nextLadder;

	private VisualElement[] m_spriteResults = (VisualElement[])(object)new VisualElement[5];

	private Button m_btnUpgradeFight;

	private Button m_btnRecord;

	public Button m_btnSettleAward;

	public Button m_btnRule;

	private Button m_btnClose;

	public Button m_btnRank;

	private VisualElement m_nodeTip;

	private Label m_lbTip;

	private IVisualElementScheduledItem m_hideSchedule;

	private IVisualElementScheduledItem m_scrollSchedule;

	private const int SCROLL_OFFSET = 3;

	private zbLadderCsvRecord m_record;

	private Button m_btnCard;

	private VisualElement m_cardTime;

	private Label m_lbCardTime;

	private SpineVisualElement m_spineCard;

	private bool m_isCardTimeRed;

	private VisualElement m_notifyCard;

	private VisualElement m_notifyRecord;

	private bool openCrossPeak = true;

	protected override void OnEnable()
	{
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_nodeMatch = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_match", (string)null);
		m_itemLadder = UQueryExtensions.Q<zbLadderItem>(m_nodeMatch, "ladder", (string)null);
		m_itemLadder.OnEnable(this);
		m_btnLadder = UQueryExtensions.Q<Button>(m_nodeMatch, "btn_ladder", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_nodeMatch, "lb_desc", (string)null);
		m_spriteVictory = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_victoryCount", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_spriteVictoryCounts[i] = UQueryExtensions.Q<VisualElement>(m_nodeMatch, "bg_victoryCount" + i, (string)null);
		}
		m_btnVictoryCount = UQueryExtensions.Q<Button>(m_nodeMatch, "btn_victoryCount", (string)null);
		m_btnFight = UQueryExtensions.Q<Button>(m_nodeMatch, "btn_fight", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_nodeMatch, "lb_count", (string)null);
		m_btnCrossPeak = UQueryExtensions.Q<Button>(m_nodeMatch, "btn_crossPeak", (string)null);
		m_lbCrossPeak = UQueryExtensions.Q<Label>(m_nodeMatch, "lb_crossPeak", (string)null);
		m_lbOpendCrossPeak = UQueryExtensions.Q<Label>(m_nodeMatch, "lb_dianfengsai", (string)null);
		m_lbScore = UQueryExtensions.Q<Label>(m_RootElement, "lb_score", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, "list", (string)null);
		m_scrollView.mode = (ScrollViewMode)1;
		((VisualElement)m_scrollView).contentContainer.style.flexDirection = StyleEnum<FlexDirection>.op_Implicit((FlexDirection)2);
		m_grandPrize = new zbCrossLadderItem();
		m_grandPrize = UQueryExtensions.Q<zbCrossLadderItem>(m_RootElement, "grandPrize", (string)null);
		m_grandPrize.OnEnable(this);
		m_nodeUpgrade = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_upgrade", (string)null);
		m_curLadder = UQueryExtensions.Q<zbLadderItem>(m_nodeUpgrade, "curLadder", (string)null);
		m_curLadder.OnEnable(this);
		m_nextLadder = UQueryExtensions.Q<zbLadderItem>(m_nodeUpgrade, "nextLadder", (string)null);
		m_nextLadder.OnEnable(this);
		for (int j = 0; j < 5; j++)
		{
			m_spriteResults[j] = UQueryExtensions.Q<VisualElement>(m_nodeUpgrade, "bg_result" + j, (string)null);
		}
		m_btnUpgradeFight = UQueryExtensions.Q<Button>(m_nodeUpgrade, "btn_upgradeFight", (string)null);
		m_btnRecord = UQueryExtensions.Q<Button>(m_RootElement, "btn_record", (string)null);
		m_btnSettleAward = UQueryExtensions.Q<Button>(m_RootElement, "btn_settleAward", (string)null);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnRank = UQueryExtensions.Q<Button>(m_RootElement, "btn_rank", (string)null);
		m_btnCard = UQueryExtensions.Q<Button>(m_RootElement, "btn_card", (string)null);
		m_cardTime = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnCard, "time", (string)null);
		m_lbCardTime = UQueryExtensions.Q<Label>(m_cardTime, "lb_cardTime", (string)null);
		m_spineCard = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)m_btnCard, "spine_baofenka", (string)null);
		m_notifyCard = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnCard, (Vector2?)new Vector2(-5f, 0f), 1f, true);
		m_notifyRecord = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnRecord, (Vector2?)new Vector2(1f, 7f), 1f, true);
		m_nodeTip = UQueryExtensions.Q<VisualElement>(m_RootElement, "tip", (string)null);
		m_lbTip = UQueryExtensions.Q<Label>(m_nodeTip, "lb_tip", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_lbTitle, (Action)OnTitle, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLadder, (Action)OnSettleAward, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnVictoryCount, (Action)OnVictoryCount, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action)OnFight, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgradeFight, (Action)OnUpgradeFight, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRecord, (Action)OnRecord, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSettleAward, (Action)OnSettleAward, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCard, (Action)OnCard, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCrossPeak, (Action)OnCrossPeak, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRank, (Action)OnRank, "", (TrickleDown)0);
		zbLanguage.Instance.SetButtonTextVisible((Button[])(object)new Button[4] { m_btnRecord, m_btnSettleAward, m_btnRule, m_btnSettleAward });
		InitTargetAwardRecords();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_CrossLadderInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_CrossPeakInfo, OnOpenPeakCloseSelf);
	}

	private void InitTargetAwardRecords()
	{
		m_records = zbDataConfig.Instance.ladderTargetAwardCsvTable.GetRecords().records.Values.ToList();
		m_grandIndexes = new List<int>();
		for (int i = 0; i < m_records.Count; i++)
		{
			if (m_records[i].reward > 0)
			{
				m_grandIndexes.Add(i);
			}
		}
	}

	public override void Refresh()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		if (!zbGuide.Instance.GetGuideBool(EGuideStep.guide_step_CrossLadder_2) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.CrossLadder))
		{
			zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_CrossLadder_2);
			zbGuide.Instance.ExecGuide(EGuideStep.guide_step_CrossLadder_2);
		}
		m_nodeTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_record = zbDataConfig.Instance.ladderCsvTable.GetRecordByKey(zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId);
		((TextElement)m_lbTitle).text = string.Format(zbUtil.Language("lang.10899"), zbServerData.sGameData.CrossLadderInfo.OpenInfo.GroupId);
		if (zbServerData.sGameData.CrossLadderInfo.CombatInfo.State == ELadderState.LadderMatch || (openCrossPeak && zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId >= 26))
		{
			m_nodeMatch.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_nodeUpgrade.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			RefreshMatch();
			RefreshTargetAwards();
			RefreshPreGrandPrizeByScroll(needRefresh: true);
		}
		else
		{
			m_nodeMatch.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_nodeUpgrade.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			RefreshUpgrade();
		}
		RefreshCrossLadderCard();
		RefreshNotify();
		RefreshTime();
		AutoFontSize();
		if (zbSelf.Instance.crossLadderHandler.crossLadderFriendRankList == null || zbSelf.Instance.crossLadderHandler.crossLadderFriendRankList.Length == 0)
		{
			zbWsMsgHandler.Instance.CrossLadder_FriendList_Req();
		}
	}

	private void AutoFontSize()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		float num = 28f;
		((VisualElement)m_btnUpgradeFight).style.fontSize = StyleLength.op_Implicit(num);
		num = ((((TextElement)m_btnUpgradeFight).MeasureTextSize(((TextElement)m_btnUpgradeFight).text, 9999f, (MeasureMode)0, 0f, (MeasureMode)0).x > 330f) ? 22f : 28f);
		((VisualElement)m_btnUpgradeFight).style.fontSize = StyleLength.op_Implicit(num);
	}

	private void RefreshCrossLadderCard()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		m_cardTime.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.crossLadderHandler.ValidateHasCrossLadderCard()));
		RefreshCardTime();
		if (zbSelf.Instance.crossLadderHandler.ValidateHasCrossLadderCard())
		{
			m_spineCard.AnimationState.SetAnimation(0, "loop", true);
		}
		else
		{
			m_spineCard.AnimationState.SetAnimation(0, "normal", false);
		}
	}

	private void RefreshMatch()
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		if (openCrossPeak && zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId >= zbSelf.Instance.crossLadderHandler.maxLadderId)
		{
			((VisualElement)m_btnCrossPeak).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbCrossPeak).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnFight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_spriteVictory.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbOpendCrossPeak).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			((VisualElement)m_btnCrossPeak).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbCrossPeak).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnFight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_spriteVictory.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbOpendCrossPeak).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		m_itemLadder.Refresh(zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId, visName: true);
		if (zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId < zbSelf.Instance.crossLadderHandler.maxLadderId)
		{
			zbLadderCsvRecord recordByKey = zbDataConfig.Instance.ladderCsvTable.GetRecordByKey(zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId + 1);
			if (recordByKey.score > zbServerData.sGameData.CrossLadderInfo.AchieveInfo.Score)
			{
				((TextElement)m_lbDesc).text = string.Format(zbUtil.Language("lang.10885"), recordByKey.score - zbServerData.sGameData.CrossLadderInfo.AchieveInfo.Score);
			}
			else
			{
				((TextElement)m_lbDesc).text = "";
			}
			if (zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId >= 6)
			{
				((TextElement)m_lbOpendCrossPeak).text = string.Format(zbUtil.Language("lang.11120"), 2000 - zbServerData.sGameData.CrossLadderInfo.AchieveInfo.Score, zbUtil.Language("lang.11088"));
			}
			else
			{
				((TextElement)m_lbOpendCrossPeak).text = "";
			}
		}
		else
		{
			((TextElement)m_lbDesc).text = "";
			((TextElement)m_lbOpendCrossPeak).text = "";
		}
		for (int i = 0; i < 3; i++)
		{
			string resName = "ui/image/crossLadder/crossLadder_point.jpg";
			if (i < zbServerData.sGameData.CrossLadderInfo.CombatInfo.VictoryCount)
			{
				resName = "ui/image/crossLadder/crossLadder_point_light.jpg";
			}
			m_spriteVictoryCounts[i].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
		}
		((TextElement)m_lbCount).text = zbUtil.Language("lang.10159") + ": " + (zbServerData.sGameData.CrossLadderInfo.SelfInfo.DailyCount - zbServerData.sGameData.CrossLadderInfo.SelfInfo.CombatCount);
	}

	private void RefreshUpgrade()
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		m_curLadder.Refresh(zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId, visName: true);
		m_nextLadder.Refresh(zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId + 1, visName: true);
		for (int i = 0; i < 5; i++)
		{
			string resName = "ui/image/crossLadder/crossLadder_upgrade_bg.png";
			if (i < zbServerData.sGameData.CrossLadderInfo.CombatInfo.UpgradeState.Count)
			{
				switch ((ELadderUpgradeState)zbServerData.sGameData.CrossLadderInfo.CombatInfo.UpgradeState[i])
				{
				case ELadderUpgradeState.Win:
					resName = "ui/image/crossLadder/crossLadder_upgrade_win.png";
					break;
				case ELadderUpgradeState.Fail:
					resName = "ui/image/crossLadder/crossLadder_upgrade_fail.png";
					break;
				}
			}
			m_spriteResults[i].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
		}
		if (zbServerData.sGameData.CrossLadderInfo.CombatInfo.State != ELadderState.Upgrading && !zbSelf.Instance.crossLadderHandler.isUpgradeAck)
		{
			zbWsMsgHandler.Instance.CrossLadder_UpgradeConfirm_Req();
		}
	}

	private void RefreshTargetAwards()
	{
		IVisualElementScheduledItem scrollSchedule = m_scrollSchedule;
		if (scrollSchedule != null)
		{
			scrollSchedule.Pause();
		}
		m_scrollSchedule = null;
		((TextElement)m_lbScore).text = zbUtil.Language("lang.10299") + $"<color=#FFD700>{zbServerData.sGameData.CrossLadderInfo.AchieveInfo.Score}</color>";
		zbLadderTargetAwardCsvTableData records = zbDataConfig.Instance.ladderTargetAwardCsvTable.GetRecords();
		((VisualElement)m_scrollView).Clear();
		VisualElement val = null;
		List<VisualElement> list = new List<VisualElement>();
		foreach (zbLadderTargetAwardCsvRecord value in records.records.Values)
		{
			zbCrossLadderItem zbCrossLadderItem2 = new zbCrossLadderItem();
			zbCrossLadderItem2.OnEnable(this);
			zbCrossLadderItem2.Refresh(value.id);
			((VisualElement)m_scrollView).Add((VisualElement)(object)zbCrossLadderItem2);
			list.Add((VisualElement)(object)zbCrossLadderItem2);
			if (value.id <= zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId)
			{
				val = (VisualElement)(object)zbCrossLadderItem2;
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

	private void RefreshPreGrandPrizeByScroll(bool needRefresh = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.FloorToInt(m_scrollView.scrollOffset.x / 110f);
		int num2 = -1;
		for (int i = 0; i < m_grandIndexes.Count; i++)
		{
			if (m_grandIndexes[i] > num + 5)
			{
				num2 = i;
				break;
			}
		}
		if (num2 == -1 && m_grandIndexes.Count > 0)
		{
			num2 = m_grandIndexes.Count - 1;
		}
		if ((num2 != m_currentGrandIndex) | needRefresh)
		{
			m_currentGrandIndex = num2;
			int index = m_grandIndexes[m_currentGrandIndex];
			zbLadderTargetAwardCsvRecord zbLadderTargetAwardCsvRecord2 = m_records[index];
			m_grandPrize.Refresh(zbLadderTargetAwardCsvRecord2.id, isGrandPrize: true);
		}
	}

	private void RefreshTime()
	{
		if (zbSelf.Instance.crossLadderHandler.IsCrossLadderOpen())
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeSmart(zbServerData.sGameData.CrossLadderInfo.OpenInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
		else
		{
			((TextElement)m_lbTime).text = "";
		}
	}

	private void OnTitle()
	{
		string text = string.Format(zbUtil.Language("lang.10899"), zbServerData.sGameData.CrossLadderInfo.OpenInfo.GroupId) + ": ";
		text = ((zbServerData.sGameData.CrossLadderInfo.OpenInfo.GroupRange[1] != 9999) ? (text + string.Format(zbUtil.Language("lang.10907"), zbServerData.sGameData.CrossLadderInfo.OpenInfo.GroupRange[0], zbServerData.sGameData.CrossLadderInfo.OpenInfo.GroupRange[1])) : (text + string.Format(zbUtil.Language("lang.10908"), zbServerData.sGameData.CrossLadderInfo.OpenInfo.GroupRange[0])));
		ShowTip(text, 200);
	}

	private void OnVictoryCount()
	{
		string strTip = string.Format(zbUtil.Language("lang.10898"), zbSelf.Instance.crossLadderHandler.victory3);
		ShowTip(strTip, 1100);
	}

	private void ShowTip(string strTip, int posY)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbTip).text = strTip;
		m_nodeTip.style.top = StyleLength.op_Implicit((float)posY);
		m_nodeTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (m_hideSchedule != null)
		{
			m_hideSchedule.Pause();
			m_hideSchedule = null;
		}
		m_hideSchedule = m_RootElement.schedule.Execute((Action)delegate
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			m_nodeTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}).StartingIn(5000L);
	}

	private void OnFight()
	{
		if (zbServerData.sGameData.CrossLadderInfo.SelfInfo.DailyCount > zbServerData.sGameData.CrossLadderInfo.SelfInfo.CombatCount)
		{
			zbUIMgr.Instance.ShowCrossLadderMatching();
		}
		else if (zbSelf.Instance.crossLadderHandler.GetBuyLimit() > zbServerData.sGameData.CrossLadderInfo.SelfInfo.BuyCombat)
		{
			zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.CrossLadderMatch);
		}
		else if (!zbSelf.Instance.crossLadderHandler.ValidateHasCrossLadderCard())
		{
			zbUIMgr.Instance.ShowCrossLadderHelpCardWnd();
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10700"));
		}
	}

	private void OnUpgradeFight()
	{
		zbWsMsgHandler.Instance.CrossLadder_UpgradeFightStart_Req();
	}

	private void OnRecord()
	{
		zbWsMsgHandler.Instance.CrossLadder_RecordList_Req();
	}

	private void OnSettleAward()
	{
		zbUIMgr.Instance.ShowCrossLadderSettleAward();
	}

	public void OnRule()
	{
		string[] langKeys = new string[19]
		{
			"lang.10922", "lang.10923", "lang.10924", "lang.10925", "lang.11153", "", "lang.10926", "lang.10927", "lang.10928", "lang.10929",
			"lang.10930", "lang.10931", "lang.10932", "lang.10933", "", "lang.10934", "lang.10935", "lang.10936", "lang.10937"
		};
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnRank()
	{
		zbWsMsgHandler.Instance.CrossLadder_FriendList_Req(openRank: true);
	}

	private void OnCard()
	{
		zbUIMgr.Instance.ShowCrossLadderHelpCardWnd();
	}

	private void OnCrossPeak()
	{
		if (openCrossPeak && zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId >= 26)
		{
			if (zbSelf.Instance.crossLadderHandler.HasTargetAwardNotify())
			{
				zbWsMsgHandler.Instance.CrossLadder_TargetAward_Req();
				zbUIMgr.Instance.ShowMessageBox("Tip", zbUtil.Language("lang.11086"), OnSwitchConfirm);
			}
			else
			{
				zbUIMgr.Instance.ShowMessageBox("Tip", zbUtil.Language("lang.11087"), OnSwitchConfirm);
			}
		}
	}

	protected void OnSwitchConfirm(bool isOk)
	{
		if (isOk)
		{
			zbGameSave.Instance.SetLong("OpenCrossPeak_" + zbSelf.Instance.GetRoleId(), zbSelf.Instance.timeHandler.GetServerTime());
			zbWsMsgHandler.Instance.CrossPeak_Data_Req(openWnd: true);
		}
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		RefreshPreGrandPrizeByScroll();
	}

	private void RefreshCardTime()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (zbSelf.Instance.crossLadderHandler.ValidateHasCrossLadderCard())
		{
			long num = zbSelf.Instance.crossLadderHandler.GetCrossLadderCardDetail().ExpireTm - zbSelf.Instance.timeHandler.GetServerTime();
			((TextElement)m_lbCardTime).text = zbUtil.FormatTimeSmart(num);
			bool flag = num < 86400;
			if (flag != m_isCardTimeRed)
			{
				m_isCardTimeRed = flag;
				((VisualElement)m_lbCardTime).style.color = StyleColor.op_Implicit(flag ? Color.red : Color.white);
			}
		}
	}

	private void RefreshNotify()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		m_notifyCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.crossLadderHandler.HasFreePackRedDot()));
		m_notifyRecord.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.reminderHandler.HasReminderNotify(EReminderType.LadderRecord)));
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
		RefreshCardTime();
		RefreshNotify();
	}

	private void OnOpenPeakCloseSelf(object data)
	{
		OnClose();
	}

	private void OnEventUpdate(object data)
	{
		Refresh();
	}
}
