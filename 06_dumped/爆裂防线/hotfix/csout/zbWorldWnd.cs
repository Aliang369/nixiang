using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldWnd : zbBaseWnd
{
	public VisualElement rootContainer;

	public VisualElement zoomContent;

	public zbWorldMap m_map;

	private VisualElement m_ui;

	private Label m_lbLevel;

	private Label m_progress;

	private Label m_lbProgress;

	private Button m_btnInfo;

	private Button m_btnRight;

	private VisualElement m_nodePanel;

	private zbWorldPanel m_panel = new zbWorldPanel();

	private bool m_isPanelVis;

	private Button m_btnResCollect;

	public Button m_btnCommRadar;

	public Button m_btnRadar;

	private SpineVisualElement m_spineRadar;

	private Button m_btnClose;

	private VisualElement m_notifyPanel;

	private VisualElement m_notifyResCollect;

	private VisualElement m_notifyCommRadar;

	private VisualElement m_notifyRadar;

	private Action<bool> m_callback;

	private bool m_needRefreshTime;

	private List<WorldEventDetail> m_radarEventList = new List<WorldEventDetail>();

	public bool m_isFirstIn = true;

	protected override void Awake()
	{
		base.Awake();
		m_map = new zbWorldMap();
	}

	protected override void OnEnable()
	{
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		rootContainer = UQueryExtensions.Q(m_RootElement, "rootContainer", (string)null);
		zoomContent = UQueryExtensions.Q(m_RootElement, "zoomContent", (string)null);
		m_map.OnEnable(this, rootContainer, zoomContent);
		m_ui = UQueryExtensions.Q<VisualElement>(m_RootElement, "ui", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_ui, "lb_level", (string)null);
		m_progress = UQueryExtensions.Q<Label>(m_ui, "lb_progress", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>(m_ui, "lb_progress_value", (string)null);
		m_btnInfo = UQueryExtensions.Q<Button>(m_ui, "btn_info", (string)null);
		m_btnRight = UQueryExtensions.Q<Button>(m_RootElement, "btn_right", (string)null);
		m_nodePanel = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel", (string)null);
		m_panel.OnEnable(this, m_nodePanel);
		m_btnResCollect = UQueryExtensions.Q<Button>(m_RootElement, "btn_resCollect", (string)null);
		m_btnCommRadar = UQueryExtensions.Q<Button>(m_RootElement, "btn_commRadar", (string)null);
		m_btnRadar = UQueryExtensions.Q<Button>(m_RootElement, "btn_radar", (string)null);
		m_spineRadar = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_radar", (string)null);
		((VisualElement)m_spineRadar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnInfo, (Action)OnInfo, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRight, (Action)OnRight, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnResCollect, (Action)OnResCollect, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCommRadar, (Action)OnCommRadar, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRadar, (Action)OnRadar, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_notifyPanel = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnRight, (Vector2?)new Vector2(-3f, 6f), 1f, true);
		m_notifyResCollect = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnResCollect, (Vector2?)new Vector2(0f, 0f), 1f, true);
		m_notifyCommRadar = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnCommRadar, (Vector2?)new Vector2(0f, 0f), 1f, true);
		m_notifyRadar = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnRadar);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Tili });
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_WorldInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_WorldEventRadar_Ack, OnEventRadarAck);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_HomelandInfo, OnEventUpdate);
		m_isPanelVis = zbSetSave.Instance.GetWorldPanelOpen();
		SetPanelVisible(m_isPanelVis);
	}

	public void RefreshCustom(WorldMapGridDetail gotoGridDetail = null, Action<bool> callback = null)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		m_callback = callback;
		if (zbServerData.sGameData.WorldInfo == null)
		{
			return;
		}
		((VisualElement)m_btnResCollect).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.worldHandler.CanWorldResIdle()));
		m_notifyPanel.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!zbSelf.Instance.worldHandler.HasWorldReconNotify(1) && !zbSelf.Instance.worldHandler.HasWorldReconNotify(2)) ? 1 : 0));
		m_notifyResCollect.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.worldHandler.HasResCollectNotify()));
		m_notifyCommRadar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.worldHandler.HasCommRadarNotify()));
		m_map.Refresh();
		if (gotoGridDetail != null)
		{
			SetPanelVisible(vis: false);
			((MonoBehaviour)this).StartCoroutine(DelayGotoGrid(gotoGridDetail));
		}
		RefreshProgress();
		RefreshRadar();
		if (m_isFirstIn)
		{
			if (zbSelf.Instance.worldHandler.HasWorldReconExploredNotify(1))
			{
				zbWsMsgHandler.Instance.World_ReconExplored_Req(1);
			}
			else if (zbSelf.Instance.worldHandler.HasWorldReconExploredNotify(2))
			{
				zbWsMsgHandler.Instance.World_ReconExplored_Req(2);
			}
			m_isFirstIn = false;
		}
	}

	private IEnumerator DelayGotoGrid(WorldMapGridDetail gridDetail)
	{
		yield return null;
		OnGotoGrid(gridDetail);
		if (m_callback != null)
		{
			zbUIMgr.Instance.ShowModel();
			yield return (object)new WaitForSeconds(0.5f);
			zbUIMgr.Instance.HideModel();
			m_callback?.Invoke(obj: true);
		}
	}

	private void RefreshProgress()
	{
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int exploredCount = zbSelf.Instance.worldHandler.GetExploredCount();
		foreach (KeyValuePair<int, zbWorldLevelCsvRecord> record in zbDataConfig.Instance.worldLevelCsvTable.GetRecords().records)
		{
			if (exploredCount >= record.Value.num)
			{
				num = record.Value.lv;
			}
		}
		((TextElement)m_lbLevel).text = num.ToString();
		zbWorldLevelCsvRecord recordByKey = zbDataConfig.Instance.worldLevelCsvTable.GetRecordByKey(num + 1);
		if (recordByKey != null)
		{
			((TextElement)m_lbProgress).text = exploredCount + "/" + recordByKey.num;
			float num2 = Mathf.Clamp((float)exploredCount / (float)recordByKey.num, 0f, 1f);
			((VisualElement)m_progress).style.width = StyleLength.op_Implicit(new Length(num2 * 200f, (LengthUnit)0));
		}
		else
		{
			((TextElement)m_lbProgress).text = "Max";
			((VisualElement)m_progress).style.width = StyleLength.op_Implicit(new Length(200f, (LengthUnit)0));
		}
	}

	private void RefreshRadar()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (zbServerData.sGameData.WorldInfo.EventInfo.NextTm < 0)
		{
			((TextElement)m_btnRadar).text = "--:--:--";
		}
		else
		{
			((TextElement)m_btnRadar).text = zbUtil.Language("lang.10524");
		}
		m_notifyRadar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.worldHandler.HasWorldRadarNotify()));
		if (zbSelf.Instance.timeHandler.GetServerTime() < zbServerData.sGameData.WorldInfo.EventInfo.NextTm)
		{
			m_needRefreshTime = true;
			RefreshTime();
		}
		else
		{
			m_needRefreshTime = false;
		}
	}

	private void RefreshTime()
	{
		if (zbServerData.sGameData.WorldInfo.EventInfo.NextTm >= 0 && m_needRefreshTime)
		{
			long num = zbServerData.sGameData.WorldInfo.EventInfo.NextTm - zbSelf.Instance.timeHandler.GetServerTime();
			if (num > 0)
			{
				((TextElement)m_btnRadar).text = zbUtil.FormatTimeHHMMSS(num);
				return;
			}
			m_needRefreshTime = false;
			RefreshRadar();
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
		if (m_map != null && m_map.worldItemMgr != null)
		{
			m_map.worldItemMgr.Update();
		}
	}

	private void OnInfo()
	{
		zbUIMgr.Instance.ShowWorldLevel();
	}

	private void OnRight()
	{
		SetPanelVisible(vis: true);
		zbSetSave.Instance.SetWorldPanelOpen(val: true);
	}

	public void SetPanelVisible(bool vis)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		m_isPanelVis = vis;
		((VisualElement)m_btnRight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_isPanelVis ? 1 : 0));
		m_nodePanel.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!m_isPanelVis));
		if (m_isPanelVis)
		{
			m_panel.Refresh();
		}
	}

	public void OnGotoGrid(WorldMapGridDetail gridDetail)
	{
		m_map.GotoGrid(gridDetail);
	}

	private void OnRadar()
	{
		if (zbServerData.sGameData.WorldInfo.EventInfo.NextTm < 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10559"));
		}
		else if (zbServerData.sGameData.WorldInfo.EventInfo.NextTm >= 0 && zbSelf.Instance.timeHandler.GetServerTime() >= zbServerData.sGameData.WorldInfo.EventInfo.NextTm)
		{
			zbWsMsgHandler.Instance.WorldEvent_Radar_Req();
		}
		else
		{
			zbUIMgr.Instance.ShowWorldRadar();
		}
	}

	private void OnResCollect()
	{
		if (zbSelf.Instance.worldHandler.CanWorldResIdle())
		{
			zbUIMgr.Instance.ShowWorldResourceCollect();
		}
	}

	private void OnCommRadar()
	{
		HomelandRoomDetail homelandRoomDetail = zbSelf.Instance.homelandHandler.GetHomelandRoomDetail(zbServerData.sGameData.HomelandInfo.BaseInfo.RoomMark.CommRadarId);
		if (homelandRoomDetail != null)
		{
			zbUIMgr.Instance.ShowRoomDetail(homelandRoomDetail);
		}
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom();
	}

	public bool IsRadarNewEvent(int gridId)
	{
		foreach (WorldEventDetail radarEvent in m_radarEventList)
		{
			if (radarEvent.GridId == gridId)
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveRadarEvent(int gridId)
	{
		m_radarEventList.RemoveAll((WorldEventDetail e) => e.GridId == gridId);
	}

	private void OnEventRadarAck(object data)
	{
		m_radarEventList.Clear();
		RepeatedField<WorldEventDetail> oldEvents = zbServerData.sGameData.WorldInfo.EventInfo.EventList;
		RepeatedField<WorldEventDetail> eventList = zbSelf.Instance.worldHandler.worldEventRadarAckData.WorldInfo.EventInfo.EventList;
		m_radarEventList = ((IEnumerable<WorldEventDetail>)eventList).Where((WorldEventDetail ne) => !((IEnumerable<WorldEventDetail>)oldEvents).Any((WorldEventDetail oe) => oe.GridId == ne.GridId)).ToList();
		SetPanelVisible(vis: false);
		((MonoBehaviour)this).StartCoroutine(SpineAnimation());
	}

	private IEnumerator SpineAnimation()
	{
		zbAudioMgr.Instance.PlayAudio("audio/ui/saomiao.ogg");
		zbUIMgr.Instance.ShowModel();
		((VisualElement)m_spineRadar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spineRadar.AnimationState.SetAnimation(0, "animation", false);
		yield return (object)new WaitForSeconds(1f);
		zbSelf.Instance.homelandHandler.UpdateHomelandInfo(zbSelf.Instance.worldHandler.worldEventRadarAckData.HomelandInfo);
		zbSelf.Instance.worldHandler.UpdateWorldInfo(zbSelf.Instance.worldHandler.worldEventRadarAckData.WorldInfo);
		zbSelf.Instance.worldHandler.worldEventRadarAckData = null;
		yield return (object)new WaitForSeconds(13f / 15f);
		((VisualElement)m_spineRadar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbUIMgr.Instance.HideModel();
	}
}
