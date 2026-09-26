using System;
using System.Collections.Generic;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbRoomHeroFollowWnd : zbBaseWnd
{
	private Button m_btnClose;

	private zbHomeWnd m_homeWnd;

	private zbRoomHero m_roomHero;

	private zbRoom m_room;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom(int heroTid)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnClose).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		foreach (KeyValuePair<EWndId, zbBaseWnd> item in zbUIMgr.Instance.m_kDictWnd)
		{
			zbBaseWnd value = item.Value;
			if (EWndId.Home != value.GetWndId() && GetWndId() != value.GetWndId() && value.IsCascadWnd() && value.IsShow())
			{
				value.Hide();
			}
		}
		m_homeWnd = zbUIMgr.Instance.GetWndById(EWndId.Home) as zbHomeWnd;
		foreach (zbRoomHero roomHero in zbHomeMgr.Instance.roomHeroMgr.roomHeroList)
		{
			if (roomHero.curHeroTid == heroTid)
			{
				m_roomHero = roomHero;
				break;
			}
		}
		foreach (zbRoomHero roomHeroWaitMove in zbHomeMgr.Instance.roomHeroMgr.roomHeroWaitMoveList)
		{
			if (roomHeroWaitMove.curHeroTid == heroTid)
			{
				m_roomHero = roomHeroWaitMove;
				break;
			}
		}
		WarRoomDetail warRoomDetailByHeroTid = zbSelf.Instance.heroHandler.GetWarRoomDetailByHeroTid(heroTid);
		foreach (zbRoom room in zbHomeMgr.Instance.roomMgr.roomList)
		{
			if (room.roomData.RoomId == warRoomDetailByHeroTid?.RoomId)
			{
				m_room = room;
				break;
			}
		}
		if ((Object)(object)m_roomHero != (Object)null)
		{
			m_homeWnd.m_MainBottom.SetVisible(vis: false);
			m_homeWnd.SetUIVisible(vis: false);
			m_homeWnd.m_homeMapCinema.SetCameraOrthographicSize(9f);
			m_homeWnd.m_homeMap.StartTracking(((Component)m_roomHero).gameObject);
		}
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		Button btnClose = m_btnClose;
		zbRoomHero roomHero = m_roomHero;
		int num;
		if (roomHero == null || roomHero.fsm?.GetStateId() != ERoomHeroState.MoveToTop)
		{
			zbRoomHero roomHero2 = m_roomHero;
			if (roomHero2 == null || roomHero2.fsm?.GetStateId() != ERoomHeroState.MoveToRoom)
			{
				zbRoomHero roomHero3 = m_roomHero;
				if (roomHero3 == null || roomHero3.fsm?.GetStateId() != ERoomHeroState.Idle)
				{
					num = 1;
					goto IL_00d9;
				}
			}
		}
		num = 0;
		goto IL_00d9;
		IL_00d9:
		zbUtil.SetDisplayIfChanged((VisualElement)(object)btnClose, (DisplayStyle)num);
		zbRoomHero roomHero4 = m_roomHero;
		if (roomHero4 != null && roomHero4.fsm?.GetStateId() == ERoomHeroState.Idle)
		{
			zbRoom room = m_room;
			if (room != null && room.fsm?.GetStateId() == ERoomState.Idle)
			{
				OnClose();
			}
		}
	}

	public void OnClose()
	{
		m_homeWnd.SetUIVisible(vis: true);
		m_homeWnd.m_homeMap.StopTracking();
		Hide();
		m_homeWnd.m_MainBottom.SetVisible(vis: true);
		m_homeWnd.SetUIVisible(vis: true);
		m_homeWnd.m_homeMapCinema.SetCameraOrthographicSize(11.6f);
		m_homeWnd.ExecGuide();
	}
}
