using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarWatchWnd : zbBaseWnd
{
	private Coroutine m_coroutine;

	private Coroutine m_buildCoroutune;

	private Button m_btnClose;

	[NonSerialized]
	public VisualElement hudRoot;

	private Button m_btnRule;

	private Label m_lbWin;

	private Label m_lbTime;

	private Button m_btnChat;

	private int m_refreshTimerSelf;

	private bool firstMoveStop;

	private float lastLeftTime;

	private float m_spineAvatarScale = 0.35f;

	private int attackLimitNum;

	private int actionLimitNum;

	private int restCD;

	private float attackRecoverCD;

	private float actionRecoverCD;

	public zbLegionWarWatchMap m_homeMap { get; set; }

	public zbLegionWarWatchMapCinema m_homeMapCinema { get; set; }

	protected override void Awake()
	{
		base.Awake();
		m_homeMapCinema = ((Component)this).GetComponent<zbLegionWarWatchMapCinema>();
		m_homeMapCinema.Init(this);
		m_homeMap = new zbLegionWarWatchMap(this, m_homeMapCinema);
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
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_homeMap.OnEnable();
		m_LastCameraPosition = new Vector3(0f, 0f, ((Component)zbCameraHelper.Instance.mainCamera).transform.position.z);
		m_LastCameraOrthographicSize = 10f;
		((Component)zbCameraHelper.Instance.mainCamera).transform.position = m_LastCameraPosition;
		zbCameraHelper.Instance.mainCamera.orthographicSize = m_LastCameraOrthographicSize;
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbWin = UQueryExtensions.Q<Label>(m_RootElement, "lb_win", (string)null);
		m_btnChat = UQueryExtensions.Q<Button>(m_RootElement, "btn_chat", (string)null);
		hudRoot = UQueryExtensions.Q<VisualElement>(m_RootElement, "hudRoot", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnChat, (Action)OnChat, "", (TrickleDown)0);
		restCD = Extensions.Value<int>((IEnumerable<JToken>)zbSelf.Instance.legionWarHandler.legionWarBaseRecord["restCD"]);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_LegionWarBaseInfo, OnEventUpdateLegionWarMapInfo);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_LegionWarMapInfo, OnEventUpdateLegionWarMapInfo);
	}

	protected override void Start()
	{
		base.Start();
	}

	public override void Refresh()
	{
		m_refreshTimerSelf = 0;
	}

	public void RefreshCustom(bool fromOut = false)
	{
		zbLegionWarWatchMgr.Instance.Init(this);
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
		zbSelf.Instance.legionWarHandler.SetSharpProgress(UQueryExtensions.Q<VisualElement>(m_RootElement, "title", (string)null), redScoreCount, blueScoreCount, zbSelf.Instance.legionWarHandler.watchLegionWarForce, 296, redScoreRate, blueScoreRate);
		zbLegionWarWatchMgr.Instance.Refresh();
		UpdatePerSecond();
	}

	protected override void UpdateAtShow()
	{
		if (GetMyGameObjectSceneRoot().activeSelf)
		{
			base.UpdateAtShow();
			if (zbSelf.Instance.legionWarHandler.GetLegionWarStateByMapInfo() != ELegionWarState.Fighting)
			{
				OnClose(null);
				zbWsMsgHandler.Instance.LegionWar_Data_Req();
			}
			else
			{
				m_homeMap.Update();
			}
		}
	}

	protected override void UpdatePerSecond()
	{
		if (GetMyGameObjectSceneRoot().activeSelf)
		{
			base.UpdatePerSecond();
			m_refreshTimerSelf++;
			if (m_refreshTimerSelf >= 10)
			{
				m_refreshTimerSelf = 0;
				zbWsMsgHandler.Instance.LegionWar_WatchMapInfo_Req(zbSelf.Instance.legionHandler.GetLegionZone());
			}
			long leftTimeByState = zbSelf.Instance.legionWarHandler.GetLeftTimeByState();
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(leftTimeByState);
		}
	}

	public void OnRule()
	{
		string[] langKeys = new string[1] { "lang.11207" };
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	public void OnChat()
	{
		zbUIMgr.Instance.ShowChat();
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	public void OnClickBiaoji(ClickEvent evt)
	{
		m_homeMap.OnClickBiaoji();
	}

	private void OnEventUpdateLegionWarMapInfo(object data)
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
