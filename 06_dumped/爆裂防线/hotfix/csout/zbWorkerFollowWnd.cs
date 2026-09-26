using System;
using System.Collections.Generic;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorkerFollowWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private Label m_lbName;

	private VisualElement m_spriteColor;

	private VisualElement m_spriteIcon;

	private Label m_lbLevel;

	private WorkerDetail m_workerDetail;

	private HomelandRoomDetail m_room;

	private zbWorker m_worker;

	private readonly string m_strSpineFollowName = "workerFollowBg";

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_spriteColor = UQueryExtensions.Q<VisualElement>(m_RootElement, "color0", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>(m_spriteColor, "icon", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_spriteColor, "lb_level", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom(WorkerDetail data)
	{
		foreach (KeyValuePair<EWndId, zbBaseWnd> item in zbUIMgr.Instance.m_kDictWnd)
		{
			zbBaseWnd value = item.Value;
			if (EWndId.Home != value.GetWndId() && GetWndId() != value.GetWndId() && value.IsCascadWnd() && value.IsShow())
			{
				value.Hide();
			}
		}
		zbHomeWnd zbHomeWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Home) as zbHomeWnd;
		foreach (zbWorker worker in zbHomeMgr.Instance.workerMgr.workerList)
		{
			if (worker.m_homelandWorkerDetail.WorkerId == data.WorkerId)
			{
				m_worker = worker;
				break;
			}
		}
		if ((Object)(object)m_worker != (Object)null)
		{
			CreateFollowBg(m_worker);
			zbHomeWnd2.m_homeMapCinema.SetCameraOrthographicSize(7f);
			zbHomeWnd2.m_homeMap.StartTracking(((Component)m_worker).gameObject);
		}
		m_workerDetail = data;
		((TextElement)m_lbName).text = data.StyleInfo.Name;
		m_room = null;
		foreach (HomelandRoomDetail room in zbServerData.sGameData.HomelandInfo.RoomList)
		{
			if (room.RoomId == m_workerDetail.RoomId)
			{
				m_room = room;
			}
		}
		RefreshRoom(m_room);
	}

	private void RefreshRoom(HomelandRoomDetail room)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		zbHomeRoomTypeCsvRecord recordByKey = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(room.TypeId);
		if (recordByKey != null)
		{
			m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.iconType));
		}
		((TextElement)m_lbLevel).text = "Lv." + room.Level;
	}

	private void OnClose()
	{
		if ((Object)(object)m_worker != (Object)null)
		{
			RemoveFollowBg(m_worker);
		}
		(zbUIMgr.Instance.GetWndById(EWndId.Home) as zbHomeWnd).m_homeMap.StopTracking();
		Hide();
	}

	private void CreateFollowBg(zbWorker worker)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)worker == (Object)null))
		{
			Transform val = ((Component)worker).transform.Find(m_strSpineFollowName);
			if ((Object)(object)val != (Object)null)
			{
				((Component)val).gameObject.SetActive(true);
				return;
			}
			GameObject obj = Object.Instantiate<GameObject>(zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/home/worker/workerFollowBg.prefab"));
			((Object)obj.transform).name = m_strSpineFollowName;
			obj.transform.SetParent(((Component)worker).transform);
			obj.transform.SetAsFirstSibling();
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localRotation = Quaternion.identity;
			obj.transform.localScale = Vector3.one;
		}
	}

	private void RemoveFollowBg(zbWorker worker)
	{
		if (!((Object)(object)worker == (Object)null))
		{
			Transform val = ((Component)worker).transform.Find(m_strSpineFollowName);
			if ((Object)(object)val != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)val).gameObject);
			}
		}
	}
}
