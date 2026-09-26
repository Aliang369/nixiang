using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarPointTipWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Label m_lbNameTitle;

	private Label m_lbSelfPre;

	private Button m_btnClose;

	private Label m_lbPointState;

	private Label m_lbRedCount;

	private Label m_lbBlueCount;

	private Label m_lbSpeed;

	private Label m_lbScore;

	private Label m_lbAward;

	private Label m_lbSearchScore;

	private Button m_btnGoto;

	private Button m_btnSearch;

	private Button m_btnEnter;

	private Button m_btnConfirm;

	private Label m_lbSearchCount;

	private Label m_lbTiliCount;

	private LegionWarPointStateDetail m_detail;

	private zbLegionWarPointTarget renderTarget;

	private VisualElement m_foldIcon;

	private Image m_descImg;

	private Vector2 m_pos2;

	private float occupyRateIdle;

	private float occupyRateNormal;

	private float occupyRateCrowded;

	private float searchRateIdle;

	private float searchRateNormal;

	private float searchRateCrowded;

	private Button m_btnCheckSpeed;

	private Label m_lbSpeedNormal;

	private Label m_lbSpeedPlayerBouns;

	private Label m_lbSpeedDecayRate;

	private Button m_btnCheckStand;

	private Label m_lbStandNormal;

	private Label m_lbStandPlayerBouns;

	private Label m_lbStandStatus;

	private Button m_btnCheckScore;

	private Label m_lbSearchNormal;

	private Label m_lbSearchPlayerBouns;

	private Label m_lbSearchStatus;

	private Button m_btnCheckState;

	private Label m_lbNormalStateVal;

	private Label m_lbIdleStateVal;

	private Label m_lbCrowdedStateVal;

	private Button m_btnMask;

	private VisualElement m_speedTip;

	private VisualElement m_standTip;

	private VisualElement m_searchTip;

	private VisualElement m_stateTip;

	private VisualElement m_speedDecayRate;

	private JToken m_baseJson;

	private Button m_btnBiaoji;

	private VisualElement m_biaojied;

	private VisualElement m_standTipStatus;

	private VisualElement m_searchTipState;

	private VisualElement m_decayRate;

	private VisualElement m_speedTipState;

	private Button m_btnBiaojiLabel;

	private VisualElement m_arrivalTime;

	private Label m_lbArrivalTime;

	private int m_refreshTimerSelf;

	private int searchCD;

	protected override void OnEnable()
	{
		//IL_07c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cd: Expected O, but got Unknown
		//IL_07dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0811: Unknown result type (might be due to invalid IL or missing references)
		//IL_082b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0841: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbNameTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_nameTitle", (string)null);
		m_lbSelfPre = UQueryExtensions.Q<Label>(m_RootElement, "lb_selfPre", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbPointState = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "node0", (string)null), "lb_value", (string)null);
		m_lbRedCount = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "red", (string)null), "lb_count", (string)null);
		m_lbBlueCount = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "blue", (string)null), "lb_count", (string)null);
		m_lbSpeed = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "node1", (string)null), "lb_value", (string)null);
		m_lbScore = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "node2", (string)null), "lb_value", (string)null);
		m_lbAward = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "node3", (string)null), "lb_value", (string)null);
		m_lbSearchScore = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "node3", (string)null), "lb_searchScore", (string)null);
		m_btnGoto = UQueryExtensions.Q<Button>(m_RootElement, "btn_goto", (string)null);
		m_lbTiliCount = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnGoto, "lb_count", (string)null);
		m_btnSearch = UQueryExtensions.Q<Button>(m_RootElement, "btn_search", (string)null);
		m_lbSearchCount = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnSearch, "lb_count", (string)null);
		m_btnEnter = UQueryExtensions.Q<Button>(m_RootElement, "btn_enter", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_foldIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "foldIcon", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGoto, (Action)OnGoto, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSearch, (Action)OnSearch, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnEnter, (Action)OnEnter, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnConfirm, "", (TrickleDown)0);
		m_btnCheckSpeed = UQueryExtensions.Q<Button>(m_RootElement, "btn_checkSpeed", (string)null);
		m_lbSpeedNormal = UQueryExtensions.Q<Label>(m_RootElement, "lb_speedNormal", (string)null);
		m_lbSpeedPlayerBouns = UQueryExtensions.Q<Label>(m_RootElement, "lb_speedPlayerBouns", (string)null);
		m_lbSpeedDecayRate = UQueryExtensions.Q<Label>(m_RootElement, "lb_speedDecayRate", (string)null);
		m_btnCheckStand = UQueryExtensions.Q<Button>(m_RootElement, "btn_checkStand", (string)null);
		m_lbStandNormal = UQueryExtensions.Q<Label>(m_RootElement, "lb_standNormal", (string)null);
		m_lbStandStatus = UQueryExtensions.Q<Label>(m_RootElement, "lb_standStatus", (string)null);
		m_lbStandPlayerBouns = UQueryExtensions.Q<Label>(m_RootElement, "lb_standPlayerBouns", (string)null);
		m_btnCheckScore = UQueryExtensions.Q<Button>(m_RootElement, "btn_checkScore", (string)null);
		m_lbSearchNormal = UQueryExtensions.Q<Label>(m_RootElement, "lb_searchNormal", (string)null);
		m_lbSearchPlayerBouns = UQueryExtensions.Q<Label>(m_RootElement, "lb_searchPlayerBouns", (string)null);
		m_lbSearchStatus = UQueryExtensions.Q<Label>(m_RootElement, "lb_searchStatus", (string)null);
		m_btnCheckState = UQueryExtensions.Q<Button>(m_RootElement, "btn_checkState", (string)null);
		m_btnBiaoji = UQueryExtensions.Q<Button>(m_RootElement, "btn_biaoji", (string)null);
		m_biaojied = UQueryExtensions.Q<VisualElement>(m_RootElement, "biaoji_ed", (string)null);
		m_standTipStatus = UQueryExtensions.Q<VisualElement>(m_RootElement, "standTipStatus", (string)null);
		m_searchTipState = UQueryExtensions.Q<VisualElement>(m_RootElement, "searchTipStatus", (string)null);
		m_decayRate = UQueryExtensions.Q<VisualElement>(m_RootElement, "decayRate", (string)null);
		m_speedTipState = UQueryExtensions.Q<VisualElement>(m_RootElement, "speedTipState", (string)null);
		m_arrivalTime = UQueryExtensions.Q<VisualElement>(m_RootElement, "arrivalTime", (string)null);
		m_lbArrivalTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_arrivalTime", (string)null);
		m_btnMask = UQueryExtensions.Q<Button>(m_RootElement, "btn_mask", (string)null);
		m_speedTip = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnMask, "speedTip", (string)null);
		m_standTip = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnMask, "standTip", (string)null);
		m_searchTip = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnMask, "searchTip", (string)null);
		m_stateTip = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnMask, "stateTip", (string)null);
		m_lbNormalStateVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_normalStateVal", (string)null);
		m_lbIdleStateVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_idleStateVal", (string)null);
		m_lbCrowdedStateVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_crowdedStateVal", (string)null);
		m_btnBiaojiLabel = UQueryExtensions.Q<Button>(m_RootElement, "btn_biaojiLabel", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBiaojiLabel, (Action)OnBiaojiLabel, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCheckSpeed, (Action)OnCheckSpeed, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCheckStand, (Action)OnCheckStand, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCheckScore, (Action)OnCheckScore, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCheckState, (Action)OnCheckState, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMask, (Action)OnMask, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBiaoji, (Action)OnBiaoji, "", (TrickleDown)0);
		occupyRateIdle = Extensions.Value<float>((IEnumerable<JToken>)zbSelf.Instance.legionWarHandler.legionWarBaseRecord["occupyRate"][(object)"idle"]);
		occupyRateNormal = Extensions.Value<float>((IEnumerable<JToken>)zbSelf.Instance.legionWarHandler.legionWarBaseRecord["occupyRate"][(object)"normal"]);
		occupyRateCrowded = Extensions.Value<float>((IEnumerable<JToken>)zbSelf.Instance.legionWarHandler.legionWarBaseRecord["occupyRate"][(object)"crowded"]);
		searchRateIdle = Extensions.Value<float>((IEnumerable<JToken>)zbSelf.Instance.legionWarHandler.legionWarBaseRecord["searchAward"][(object)"awardRate"][(object)"idle"]);
		searchRateNormal = Extensions.Value<float>((IEnumerable<JToken>)zbSelf.Instance.legionWarHandler.legionWarBaseRecord["searchAward"][(object)"awardRate"][(object)"normal"]);
		searchRateCrowded = Extensions.Value<float>((IEnumerable<JToken>)zbSelf.Instance.legionWarHandler.legionWarBaseRecord["searchAward"][(object)"awardRate"][(object)"crowded"]);
		renderTarget = ((Component)((Component)this).transform.Find("legionWarPointTarget")).GetComponent<zbLegionWarPointTarget>();
		if (m_descImg == null)
		{
			m_descImg = new Image();
			((VisualElement)m_descImg).style.left = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImg).style.top = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImg).style.width = StyleLength.op_Implicit(240f);
			((VisualElement)m_descImg).style.height = StyleLength.op_Implicit(240f);
			((VisualElement)m_descImg).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		}
		m_foldIcon.Add((VisualElement)(object)m_descImg);
		AddDark();
		m_baseJson = (JToken)(object)zbDataConfig.Instance.legionWarBaseJsonTable.GetRecord();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_LegionWarMapInfo, OnEventUpdateLegionWarMapInfo);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_LegionWarPointAward, OnEventUpdateLegionWarPointAward);
	}

	public void RefreshCustom(LegionWarPointStateDetail detail, Vector2 pos2)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		searchCD = Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.legionWarBaseJsonTable.GetRecord()["searchAward"][(object)"searchCD"]);
		m_detail = detail;
		m_pos2 = pos2;
		InitTarget();
		m_refreshTimerSelf = 0;
		InitDetail();
		RefreshSearchBtn();
	}

	private void InitTarget()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		renderTarget.StartRender(m_descImg, m_pos2);
	}

	private void InitDetail()
	{
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Invalid comparison between Unknown and I4
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Invalid comparison between Unknown and I4
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Invalid comparison between Unknown and I4
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		LegionWarPointForceDetail legionWarPointForceDetail = ((IEnumerable<LegionWarPointForceDetail>)m_detail.PointForceList).FirstOrDefault((LegionWarPointForceDetail p) => p.ForceId == ELegionWarForce.Red);
		LegionWarPointForceDetail legionWarPointForceDetail2 = ((IEnumerable<LegionWarPointForceDetail>)m_detail.PointForceList).FirstOrDefault((LegionWarPointForceDetail p) => p.ForceId == ELegionWarForce.Blue);
		LegionWarPointForceDetail legionWarPointForceDetail3 = GetLegionWarPointForceDetail();
		zbSelf.Instance.legionWarHandler.SetPlayerCount(m_RootElement, legionWarPointForceDetail.PlayerCount, legionWarPointForceDetail2.PlayerCount, zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId);
		zbSelf.Instance.legionWarHandler.SetBluntProgress(m_RootElement, legionWarPointForceDetail.OccupyPct, legionWarPointForceDetail2.OccupyPct, zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId, "process_420_", 420, ".png");
		bool flag = zbSelf.Instance.legionWarHandler.CanBiaoji();
		((VisualElement)m_btnBiaoji).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!flag || zbSelf.Instance.legionWarHandler.legionWarLegionInfo.MarkPointId == m_detail.PointId) ? 1 : 0));
		m_biaojied.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.legionWarHandler.legionWarLegionInfo.MarkPointId != m_detail.PointId) ? 1 : 0));
		if (zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ArrivalTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			((VisualElement)m_btnSearch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnEnter).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnGoto).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else if (zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ToPointId == m_detail.PointId)
		{
			if (m_detail.OccupyForceId == zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId)
			{
				((VisualElement)m_btnSearch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			else
			{
				((VisualElement)m_btnSearch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			((VisualElement)m_btnEnter).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnGoto).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			((VisualElement)m_btnSearch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnEnter).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			if (zbSelf.Instance.legionWarHandler.IsNearPoint(m_detail.PointId, zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ToPointId))
			{
				if (zbSelf.Instance.legionWarHandler.GetLegionWarPointStateDetail(zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ToPointId).OccupyForceId == zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId || m_detail.OccupyForceId == zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId)
				{
					((VisualElement)m_btnGoto).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					m_arrivalTime.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					RefreshMoveInfo();
				}
				else
				{
					((VisualElement)m_btnGoto).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				}
			}
			else
			{
				((VisualElement)m_btnGoto).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
		((VisualElement)m_btnConfirm).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(((int)((VisualElement)m_btnSearch).resolvedStyle.display != 1 || (int)((VisualElement)m_btnEnter).resolvedStyle.display != 1 || (int)((VisualElement)m_btnGoto).resolvedStyle.display != 1) ? 1 : 0));
		zbLegionWarMapCsvRecord recordByKey = zbDataConfig.Instance.legionWarMapCsvTable.GetRecordByKey(m_detail.PointId);
		((TextElement)m_lbNameTitle).text = zbUtil.Language(recordByKey.pointName);
		if (zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId == ELegionWarForce.Red)
		{
			((TextElement)m_lbSelfPre).text = zbUtil.Language("lang.11239") + ": " + legionWarPointForceDetail.OccupyPct * 100f + "%";
		}
		else
		{
			((TextElement)m_lbSelfPre).text = zbUtil.Language("lang.11239") + ": " + legionWarPointForceDetail2.OccupyPct * 100f + "%";
		}
		zbLegionWarMapCsvRecord recordByKey2 = zbDataConfig.Instance.legionWarMapCsvTable.GetRecordByKey(recordByKey.pointId);
		double value = legionWarPointForceDetail3.OccupyRate * 60f * 100f;
		double num = recordByKey2.standOccupyPct * 60f * 100f;
		bool num2 = legionWarPointForceDetail.OccupyPct == 0f && legionWarPointForceDetail2.OccupyPct == 0f && legionWarPointForceDetail.PlayerCount == 0 && legionWarPointForceDetail2.PlayerCount == 0;
		bool flag2 = m_detail.OccupyForceId == ELegionWarForce.EmptyValue;
		value = ((!num2) ? Math.Round(value, 2) : 0.0);
		int num3 = (int)(legionWarPointForceDetail3.ScoreRate * 60f);
		float num4 = recordByKey2.standScore * 60f;
		int num5 = 0;
		float num6 = recordByKey.awardList[0][2];
		if (legionWarPointForceDetail3.CrowdedState == ELegionWarCrowdedState.Crowded)
		{
			num5 = (int)((float)recordByKey.awardList[0][2] * searchRateCrowded);
			((TextElement)m_lbPointState).text = zbUtil.Language("lang.11195");
		}
		else if (legionWarPointForceDetail3.CrowdedState == ELegionWarCrowdedState.Idle)
		{
			num5 = (int)((float)recordByKey.awardList[0][2] * searchRateIdle);
			((TextElement)m_lbPointState).text = zbUtil.Language("lang.11197");
		}
		else
		{
			num5 = (int)((float)recordByKey.awardList[0][2] * searchRateNormal);
			((TextElement)m_lbPointState).text = zbUtil.Language("lang.11196");
		}
		num5 = ((!flag2) ? num5 : 0);
		string text = ((value < num) ? "red" : "#007008");
		string arg = (((float)num3 < num4) ? "red" : "#007008");
		string arg2 = (((float)num5 < num6) ? "red" : "#007008");
		LegionWarSelfInfo legionWarSelfInfo = zbSelf.Instance.legionWarHandler.legionWarSelfInfo;
		((TextElement)m_lbTiliCount).text = legionWarSelfInfo.ActionPoints + "/" + recordByKey.actionCost;
		((TextElement)m_lbSearchCount).text = zbSelf.Instance.legionWarHandler.searchAwardLimitNum - zbSelf.Instance.legionWarHandler.legionWarSelfInfo.AwardNum + "/1";
		string arg3 = ((value > 0.0) ? $"<color={text}>+{value:F2}%</color>" : ((!(value < 0.0)) ? ("<color=" + text + ">0%</color>") : $"<color={text}>{value:F2}%</color>"));
		string arg4 = $"<color={arg}>{num3}</color>";
		string text2 = $"<color={arg2}>{num5}</color>";
		((TextElement)m_lbSpeed).text = string.Format(zbUtil.Language("lang.11170"), arg3);
		((TextElement)m_lbScore).text = string.Format(zbUtil.Language("lang.11170"), arg4);
		((TextElement)m_lbAward).text = text2;
		((TextElement)m_lbSearchScore).text = (flag2 ? "0" : Extensions.Value<float>((IEnumerable<JToken>)m_baseJson[(object)"searchAward"][(object)"searchScore"]).ToString());
	}

	private LegionWarPointForceDetail GetLegionWarPointForceDetail()
	{
		foreach (LegionWarPointForceDetail pointForce in m_detail.PointForceList)
		{
			if (zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId == pointForce.ForceId)
			{
				return pointForce;
			}
		}
		return null;
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnGoto()
	{
		zbWsMsgHandler.Instance.LegionWar_Goto_Req(m_detail.PointId);
		Hide();
	}

	private void OnBiaojiLabel()
	{
		zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11260"));
	}

	private void OnConfirm()
	{
		Hide();
	}

	private void OnCheckSpeed()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_speedTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_searchTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_standTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_stateTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbLegionWarMapCsvRecord recordByKey = zbDataConfig.Instance.legionWarMapCsvTable.GetRecordByKey(m_detail.PointId);
		LegionWarPointForceDetail legionWarPointForceDetail = GetLegionWarPointForceDetail();
		_ = legionWarPointForceDetail.OccupyRate;
		bool num = m_detail.OccupyForceId == ELegionWarForce.EmptyValue;
		bool flag = m_detail.PointForceList[0].PlayerCount > 0;
		bool flag2 = m_detail.PointForceList[1].PlayerCount > 0;
		bool flag3 = num & flag & flag2;
		bool flag4 = ((zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId == ELegionWarForce.Red) ? flag : flag2);
		m_decayRate.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((flag3 || !flag4) ? 1 : 0));
		m_speedTipState.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag3));
		((TextElement)m_lbSpeedNormal).text = "+" + Math.Round(recordByKey.standOccupyPct * 60f * 100f, 2) + "%";
		switch (legionWarPointForceDetail.CrowdedState)
		{
		case ELegionWarCrowdedState.Idle:
			((TextElement)m_lbSpeedPlayerBouns).text = string.Format("-{0}% ", Math.Abs(Math.Round((Extensions.Value<float>((IEnumerable<JToken>)m_baseJson[(object)"occupyRate"][(object)"idle"]) - 1f) * 100f)));
			break;
		case ELegionWarCrowdedState.Crowded:
			((TextElement)m_lbSpeedPlayerBouns).text = string.Format("+{0}% ", Math.Abs(Math.Round((Extensions.Value<float>((IEnumerable<JToken>)m_baseJson[(object)"occupyRate"][(object)"crowded"]) - 1f) * 100f)));
			break;
		case ELegionWarCrowdedState.Normal:
			((TextElement)m_lbSpeedPlayerBouns).text = string.Format("+{0}% ", Math.Abs(Math.Round((Extensions.Value<float>((IEnumerable<JToken>)m_baseJson[(object)"occupyRate"][(object)"normal"]) - 1f) * 100f)));
			break;
		}
	}

	private void OnCheckStand()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_speedTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_searchTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_standTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_stateTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbLegionWarMapCsvRecord recordByKey = zbDataConfig.Instance.legionWarMapCsvTable.GetRecordByKey(m_detail.PointId);
		LegionWarPointForceDetail legionWarPointForceDetail = GetLegionWarPointForceDetail();
		bool flag = m_detail.OccupyForceId == ELegionWarForce.EmptyValue;
		((TextElement)m_lbStandNormal).text = Math.Round(recordByKey.standScore * 60f, 2).ToString();
		switch (legionWarPointForceDetail.CrowdedState)
		{
		case ELegionWarCrowdedState.Idle:
			((TextElement)m_lbStandPlayerBouns).text = string.Format("+{0}% ", Math.Abs((Extensions.Value<float>((IEnumerable<JToken>)m_baseJson[(object)"scoreRate"][(object)"idle"]) - 1f) * 100f));
			break;
		case ELegionWarCrowdedState.Crowded:
			((TextElement)m_lbStandPlayerBouns).text = string.Format("-{0}% ", Math.Abs((Extensions.Value<float>((IEnumerable<JToken>)m_baseJson[(object)"scoreRate"][(object)"crowded"]) - 1f) * 100f));
			break;
		case ELegionWarCrowdedState.Normal:
			((TextElement)m_lbStandPlayerBouns).text = string.Format("{0}% ", Math.Abs((Extensions.Value<float>((IEnumerable<JToken>)m_baseJson[(object)"scoreRate"][(object)"normal"]) - 1f) * 100f));
			break;
		}
		m_standTipStatus.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
	}

	private void OnCheckScore()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_speedTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_searchTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_standTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_stateTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbLegionWarMapCsvRecord recordByKey = zbDataConfig.Instance.legionWarMapCsvTable.GetRecordByKey(m_detail.PointId);
		LegionWarPointForceDetail legionWarPointForceDetail = GetLegionWarPointForceDetail();
		_ = legionWarPointForceDetail.OccupyRate;
		bool flag = m_detail.OccupyForceId == ELegionWarForce.EmptyValue;
		((TextElement)m_lbSearchNormal).text = recordByKey.awardList[0][2].ToString();
		switch (legionWarPointForceDetail.CrowdedState)
		{
		case ELegionWarCrowdedState.Idle:
			((TextElement)m_lbSearchPlayerBouns).text = string.Format("+{0}%", Math.Abs((Extensions.Value<float>((IEnumerable<JToken>)m_baseJson[(object)"searchAward"][(object)"awardRate"][(object)"idle"]) - 1f) * 100f));
			break;
		case ELegionWarCrowdedState.Crowded:
			((TextElement)m_lbSearchPlayerBouns).text = string.Format("-{0}%", Math.Abs((Extensions.Value<float>((IEnumerable<JToken>)m_baseJson[(object)"searchAward"][(object)"awardRate"][(object)"crowded"]) - 1f) * 100f));
			break;
		case ELegionWarCrowdedState.Normal:
			((TextElement)m_lbSearchPlayerBouns).text = string.Format("+{0}%", Math.Abs((Extensions.Value<float>((IEnumerable<JToken>)m_baseJson[(object)"searchAward"][(object)"awardRate"][(object)"normal"]) - 1f) * 100f));
			break;
		}
		m_searchTipState.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
	}

	private void OnCheckState()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_speedTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_searchTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_stateTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_standTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbLegionWarMapCsvRecord recordByKey = zbDataConfig.Instance.legionWarMapCsvTable.GetRecordByKey(m_detail.PointId);
		((TextElement)m_lbNormalStateVal).text = recordByKey.standCrowded.ToString();
		((TextElement)m_lbIdleStateVal).text = "<" + (float)recordByKey.standCrowded * Extensions.Value<float>((IEnumerable<JToken>)m_baseJson[(object)"crowdedRate"][(object)"idle"]);
		((TextElement)m_lbCrowdedStateVal).text = ">" + (float)recordByKey.standCrowded * Extensions.Value<float>((IEnumerable<JToken>)m_baseJson[(object)"crowdedRate"][(object)"normal"]);
	}

	private void OnMask()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private void OnBiaoji()
	{
		zbWsMsgHandler.Instance.LegionWar_MarkPoint_Req(m_detail.PointId);
	}

	private void OnSearch()
	{
		LegionWarSelfInfo legionWarSelfInfo = zbSelf.Instance.legionWarHandler.legionWarSelfInfo;
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		zbDataConfig.Instance.legionWarBaseJsonTable.GetRecord();
		if (legionWarSelfInfo.AwardNum < zbSelf.Instance.legionWarHandler.searchAwardLimitNum && serverTime - legionWarSelfInfo.AwardLastTm > searchCD)
		{
			zbWsMsgHandler.Instance.LegionWar_PointAward_Req(m_detail.PointId);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11208"), zbUtil.FormatTimeHHMMSS(legionWarSelfInfo.AwardLastTm + searchCD - zbSelf.Instance.timeHandler.GetServerTime())));
		}
	}

	private void OnEnter()
	{
		zbWsMsgHandler.Instance.LegionWar_EnterPoint_Req(m_detail.PointId, openLegionWarFight: true);
		Hide();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		if (zbSelf.Instance.legionWarHandler.GetLegionWarStateByMapInfo() != ELegionWarState.Fighting)
		{
			OnClose();
		}
	}

	protected override void UpdatePerSecond()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		base.UpdatePerSecond();
		m_refreshTimerSelf++;
		if (m_refreshTimerSelf >= 5)
		{
			zbWsMsgHandler.Instance.LegionWar_PointInfo_Req(m_detail.PointId, Vector2.op_Implicit(m_pos2));
		}
		RefreshSearchBtn();
	}

	private void RefreshSearchBtn()
	{
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		LegionWarSelfInfo legionWarSelfInfo = zbSelf.Instance.legionWarHandler.legionWarSelfInfo;
		if (legionWarSelfInfo.AwardNum < zbSelf.Instance.legionWarHandler.searchAwardLimitNum && serverTime - legionWarSelfInfo.AwardLastTm < searchCD)
		{
			((TextElement)m_btnSearch).text = zbUtil.FormatTimeHHMMSS(legionWarSelfInfo.AwardLastTm + searchCD - zbSelf.Instance.timeHandler.GetServerTime());
		}
		else
		{
			((TextElement)m_btnSearch).text = zbUtil.Language("lang.11254");
		}
	}

	private void RefreshMoveInfo()
	{
		LegionWarPointForceDetail legionWarPointForceDetail = GetLegionWarPointForceDetail();
		float num = 0f;
		switch (legionWarPointForceDetail.CrowdedState)
		{
		case ELegionWarCrowdedState.Normal:
			num = Extensions.Value<float>((IEnumerable<JToken>)zbSelf.Instance.legionWarHandler.legionWarBaseRecord["action"][(object)"moveCD"][(object)"normal"]);
			break;
		case ELegionWarCrowdedState.Idle:
			num = Extensions.Value<float>((IEnumerable<JToken>)zbSelf.Instance.legionWarHandler.legionWarBaseRecord["action"][(object)"moveCD"][(object)"idle"]);
			break;
		case ELegionWarCrowdedState.Crowded:
			num = Extensions.Value<float>((IEnumerable<JToken>)zbSelf.Instance.legionWarHandler.legionWarBaseRecord["action"][(object)"moveCD"][(object)"crowded"]);
			break;
		}
		((TextElement)m_lbArrivalTime).text = num.ToString();
	}

	private void OnEventUpdateLegionWarMapInfo(object data)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (IsShow())
		{
			RefreshCustom(m_detail, m_pos2);
		}
	}

	private void OnEventUpdateLegionWarPointAward(object data)
	{
		LegionWarPointForceDetail legionWarPointForceDetail = GetLegionWarPointForceDetail();
		float num = 0f;
		if (legionWarPointForceDetail.CrowdedState == ELegionWarCrowdedState.Crowded)
		{
			num = Extensions.Value<float>((IEnumerable<JToken>)m_baseJson[(object)"searchAward"][(object)"searchScore"]) * searchRateCrowded;
		}
		else if (legionWarPointForceDetail.CrowdedState == ELegionWarCrowdedState.Idle)
		{
			num = Extensions.Value<float>((IEnumerable<JToken>)m_baseJson[(object)"searchAward"][(object)"searchScore"]) * searchRateIdle;
		}
		else if (legionWarPointForceDetail.CrowdedState == ELegionWarCrowdedState.Normal)
		{
			num = Extensions.Value<float>((IEnumerable<JToken>)m_baseJson[(object)"searchAward"][(object)"searchScore"]) * searchRateNormal;
		}
		zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11253"), num));
	}
}
