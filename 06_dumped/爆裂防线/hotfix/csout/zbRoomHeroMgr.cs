using System.Collections.Generic;
using Google.Protobuf.Collections;
using Proto.Types;
using UnityEngine;

public class zbRoomHeroMgr
{
	public zbPlane plane;

	public zbElevatorTop elevatorTopLeft;

	public zbElevatorTop elevatorTopRight;

	public List<zbRoomHero> roomHeroList = new List<zbRoomHero>();

	public List<zbRoomHero> roomHeroWaitMoveList = new List<zbRoomHero>();

	private GameObject m_objScene;

	private GameObject m_prefab;

	private Transform m_frontTransform;

	private Transform m_rootTransform;

	private bool m_bInitBuild;

	public void Init(GameObject objScene)
	{
		m_objScene = objScene;
		m_prefab = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/home/roomHero.prefab");
		m_frontTransform = ((Component)m_objScene.transform.Find("homeMap")).transform.Find("front");
		m_rootTransform = m_frontTransform.Find("root_worker").Find("roomhero");
		plane = ((Component)m_frontTransform.Find("plane")).GetComponent<zbPlane>();
		elevatorTopLeft = ((Component)m_frontTransform.Find("elevator_left")).GetComponent<zbElevatorTop>();
		elevatorTopRight = ((Component)m_frontTransform.Find("elevator_right")).GetComponent<zbElevatorTop>();
	}

	public void Free()
	{
		foreach (zbRoomHero roomHero in roomHeroList)
		{
			if ((Object)(object)roomHero != (Object)null && (Object)(object)((Component)roomHero).gameObject != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)roomHero).gameObject);
			}
		}
		roomHeroList.Clear();
		foreach (zbRoomHero roomHeroWaitMove in roomHeroWaitMoveList)
		{
			if ((Object)(object)roomHeroWaitMove != (Object)null && (Object)(object)((Component)roomHeroWaitMove).gameObject != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)roomHeroWaitMove).gameObject);
			}
		}
		roomHeroWaitMoveList.Clear();
		m_prefab = null;
	}

	public void Build()
	{
		m_bInitBuild = true;
		Refresh();
		m_bInitBuild = false;
	}

	public void Refresh()
	{
		RepeatedField<WarRoomDetail> roomList = zbServerData.sGameData.WarRoomInfo.RoomList;
		if (zbSelf.Instance.guideHandler.IsFirstEnterHomeWnd())
		{
			foreach (WarRoomDetail item in roomList)
			{
				if (item.HeroTid > 0)
				{
					AddRoomHero(item, isNewUnlock: true);
					if (!zbSelf.Instance.homelandHandler.warRoomTypeList.Contains(item.SkillType))
					{
						zbSelf.Instance.homelandHandler.warRoomTypeList.Add(item.SkillType);
					}
				}
			}
			zbSelf.Instance.guideHandler.SetEnteredHomeWnd();
			return;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (zbRoomHero roomHero in roomHeroList)
		{
			if (roomHero.curWarRoomDetail != null)
			{
				dictionary[roomHero.curWarRoomDetail.RoomId] = roomHero.curHeroTid;
			}
		}
		for (int num = roomHeroList.Count - 1; num >= 0; num--)
		{
			zbRoomHero zbRoomHero2 = roomHeroList[num];
			bool flag = false;
			foreach (WarRoomDetail item2 in roomList)
			{
				if (zbRoomHero2.curWarRoomDetail?.RoomId == item2.RoomId)
				{
					flag = true;
					if (zbRoomHero2.curHeroTid != item2.HeroTid)
					{
						Object.Destroy((Object)(object)((Component)zbRoomHero2).gameObject);
						roomHeroList.RemoveAt(num);
					}
					break;
				}
			}
			if (!flag)
			{
				Object.Destroy((Object)(object)((Component)zbRoomHero2).gameObject);
				roomHeroList.RemoveAt(num);
			}
		}
		foreach (WarRoomDetail item3 in roomList)
		{
			if (item3.HeroTid > 0 && !IsLocalExist(item3.HeroTid))
			{
				bool isNewUnlock = !dictionary.TryGetValue(item3.RoomId, out var value) || value == 0;
				AddRoomHero(item3, isNewUnlock);
			}
		}
		foreach (zbRoomHero roomHero2 in roomHeroList)
		{
			foreach (WarRoomDetail item4 in roomList)
			{
				if (roomHero2.curWarRoomDetail?.RoomId == item4.RoomId)
				{
					roomHero2.Refresh(item4.HeroTid);
					break;
				}
			}
		}
	}

	public void Update()
	{
	}

	public bool IsLocalExist(int heroTid)
	{
		foreach (zbRoomHero roomHero in roomHeroList)
		{
			if (roomHero.curHeroTid == heroTid)
			{
				return true;
			}
		}
		foreach (zbRoomHero roomHeroWaitMove in roomHeroWaitMoveList)
		{
			if (roomHeroWaitMove.curHeroTid == heroTid)
			{
				return true;
			}
		}
		return false;
	}

	public void AddRoomHero(WarRoomDetail warRoomDetail, bool isNewUnlock)
	{
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		if (IsLocalExist(warRoomDetail.HeroTid))
		{
			return;
		}
		for (int num = roomHeroList.Count - 1; num >= 0; num--)
		{
			zbRoomHero zbRoomHero2 = roomHeroList[num];
			if (zbRoomHero2.curWarRoomDetail?.RoomId == warRoomDetail.RoomId)
			{
				Object.Destroy((Object)(object)((Component)zbRoomHero2).gameObject);
				roomHeroList.RemoveAt(num);
				break;
			}
		}
		zbRoomHero component = Object.Instantiate<GameObject>(m_prefab).GetComponent<zbRoomHero>();
		((Component)component).transform.SetParent(m_rootTransform);
		component.Refresh(warRoomDetail.HeroTid);
		zbRoom room = zbHomeMgr.Instance.roomMgr.GetRoom(warRoomDetail.RoomId);
		if ((m_bInitBuild && !zbSelf.Instance.guideHandler.IsFirstEnterHomeWnd()) || !isNewUnlock)
		{
			if ((Object)(object)room != (Object)null)
			{
				zbSelf.Instance.homelandHandler.warRoomTypeList.Remove(warRoomDetail.SkillType);
				room.Refresh(room.roomData);
				Vector3 centerPos = room.GetCenterPos();
				((Component)component).transform.position = centerPos;
				component.fsm.SetState(ERoomHeroState.Idle);
			}
			roomHeroList.Add(component);
		}
		else
		{
			((Component)component).transform.position = plane.GetInsidePosition() + new Vector3(0f, 0f, 210f);
			component.m_sortType = ESortMainLayerType.Back;
			roomHeroWaitMoveList.Add(component);
		}
	}

	public void RemoveRoomHero(int heroTid)
	{
		roomHeroList.RemoveAll(delegate(zbRoomHero roomHero)
		{
			if ((Object)(object)roomHero != (Object)null && (Object)(object)((Component)roomHero).gameObject != (Object)null && roomHero.curHeroTid == heroTid)
			{
				Object.Destroy((Object)(object)((Component)roomHero).gameObject);
				return true;
			}
			return false;
		});
	}
}
