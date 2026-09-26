using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarFightWnd : zbBaseWnd
{
	private Button m_btnBack;

	private Button m_btnFight;

	private Label m_lbTime;

	private int m_refreshTimerSelf;

	private Label m_lbSelfPre;

	private VisualElement m_gunObject;

	private Label m_lbGunLevel;

	private Label m_lbGunAmmo;

	private zbRectProgress m_gunProgress;

	private Button m_btnGun;

	private VisualElement[] m_nodeDetail = (VisualElement[])(object)new VisualElement[5];

	private TemplateContainer[] tb_skillMod = (TemplateContainer[])(object)new TemplateContainer[5];

	private zbPieProgress[] m_skillOutProgress = new zbPieProgress[5];

	private zbPieProgress[] m_skillInProgress = new zbPieProgress[5];

	private VisualElement[] m_skillIcon = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_lbSkillLevel = (Label[])(object)new Label[5];

	private Button[] m_btnSkill = (Button[])(object)new Button[5];

	private Label[] m_lbIconNum = (Label[])(object)new Label[5];

	private Label m_lbAttackDesc;

	private Label m_lbActionDesc;

	private Label m_lbSearchDesc;

	private Label m_lbSelfPointDesc;

	private Label m_lbLegionPointDesc;

	private Button m_btnRule;

	private Button m_btnRank;

	private Button m_btnLegionWarTask;

	private VisualElement m_taskNotify;

	private Button m_btnBtns;

	private Button m_btnMask;

	private Button m_btnLog;

	private VisualElement m_logNotify;

	private int attackLimitNum;

	private int actionLimitNum;

	private long fightStartTime;

	private Label m_lbFightStartTime;

	public zbLegionWarFightMap m_homeMap { get; set; }

	public zbLegionWarFightCinema m_homeMapCinema { get; set; }

	protected override void Awake()
	{
		base.Awake();
		m_homeMapCinema = ((Component)this).GetComponent<zbLegionWarFightCinema>();
		m_homeMapCinema.Init(this);
		m_homeMap = new zbLegionWarFightMap(this, m_homeMapCinema);
	}

	protected override void OnEnable()
	{
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_homeMap.OnEnable();
		m_LastCameraOrthographicSize = 7.4f;
		m_gunObject = UQueryExtensions.Q<VisualElement>(m_RootElement, "gunObject", (string)null);
		m_lbGunLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_gun", (string)null);
		m_lbGunAmmo = UQueryExtensions.Q<Label>(m_RootElement, "lb_ammo", (string)null);
		m_gunProgress = UQueryExtensions.Q<zbRectProgress>(m_RootElement, (string)null, (string)null);
		((TextElement)m_lbGunAmmo).text = "";
		m_gunProgress.progress = 0f;
		m_btnGun = UQueryExtensions.Q<Button>(m_RootElement, "button_gun", (string)null);
		m_lbSelfPre = UQueryExtensions.Q<Label>(m_RootElement, "lb_selfPre", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_btnMask = UQueryExtensions.Q<Button>(m_RootElement, "btn_mask", (string)null);
		m_btnBtns = UQueryExtensions.Q<Button>(m_RootElement, "btn_btns", (string)null);
		for (int i = 0; i < 5; i++)
		{
			tb_skillMod[i] = UQueryExtensions.Q<TemplateContainer>(m_RootElement, "skill" + (i + 1), (string)null);
			m_nodeDetail[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tb_skillMod[i], "nodeDetail", (string)null);
			m_skillOutProgress[i] = UQueryExtensions.Q<zbPieProgress>((VisualElement)(object)tb_skillMod[i], "outProgress", (string)null);
			m_skillOutProgress[i].progress = 100f;
			m_skillInProgress[i] = UQueryExtensions.Q<zbPieProgress>((VisualElement)(object)tb_skillMod[i], "inProgress", (string)null);
			m_skillInProgress[i].progress = 100f;
			m_skillIcon[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tb_skillMod[i], "skillIcon", (string)null);
			m_lbSkillLevel[i] = UQueryExtensions.Q<Label>((VisualElement)(object)tb_skillMod[i], "lb_skill", (string)null);
			m_btnSkill[i] = UQueryExtensions.Q<Button>((VisualElement)(object)tb_skillMod[i], "btn_skill", (string)null);
			int curIndex = i + 1;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSkill[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnSkillClick(evt, curIndex);
			}, "", (TrickleDown)0);
			m_lbIconNum[i] = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_" + (i + 1), (string)null), "lb_icon", (string)null);
		}
		m_btnLegionWarTask = UQueryExtensions.Q<Button>(m_RootElement, "btn_legionWarTask", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLegionWarTask, (Action)OnTask, "", (TrickleDown)0);
		m_lbAttackDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_attackDesc", (string)null);
		m_lbActionDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_actionDesc", (string)null);
		m_lbSearchDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_searchDesc", (string)null);
		m_lbSelfPointDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_selfPointDesc", (string)null);
		m_lbLegionPointDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_legionPointDesc", (string)null);
		m_lbFightStartTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_leftTime", (string)null);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_btnRank = UQueryExtensions.Q<Button>(m_RootElement, "btn_rank", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRank, (Action)OnRank, "", (TrickleDown)0);
		((VisualElement)tb_skillMod[4]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGun, (Action<ClickEvent>)delegate(ClickEvent evt)
		{
			OnSkillClick(evt, 0);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBtns, (Action)OnMask, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMask, (Action)OnCloseMask, "", (TrickleDown)0);
		m_btnLog = UQueryExtensions.Q<Button>(m_RootElement, "btn_log", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLog, (Action)OnLog, "", (TrickleDown)0);
		m_logNotify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnLog);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnFight = UQueryExtensions.Q<Button>(m_RootElement, "btn_fight", (string)null);
		m_taskNotify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnLegionWarTask);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action)OnFight, "", (TrickleDown)0);
		zbLanguage.Instance.SetButtonTextVisible((Button[])(object)new Button[3] { m_btnRank, m_btnRule, m_btnLegionWarTask });
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_LegionWarPointInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_ReminderList, RefreshNotify);
	}

	public override void Refresh()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		m_homeMapCinema.FocusToPosition(new Vector3(-2f, 1f));
		RefreshNotify();
		UpdatePerSecond();
	}

	private void RefreshNotify(object data = null)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		m_taskNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.legionWarHandler.HasTaskNotify()));
		m_logNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.reminderHandler.HasReminderNotify(EReminderType.LegionWar)));
	}

	public void RefreshCustom()
	{
		(attackLimitNum, actionLimitNum) = zbSelf.Instance.legionWarHandler.GetAttackLimitNum();
		zbLegionWarFightMgr.Instance.InitHeroList();
		RefreshSelfInfo();
		RefreshSkill();
		LegionWarPointForceDetail legionWarPointForceDetail = null;
		LegionWarPointForceDetail legionWarPointForceDetail2 = null;
		foreach (LegionWarPointForceDetail pointForce in zbSelf.Instance.legionWarHandler.legionWarPointInfo.PointState.PointForceList)
		{
			if (pointForce.ForceId == ELegionWarForce.Red)
			{
				legionWarPointForceDetail = pointForce;
			}
			else
			{
				legionWarPointForceDetail2 = pointForce;
			}
		}
		LegionWarPointForceDetail legionWarPointForceDetail3 = ((IEnumerable<LegionWarPointForceDetail>)zbSelf.Instance.legionWarHandler.legionWarPointInfo.PointState.PointForceList).FirstOrDefault((LegionWarPointForceDetail p) => p.ForceId == ELegionWarForce.Red);
		LegionWarPointForceDetail legionWarPointForceDetail4 = ((IEnumerable<LegionWarPointForceDetail>)zbSelf.Instance.legionWarHandler.legionWarPointInfo.PointState.PointForceList).FirstOrDefault((LegionWarPointForceDetail p) => p.ForceId == ELegionWarForce.Blue);
		zbSelf.Instance.legionWarHandler.SetPlayerCount(m_RootElement, legionWarPointForceDetail3.PlayerCount, legionWarPointForceDetail4.PlayerCount, zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId);
		zbSelf.Instance.legionWarHandler.SetBluntProgress(m_RootElement, legionWarPointForceDetail.OccupyPct, legionWarPointForceDetail2.OccupyPct, zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId, "process_600_", 600, ".png");
		if (zbSelf.Instance.legionWarHandler.legionWarSelfInfo.BebackState == EBoolState.True)
		{
			Hide();
			zbUIMgr.Instance.ShowLegionWar();
		}
		((TextElement)m_lbAttackDesc).text = zbUtil.Language("lang.11210");
		((TextElement)m_lbActionDesc).text = zbUtil.Language("lang.11211");
		((TextElement)m_lbSearchDesc).text = zbUtil.Language("lang.11212");
		((TextElement)m_lbSelfPointDesc).text = zbUtil.Language("lang.11213");
		((TextElement)m_lbLegionPointDesc).text = zbUtil.Language("lang.11214");
		if (zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId == ELegionWarForce.Red)
		{
			((TextElement)m_lbSelfPre).text = zbUtil.Language("lang.11239") + ": " + legionWarPointForceDetail3.OccupyPct * 100f + "%";
		}
		else
		{
			((TextElement)m_lbSelfPre).text = zbUtil.Language("lang.11239") + ": " + legionWarPointForceDetail4.OccupyPct * 100f + "%";
		}
	}

	private void RefreshSelfInfo()
	{
		LegionWarSelfInfo legionWarSelfInfo = zbSelf.Instance.legionWarHandler.legionWarSelfInfo;
		LegionWarLegionInfo legionWarLegionInfo = zbSelf.Instance.legionWarHandler.legionWarLegionInfo;
		if (legionWarSelfInfo != null && legionWarLegionInfo != null)
		{
			((TextElement)m_lbIconNum[0]).text = legionWarSelfInfo.AttackNum + "/" + attackLimitNum;
			((TextElement)m_lbIconNum[1]).text = Math.Max(legionWarSelfInfo.ActionPoints, 0) + "/" + actionLimitNum;
			((TextElement)m_lbIconNum[2]).text = zbSelf.Instance.legionWarHandler.searchAwardLimitNum - legionWarSelfInfo.AwardNum + "/" + zbSelf.Instance.legionWarHandler.searchAwardLimitNum;
			((TextElement)m_lbIconNum[3]).text = legionWarSelfInfo.SelfScore.ToString();
			((TextElement)m_lbIconNum[4]).text = legionWarLegionInfo.ScoreCount.ToString();
		}
	}

	public void RefreshSkill()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		RepeatedField<int> modList = zbSelf.Instance.legionWarHandler.legionWarSelfTeamInfo.ModList;
		_ = zbSelf.Instance.legionWarHandler.legionWarSelfTeamInfo.SkillList;
		for (int i = 0; i < 5; i++)
		{
			m_nodeDetail[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		bool flag = false;
		int num = 0;
		foreach (int item in modList)
		{
			zbModCsvRecord recordByKey = zbDataConfig.Instance.modCsvTable.GetRecordByKey(item);
			if (recordByKey.modIndex == 1000)
			{
				if (recordByKey.skillType[0] == 1)
				{
					flag = true;
					continue;
				}
				((VisualElement)tb_skillMod[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_nodeDetail[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				SkillDetail skill = zbSelf.Instance.legionWarHandler.GetSkill(recordByKey.skillType[0]);
				zbSkillCsvRecord recordByKey2 = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(skill.SkillId);
				Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey2.icon);
				m_skillIcon[num].style.backgroundImage = StyleBackground.op_Implicit(val);
				m_skillIcon[num].transform.scale = Vector3.one * 0.45f;
				int modNum = zbSelf.Instance.legionWarHandler.GetModNum(recordByKey.skillType[0]);
				((TextElement)m_lbSkillLevel[num]).text = modNum.ToString();
				num++;
			}
		}
		m_gunObject.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((TextElement)m_lbGunLevel).text = "Lv." + zbSelf.Instance.legionWarHandler.GetModNum(1);
	}

	protected override void UpdateAtShow()
	{
		if (!GetMyGameObjectSceneRoot().activeSelf)
		{
			return;
		}
		base.UpdateAtShow();
		if (zbSelf.Instance.legionWarHandler.GetLegionWarStateByMapInfo() != ELegionWarState.Fighting)
		{
			OnClose();
			return;
		}
		RefreshNotify();
		m_homeMap.Update();
		if (zbSelf.Instance.timeHandler.GetServerTime() < fightStartTime)
		{
			long timeInSeconds = fightStartTime - zbSelf.Instance.timeHandler.GetServerTime();
			((TextElement)m_lbFightStartTime).text = zbUtil.FormatTimeMMSS(timeInSeconds);
			zbUtil.SetDisplayIfChanged((VisualElement)(object)m_lbFightStartTime, (DisplayStyle)0);
		}
		else
		{
			zbUtil.SetDisplayIfChanged((VisualElement)(object)m_lbFightStartTime, (DisplayStyle)1);
		}
	}

	protected override void UpdatePerSecond()
	{
		if (GetMyGameObjectSceneRoot().activeSelf)
		{
			base.UpdatePerSecond();
			long leftTimeByState = zbSelf.Instance.legionWarHandler.GetLeftTimeByState();
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(leftTimeByState);
			m_refreshTimerSelf++;
			if (m_refreshTimerSelf >= 10)
			{
				m_refreshTimerSelf = 0;
				zbWsMsgHandler.Instance.LegionWar_EnterPoint_Req(zbSelf.Instance.legionWarHandler.legionWarPointInfo.PointState.PointId);
			}
		}
	}

	public void OnRule()
	{
		string[] langKeys = new string[1] { "lang.11207" };
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	public void OnRank()
	{
		zbUIMgr.Instance.ShowLegionWarRank();
	}

	private void OnMask()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_btnMask).SetEnabled(true);
		((VisualElement)m_btnMask).pickingMode = (PickingMode)0;
	}

	private void OnTask()
	{
		zbUIMgr.Instance.ShowLegionWarTask();
	}

	public void OnLog()
	{
		zbWsMsgHandler.Instance.LegionWar_RecordList_Req();
	}

	private void OnCloseMask()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnMask).SetEnabled(false);
		((VisualElement)m_btnMask).pickingMode = (PickingMode)1;
	}

	private void OnFight()
	{
		if (zbSelf.Instance.legionWarHandler.legionWarSelfInfo.AttackNum <= 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.EErrorCode.30353"));
		}
		else if (zbSelf.Instance.timeHandler.GetServerTime() >= fightStartTime)
		{
			fightStartTime = zbSelf.Instance.timeHandler.GetServerTime() + 5;
			zbWsMsgHandler.Instance.LegionWar_FightStart_Req();
		}
	}

	public void OnSkillClick(ClickEvent evt, int skillIndex)
	{
		zbSelf.Instance.legionWarHandler.OnModList(skillIndex);
	}

	private void OnClose()
	{
		Hide();
		zbLegionWarFightMgr.Instance.heroPositions.Clear();
		zbLegionWarWnd zbLegionWarWnd2 = zbUIMgr.Instance.GetWndById(EWndId.LegionWar) as zbLegionWarWnd;
		if ((Object)(object)zbLegionWarWnd2 != (Object)null)
		{
			zbLegionWarWnd2.RefreshCustom(fromOut: true);
		}
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom();
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		RefreshCustom();
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		RefreshCustom();
	}
}
