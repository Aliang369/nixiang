using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarWnd : zbBaseWnd
{
	private Coroutine m_coroutine;

	private Coroutine m_buildCoroutune;

	private Button m_btnClose;

	[NonSerialized]
	public VisualElement hudRoot;

	private Button m_btnRule;

	private Button m_btnRank;

	private Button m_btnBiaoji;

	private Button m_btnCallBack;

	private Button m_btnPrivilege;

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

	private Button m_btnLegionWarTask;

	private Button m_btnBtns;

	private Button m_btnMask;

	private Label m_lbWin;

	private Label m_lbTime;

	private Label m_lbAttackDesc;

	private Label m_lbActionDesc;

	private Label m_lbSearchDesc;

	private Label m_lbSelfPointDesc;

	private Label m_lbLegionPointDesc;

	private Label m_lbzhanlingLevel;

	private VisualElement m_zhanlingProgress;

	private VisualElement m_zhanlingBg;

	private VisualElement m_zhanlingNotify;

	private VisualElement m_taskNotify;

	private Button m_btnChat;

	private Button m_btnProgressLeft;

	private Button m_btnProgressRight;

	private Button m_btnLog;

	private VisualElement m_logNotify;

	private int m_refreshTimerSelf;

	private bool firstMoveStop;

	private float lastLeftTime;

	private float m_spineAvatarScale = 0.35f;

	private int attackLimitNum;

	private int actionLimitNum;

	private int restCD;

	private float attackRecoverCD;

	private float actionRecoverCD;

	[NonSerialized]
	public SkeletonAnimation fangxiangAnimation;

	[NonSerialized]
	public SkeletonAnimation[] fangxiangArrow = (SkeletonAnimation[])(object)new SkeletonAnimation[3];

	[NonSerialized]
	public Bone[] fangxiangBone = (Bone[])(object)new Bone[3];

	public zbLegionWarMap m_homeMap { get; set; }

	public zbLegionWarMapCinema m_homeMapCinema { get; set; }

	public zbPlayerAvatar m_spineAvatar { get; set; }

	protected override void Awake()
	{
		base.Awake();
		m_homeMapCinema = ((Component)this).GetComponent<zbLegionWarMapCinema>();
		m_homeMapCinema.Init(this);
		m_homeMap = new zbLegionWarMap(this, m_homeMapCinema);
	}

	protected override void OnDestroy()
	{
		if (m_coroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_coroutine);
			m_coroutine = null;
		}
		if (m_buildCoroutune != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_buildCoroutune);
			m_buildCoroutune = null;
		}
		base.OnDestroy();
	}

	protected override void OnEnable()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f9: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_homeMap.OnEnable();
		m_spineAvatar = ((Component)m_GameObjectSceneRoot.transform.Find("playerAvatar")).GetComponent<zbPlayerAvatar>();
		fangxiangAnimation = ((Component)m_GameObjectSceneRoot.transform.Find("fangXiang")).GetComponent<SkeletonAnimation>();
		m_LastCameraPosition = new Vector3(0f, 0f, ((Component)zbCameraHelper.Instance.mainCamera).transform.position.z);
		m_LastCameraOrthographicSize = 10f;
		((Component)zbCameraHelper.Instance.mainCamera).transform.position = m_LastCameraPosition;
		zbCameraHelper.Instance.mainCamera.orthographicSize = m_LastCameraOrthographicSize;
		m_gunObject = UQueryExtensions.Q<VisualElement>(m_RootElement, "gunObject", (string)null);
		m_lbGunLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_gun", (string)null);
		m_lbGunAmmo = UQueryExtensions.Q<Label>(m_RootElement, "lb_ammo", (string)null);
		m_gunProgress = UQueryExtensions.Q<zbRectProgress>(m_RootElement, (string)null, (string)null);
		((TextElement)m_lbGunAmmo).text = "";
		m_gunProgress.progress = 0f;
		m_btnGun = UQueryExtensions.Q<Button>(m_RootElement, "button_gun", (string)null);
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
		((VisualElement)tb_skillMod[4]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbWin = UQueryExtensions.Q<Label>(m_RootElement, "lb_win", (string)null);
		m_btnMask = UQueryExtensions.Q<Button>(m_RootElement, "btn_mask", (string)null);
		m_btnBtns = UQueryExtensions.Q<Button>(m_RootElement, "btn_btns", (string)null);
		m_lbAttackDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_attackDesc", (string)null);
		m_lbActionDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_actionDesc", (string)null);
		m_lbSearchDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_searchDesc", (string)null);
		m_lbSelfPointDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_selfPointDesc", (string)null);
		m_lbLegionPointDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_legionPointDesc", (string)null);
		m_lbzhanlingLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_zhanlingLevel", (string)null);
		m_zhanlingProgress = UQueryExtensions.Q<VisualElement>(m_RootElement, "zhanlingProgress", (string)null);
		m_btnChat = UQueryExtensions.Q<Button>(m_RootElement, "btn_chat", (string)null);
		m_btnLegionWarTask = UQueryExtensions.Q<Button>(m_RootElement, "btn_legionWarTask", (string)null);
		m_taskNotify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnLegionWarTask);
		m_btnProgressLeft = UQueryExtensions.Q<Button>(m_RootElement, "btn_progressLeft", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnProgressLeft, (Action)OnLeft, "", (TrickleDown)0);
		m_btnProgressRight = UQueryExtensions.Q<Button>(m_RootElement, "btn_progressRight", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnProgressRight, (Action)OnRight, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLegionWarTask, (Action)OnTask, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGun, (Action<ClickEvent>)delegate(ClickEvent evt)
		{
			OnSkillClick(evt, 0);
		}, "", (TrickleDown)0);
		hudRoot = UQueryExtensions.Q<VisualElement>(m_RootElement, "hudRoot", (string)null);
		m_btnLog = UQueryExtensions.Q<Button>(m_RootElement, "btn_log", (string)null);
		m_logNotify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnLog);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLog, (Action)OnLog, "", (TrickleDown)0);
		m_btnBiaoji = UQueryExtensions.Q<Button>(m_RootElement, "btnBiaoji", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBiaoji, (Action<ClickEvent>)OnClickBiaoji, "", (TrickleDown)0);
		m_btnCallBack = UQueryExtensions.Q<Button>(m_RootElement, "btnCallBack", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCallBack, (Action<ClickEvent>)OnClickCallBack, "", (TrickleDown)0);
		m_btnPrivilege = UQueryExtensions.Q<Button>(m_RootElement, "btn_privilege", (string)null);
		m_zhanlingBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "zhanlingBg", (string)null);
		m_zhanlingNotify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnPrivilege, (Vector2?)new Vector2(-10f, -10f), 1f, true);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_btnRank = UQueryExtensions.Q<Button>(m_RootElement, "btn_rank", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRank, (Action)OnRank, "", (TrickleDown)0);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPrivilege, (Action)OnZhangling, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBtns, (Action)OnMask, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMask, (Action)OnCloseMask, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnChat, (Action)OnChat, "", (TrickleDown)0);
		restCD = Extensions.Value<int>((IEnumerable<JToken>)zbSelf.Instance.legionWarHandler.legionWarBaseRecord["restCD"]);
		zbLanguage.Instance.SetButtonTextVisible((Button[])(object)new Button[4] { m_btnRank, m_btnRule, m_btnLegionWarTask, m_btnLog });
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_LegionWarBaseInfo, OnEventUpdateLegionWarMapInfo);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_LegionWarMapInfo, OnEventUpdateLegionWarMapInfo);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_LegionWarMove, OnEventUpdateLegionWarMapMove);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_ReminderList, RefreshNotify);
	}

	protected override void Start()
	{
		base.Start();
		for (int i = 0; i < 3; i++)
		{
			string text = "fangxiang_arrow" + (i + 1);
			fangxiangArrow[i] = ((Component)((Component)fangxiangAnimation).transform.Find(text)).GetComponent<SkeletonAnimation>();
			SkeletonAnimation val = fangxiangArrow[i];
			fangxiangBone[i] = ((SkeletonRenderer)val).skeleton.FindBone("shoot");
		}
		zbUtil.SetActive(((Component)fangxiangAnimation).gameObject, active: false);
	}

	public override void Refresh()
	{
		m_refreshTimerSelf = 0;
	}

	public void RefreshCustom(bool fromOut = false)
	{
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		zbLegionWarMgr.Instance.Init(this);
		if (fromOut)
		{
			OnClickBiaoji(null);
		}
		(int, int) tuple = zbSelf.Instance.legionWarHandler.GetAttackLimitNum();
		attackLimitNum = tuple.Item1;
		actionLimitNum = tuple.Item2;
		attackRecoverCD = (float)restCD / (float)attackLimitNum;
		actionRecoverCD = (float)restCD * (1f - zbSelf.Instance.legionWarHandler.GetActionRecoverPer()) / (float)actionLimitNum;
		firstMoveStop = false;
		int num = Extensions.Value<int>((IEnumerable<JToken>)zbSelf.Instance.legionWarHandler.legionWarBaseRecord["warWinScore"]);
		((TextElement)m_lbWin).text = string.Format(zbUtil.Language("lang.11191"), zbUtil.ConvertToTBMK(num));
		zbPlayerGunCsvRecord recordByKey = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(zbSelf.Instance.GetGunId());
		m_gunObject.style.backgroundImage = StyleBackground.op_Implicit(zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.battleBanner));
		int redScoreCount = 0;
		int blueScoreCount = 0;
		int redScoreRate = 0;
		int blueScoreRate = 0;
		foreach (LegionWarForceStateDetail forceState in zbSelf.Instance.legionWarHandler.legionWarMapInfo.ForceStateList)
		{
			if (forceState.ForceId == ELegionWarForce.Red)
			{
				redScoreCount = forceState.ScoreCount;
				redScoreRate = forceState.ScoreRate * 60;
			}
			else
			{
				blueScoreCount = forceState.ScoreCount;
				blueScoreRate = forceState.ScoreRate * 60;
			}
		}
		zbSelf.Instance.legionWarHandler.SetSharpProgress(UQueryExtensions.Q<VisualElement>(m_RootElement, "title", (string)null), redScoreCount, blueScoreCount, zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId, 296, redScoreRate, blueScoreRate);
		zbLegionWarMgr.Instance.Refresh();
		RefreshSelfInfo();
		RefreshSkill();
		m_spineAvatar.RefreshCustom(zbSelf.Instance.GetAvatarId(), zbSelf.Instance.GetGunId());
		lastLeftTime = zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ArrivalTm - zbSelf.Instance.timeHandler.GetServerTime();
		if (zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ArrivalTm <= zbSelf.Instance.timeHandler.GetServerTime())
		{
			m_spineAvatar.SetAnimation(0, "idle", loop: true);
			zbLegionWarPoint point = zbLegionWarMgr.Instance.GetPoint(zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ToPointId);
			((Component)m_spineAvatar).transform.position = new Vector3(point.GetPositionXY().x, point.GetPositionXY().y, point.GetPositionXY().y / 5f - 2f);
			((Component)m_spineAvatar).transform.localScale = new Vector3(0f - m_spineAvatarScale, m_spineAvatarScale, m_spineAvatarScale);
			((Component)fangxiangAnimation).transform.position = new Vector3(((Component)m_spineAvatar).transform.position.x, ((Component)m_spineAvatar).transform.position.y, ((Component)m_spineAvatar).transform.position.z + 0.002f);
		}
		UpdatePerSecond();
		((TextElement)m_lbAttackDesc).text = zbUtil.Language("lang.11210");
		((TextElement)m_lbActionDesc).text = zbUtil.Language("lang.11211");
		((TextElement)m_lbSearchDesc).text = zbUtil.Language("lang.11212");
		((TextElement)m_lbSelfPointDesc).text = zbUtil.Language("lang.11213");
		((TextElement)m_lbLegionPointDesc).text = zbUtil.Language("lang.11214");
		if (zbSelf.Instance.legionWarHandler.legionWarSelfInfo.BebackState == EBoolState.True)
		{
			zbUIMgr.Instance.ShowLegionWarBeback();
		}
		RefreshNotify();
		RefreshZhanlingInfo();
	}

	private void RefreshNotify(object data = null)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		m_taskNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.legionWarHandler.HasTaskNotify()));
		m_logNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.reminderHandler.HasReminderNotify(EReminderType.LegionWar)));
	}

	private void RefreshZhanlingInfo()
	{
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		LegionWarZhanLingInfo legionWarZhanLingInfo = zbServerData.sGameData.LegionWarBaseInfo.LegionWarZhanLingInfo;
		((TextElement)m_lbzhanlingLevel).text = legionWarZhanLingInfo.Level.ToString();
		LegionWarSelfInfo legionWarSelfInfo = zbSelf.Instance.legionWarHandler.legionWarSelfInfo;
		Dictionary<int, zbLegionWarLevelCsvRecord> records = zbDataConfig.Instance.legionWarLevelCsvTable.GetRecords().records;
		zbLegionWarLevelCsvRecord recordByKey = zbDataConfig.Instance.legionWarLevelCsvTable.GetRecordByKey(legionWarZhanLingInfo.Level + 1);
		bool num = legionWarZhanLingInfo.Level == records.Count || recordByKey == null;
		ELegionWarState legionWarState = zbSelf.Instance.legionWarHandler.GetLegionWarState();
		float num2 = (num ? 1f : Mathf.Clamp01((float)((legionWarState == ELegionWarState.Fighting) ? legionWarSelfInfo.SelfScore : legionWarZhanLingInfo.Score) / (float)recordByKey.score));
		bool flag = num2 * 100f >= 100f;
		m_zhanlingNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!flag || zbSelf.Instance.legionWarHandler.IsZhanlingMax()) ? 1 : 0));
		m_zhanlingBg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionwar/" + (flag ? "zhanling_2" : "zhanling_1") + ".jpg"));
		m_zhanlingProgress.style.width = new StyleLength(Length.Percent(num2 * 100f));
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
			if (zbDataConfig.Instance.legionWarMapCsvTable.GetRecordByKey(zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ToPointId).pointForce == (int)zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId)
			{
				long num = zbSelf.Instance.timeHandler.GetServerTime() - zbSelf.Instance.legionWarHandler.legionWarSelfInfo.RestStartTm;
				int num2 = Math.Min(zbSelf.Instance.legionWarHandler.legionWarSelfInfo.AttackNum + (int)((float)num / attackRecoverCD), attackLimitNum);
				int val = Math.Min(zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ActionPoints + (int)((float)num / actionRecoverCD), actionLimitNum);
				((TextElement)m_lbIconNum[0]).text = num2 + "/" + attackLimitNum;
				((TextElement)m_lbIconNum[1]).text = Math.Max(val, 0) + "/" + actionLimitNum;
			}
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
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		if (!GetMyGameObjectSceneRoot().activeSelf)
		{
			return;
		}
		base.UpdateAtShow();
		if (zbSelf.Instance.legionWarHandler.GetLegionWarStateByMapInfo() != ELegionWarState.Fighting)
		{
			OnClose(null);
			zbWsMsgHandler.Instance.LegionWar_Data_Req();
			return;
		}
		m_homeMap.Update();
		if (zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ArrivalTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			firstMoveStop = true;
			m_spineAvatar.SetAnimation(0, "run", loop: true);
			zbLegionWarPoint point = zbLegionWarMgr.Instance.GetPoint(zbSelf.Instance.legionWarHandler.legionWarSelfInfo.FromPointId);
			zbLegionWarPoint point2 = zbLegionWarMgr.Instance.GetPoint(zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ToPointId);
			lastLeftTime -= Time.deltaTime;
			float num = 1f - lastLeftTime / (float)zbSelf.Instance.legionWarHandler.legionWarSelfInfo.MoveCD;
			float num2 = point.GetPositionX() + (point2.GetPositionX() - point.GetPositionX()) * num;
			float num3 = point.GetPositionY() + (point2.GetPositionY() - point.GetPositionY()) * num;
			((Component)m_spineAvatar).transform.position = new Vector3(num2, num3, num3 / 2f - 2f);
			if (((Component)point2).transform.position.x - ((Component)point).transform.position.x > 0f)
			{
				((Component)m_spineAvatar).transform.localScale = new Vector3(0f - m_spineAvatarScale, m_spineAvatarScale, m_spineAvatarScale);
			}
			else
			{
				((Component)m_spineAvatar).transform.localScale = new Vector3(m_spineAvatarScale, m_spineAvatarScale, m_spineAvatarScale);
			}
		}
		else if (firstMoveStop)
		{
			RefreshCustom();
			firstMoveStop = false;
		}
	}

	protected override void UpdatePerSecond()
	{
		if (GetMyGameObjectSceneRoot().activeSelf)
		{
			base.UpdatePerSecond();
			RefreshNotify();
			m_refreshTimerSelf++;
			if (m_refreshTimerSelf >= 13)
			{
				m_refreshTimerSelf = 0;
				zbWsMsgHandler.Instance.LegionWar_MapInfo_Req();
			}
			long leftTimeByState = zbSelf.Instance.legionWarHandler.GetLeftTimeByState();
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(leftTimeByState);
			if (zbDataConfig.Instance.legionWarMapCsvTable.GetRecordByKey(zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ToPointId).pointForce == (int)zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId)
			{
				long num = zbSelf.Instance.timeHandler.GetServerTime() - zbSelf.Instance.legionWarHandler.legionWarSelfInfo.RestStartTm;
				int num2 = Math.Min(zbSelf.Instance.legionWarHandler.legionWarSelfInfo.AttackNum + (int)((float)num / attackRecoverCD), attackLimitNum);
				int val = Math.Min(zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ActionPoints + (int)((float)num / actionRecoverCD), actionLimitNum);
				((TextElement)m_lbIconNum[0]).text = num2 + "/" + attackLimitNum;
				((TextElement)m_lbIconNum[1]).text = Math.Max(val, 0) + "/" + actionLimitNum;
			}
		}
	}

	public void OnRule()
	{
		string[] langKeys = new string[1] { "lang.11207" };
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	private void OnLeft()
	{
		ELegionWarForce legionForceId = zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId;
		int num = 0;
		foreach (LegionWarForceStateDetail forceState in zbSelf.Instance.legionWarHandler.legionWarMapInfo.ForceStateList)
		{
			if (forceState.ForceId == legionForceId)
			{
				num = forceState.ScoreRate * 60;
				break;
			}
		}
		string key = ((legionForceId == ELegionWarForce.Red) ? "lang.11250" : "lang.11251");
		zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language(key), num));
	}

	private void OnRight()
	{
		ELegionWarForce eLegionWarForce = ((zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId != ELegionWarForce.Red) ? ELegionWarForce.Red : ELegionWarForce.Blue);
		int num = 0;
		foreach (LegionWarForceStateDetail forceState in zbSelf.Instance.legionWarHandler.legionWarMapInfo.ForceStateList)
		{
			if (forceState.ForceId == eLegionWarForce)
			{
				num = forceState.ScoreRate * 60;
				break;
			}
		}
		string key = ((eLegionWarForce == ELegionWarForce.Red) ? "lang.11250" : "lang.11251");
		zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language(key), num));
	}

	public void OnChat()
	{
		zbUIMgr.Instance.ShowChat();
	}

	private void OnTask()
	{
		zbUIMgr.Instance.ShowLegionWarTask();
	}

	public void OnZhangling()
	{
		zbUIMgr.Instance.ShowLegionWarZhanling();
	}

	private void OnMask()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_btnMask).SetEnabled(true);
		((VisualElement)m_btnMask).pickingMode = (PickingMode)0;
	}

	private void OnCloseMask()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnMask).SetEnabled(false);
		((VisualElement)m_btnMask).pickingMode = (PickingMode)1;
	}

	public void OnRank()
	{
		zbUIMgr.Instance.ShowLegionWarRank();
	}

	public void OnLog()
	{
		zbWsMsgHandler.Instance.LegionWar_RecordList_Req();
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	public void OnClickBiaoji(ClickEvent evt)
	{
		m_homeMap.OnClickBiaoji();
	}

	public void OnClickCallBack(ClickEvent evt)
	{
		if (zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ArrivalTm - zbSelf.Instance.timeHandler.GetServerTime() > 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11203"));
			return;
		}
		if (zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ToPointId == 1001 || zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ToPointId == 2001)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11257"));
			return;
		}
		zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), zbUtil.Language("lang.11194"), delegate(bool isOK)
		{
			if (isOK)
			{
				zbWsMsgHandler.Instance.LegionWar_Rest_Req();
			}
		});
	}

	private void OnEventUpdateLegionWarMapInfo(object data)
	{
		zbLegionWarBebackWnd zbLegionWarBebackWnd2 = zbUIMgr.Instance.GetWndById(EWndId.LegionWarBeback) as zbLegionWarBebackWnd;
		if ((Object)(object)zbLegionWarBebackWnd2 == (Object)null || !zbLegionWarBebackWnd2.IsShow())
		{
			RefreshCustom();
		}
	}

	private void OnEventUpdateLegionWarMapMove(object data)
	{
		if (IsShow())
		{
			zbLegionWarMgr.Instance.HeroMoveHandler();
		}
	}

	public void OnSkillClick(ClickEvent evt, int skillIndex)
	{
		zbSelf.Instance.legionWarHandler.OnModList(skillIndex);
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
