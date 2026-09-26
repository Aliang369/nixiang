using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbRoomWorkerDetailWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private Label m_workerName;

	private Button m_btnGoto;

	private Button m_btnSwitch;

	private VisualElement m_noWork;

	private VisualElement m_spriteIconWork;

	private Label m_lbLeveWork;

	private VisualElement m_workState;

	private Label m_lbWorkerDesc;

	private Label m_lbWorkdesc;

	private Image m_descImg;

	private HomelandRoomDetail m_WorkRoom;

	private WorkerDetail m_workerDetail;

	protected override void OnEnable()
	{
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_workerName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_btnGoto = UQueryExtensions.Q<Button>(m_RootElement, "btn_goto", (string)null);
		m_lbWorkerDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_workState = UQueryExtensions.Q<VisualElement>(m_RootElement, "workState", (string)null);
		m_lbWorkdesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_workdesc", (string)null);
		m_btnSwitch = UQueryExtensions.Q<Button>(m_RootElement, "btn_switch", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "work", (string)null);
		m_noWork = UQueryExtensions.Q<VisualElement>(val, "roomStatusIcon", (string)null);
		m_spriteIconWork = UQueryExtensions.Q<VisualElement>(val, "icon", (string)null);
		m_lbLeveWork = UQueryExtensions.Q<Label>(val, "lb_level", (string)null);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>(m_RootElement, "nodedesc", (string)null);
		if (m_descImg == null)
		{
			m_descImg = new Image();
			((VisualElement)m_descImg).style.left = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImg).style.top = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImg).style.width = StyleLength.op_Implicit(630f);
			((VisualElement)m_descImg).style.height = StyleLength.op_Implicit(200f);
			((VisualElement)m_descImg).style.position = StyleEnum<Position>.op_Implicit((Position)1);
			val2.Add((VisualElement)(object)m_descImg);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGoto, (Action)OnGoto, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSwitch, (Action<ClickEvent>)OnSwitch, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_HomelandInfo, RefreshUI);
		m_gameEevent.RegisterEvent(EGameEvent.Event_ReplaceWorkerSelect_Ack, OnEventUpdate);
		AddDark();
	}

	private void OnSwitch(ClickEvent evt)
	{
		if (m_workerDetail != null)
		{
			zbUIMgr.Instance.ShowRoomWorkerRefresh(m_workerDetail.WorkerId);
		}
	}

	private void OnEventUpdate(object data)
	{
		OnClose();
	}

	private void RefreshUI(object obj)
	{
		if (m_workerDetail != null)
		{
			WorkerDetail workerDetail = zbSelf.Instance?.homelandHandler?.GetHomelandWorkerDetail(m_workerDetail.WorkerId);
			if (workerDetail != null)
			{
				m_workerDetail = workerDetail;
				RefreshCustom(m_workerDetail);
			}
		}
	}

	public void RefreshCustom(WorkerDetail _workerDetail)
	{
		if (_workerDetail != null)
		{
			zbWorker zbWorker2 = zbHomeMgr.Instance.workerMgr.FindWorker(_workerDetail.WorkerId);
			if ((Object)(object)zbWorker2 != (Object)null)
			{
				((Component)zbWorker2.m_skeletonAnimation).gameObject.SetActive(true);
				zbHomeMgr.Instance.renderTarget.StartRender(((Component)zbWorker2).gameObject, 630, 200);
			}
			m_workerDetail = _workerDetail;
			RefreshView();
		}
	}

	private void RefreshView()
	{
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_workerName).text = m_workerDetail?.StyleInfo?.Name ?? "???";
		((TextElement)m_lbWorkerDesc).text = zbDataConfig.Instance.npcDescCsvTable.GetRecordByKey(m_workerDetail.StyleInfo.Desc).desc;
		m_WorkRoom = null;
		foreach (HomelandRoomDetail room in zbServerData.sGameData.HomelandInfo.RoomList)
		{
			if (room.RoomId == m_workerDetail.RoomId)
			{
				m_WorkRoom = room;
			}
		}
		m_workState.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_workerDetail.RoomId == 0));
		((TextElement)m_btnGoto).text = zbUtil.Language("lang.10614");
		RefreshWorkRoom(m_WorkRoom);
	}

	private void RefreshWorkRoom(HomelandRoomDetail room)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		bool flag = room != null;
		m_noWork.visible = !flag;
		m_spriteIconWork.visible = flag;
		if (!flag)
		{
			((TextElement)m_lbLeveWork).text = "<color=#F00502>" + zbUtil.Language("lang.10196") + "</color>";
			return;
		}
		zbHomeRoomTypeCsvRecord recordByKey = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(room.TypeId);
		if (recordByKey != null)
		{
			m_spriteIconWork.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.iconType));
		}
		((TextElement)m_lbLeveWork).text = "Lv." + room.Level;
		((TextElement)m_lbWorkdesc).text = string.Format(zbUtil.Language("lang.10615"), recordByKey.name, room.Level);
	}

	private void OnGoto()
	{
		OnClose();
		zbUIMgr.Instance.ShowWorkerFollow(m_workerDetail);
	}

	private void OnClose()
	{
		zbHomeMgr.Instance.renderTarget.StopRender();
		Hide();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		if (m_descImg != null && !((Object)(object)zbHomeMgr.Instance.renderTarget == (Object)null))
		{
			m_descImg.image = (Texture)(object)zbHomeMgr.Instance.renderTarget.GetRenderTexture();
		}
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		if ((Object)(object)zbHomeMgr.Instance.renderTarget != (Object)null && m_workerDetail != null)
		{
			zbWorker zbWorker2 = zbHomeMgr.Instance.workerMgr.FindWorker(m_workerDetail.WorkerId);
			if ((Object)(object)zbWorker2 != (Object)null)
			{
				((Component)zbWorker2.m_skeletonAnimation).gameObject.SetActive(true);
				zbHomeMgr.Instance.renderTarget.StartRender(((Component)zbWorker2).gameObject, 630, 200);
			}
		}
	}
}
