using System.Collections.Generic;
using Proto.Types;
using UnityEngine;

public class zbRoomMgr
{
	public List<zbRoom> roomList = new List<zbRoom>();

	public List<zbRoomEmpty> roomEmptyList = new List<zbRoomEmpty>();

	public List<zbRoomCreate> roomCreateList = new List<zbRoomCreate>();

	private GameObject m_objScene;

	private GameObject m_prefab;

	private GameObject m_prefabEmpty;

	private GameObject m_prefabCreate;

	private Transform m_frontTransform;

	private float m_refreshStateTimer;

	private const float m_refreshStateInterval = 10f;

	public void Init(GameObject objScene)
	{
		m_objScene = objScene;
		m_prefab = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/home/room.prefab");
		m_prefabEmpty = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/home/roomEmpty.prefab");
		m_prefabCreate = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/home/roomCreate.prefab");
		m_frontTransform = ((Component)m_objScene.transform.Find("homeMap")).transform.Find("front");
	}

	public void Build()
	{
		BuildRealRooms();
		BuildEmptyRooms();
	}

	public void BuildRealRooms()
	{
		foreach (HomelandRoomDetail room in zbServerData.sGameData.HomelandInfo.RoomList)
		{
			CreateRoom(room);
		}
	}

	public void BuildEmptyRooms()
	{
		for (int num = 3; num >= -8; num--)
		{
			for (int i = 1; i <= 6; i++)
			{
				if (!IsExistRoom(i, num))
				{
					CreateRoomEmpty(i, num);
				}
			}
		}
	}

	public void Free()
	{
		foreach (zbRoom room in roomList)
		{
			if ((Object)(object)room != (Object)null && (Object)(object)((Component)room).gameObject != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)room).gameObject);
			}
		}
		roomList.Clear();
		foreach (zbRoomEmpty roomEmpty in roomEmptyList)
		{
			if ((Object)(object)roomEmpty != (Object)null && (Object)(object)((Component)roomEmpty).gameObject != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)roomEmpty).gameObject);
			}
		}
		roomEmptyList.Clear();
		foreach (zbRoomCreate roomCreate in roomCreateList)
		{
			if ((Object)(object)roomCreate != (Object)null && (Object)(object)((Component)roomCreate).gameObject != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)roomCreate).gameObject);
			}
		}
		roomCreateList.Clear();
		m_prefab = null;
		m_prefabEmpty = null;
		m_prefabCreate = null;
	}

	public void Refresh()
	{
		foreach (zbRoom item in new List<zbRoom>(roomList))
		{
			HomelandRoomDetail homelandRoomDetail = null;
			foreach (HomelandRoomDetail room in zbServerData.sGameData.HomelandInfo.RoomList)
			{
				if (room.X == item.roomData.X && room.Y == item.roomData.Y && room.RoomId == item.roomData.RoomId)
				{
					homelandRoomDetail = room;
					break;
				}
			}
			if (homelandRoomDetail != null)
			{
				item.Refresh(homelandRoomDetail);
			}
			else if (item.roomData.RoomId == zbSelf.Instance.homelandHandler.DestroyRoomId)
			{
				item.fsm.SetState(ERoomState.Destroy);
			}
			else
			{
				RemoveRoom(item.roomData.RoomId);
			}
		}
		roomList.RemoveAll((zbRoom r) => (Object)(object)r == (Object)null);
		foreach (HomelandRoomDetail roomData in zbServerData.sGameData.HomelandInfo.RoomList)
		{
			if (!roomList.Exists((zbRoom r) => r.roomData.X == roomData.X && r.roomData.Y == roomData.Y))
			{
				CreateRoom(roomData, roomData.Level > 0);
			}
		}
		foreach (zbRoomEmpty roomEmpty in roomEmptyList)
		{
			if ((Object)(object)roomEmpty != (Object)null && (Object)(object)((Component)roomEmpty).gameObject != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)roomEmpty).gameObject);
			}
		}
		roomEmptyList.Clear();
		List<(int, int)> list = new List<(int, int)>();
		for (int num = 3; num >= -8; num--)
		{
			for (int num2 = 1; num2 <= 6; num2++)
			{
				if (!IsExistRoom(num2, num))
				{
					CreateRoomEmpty(num2, num);
					list.Add((num2, num));
				}
			}
		}
		foreach (zbRoomCreate roomCreate in roomCreateList)
		{
			if ((Object)(object)roomCreate != (Object)null && (Object)(object)((Component)roomCreate).gameObject != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)roomCreate).gameObject);
			}
		}
		roomCreateList.Clear();
		zbRoomCreateData[] canCreateRoomList = zbSelf.Instance.homeEditHandler.GetCanCreateRoomList(list);
		foreach (zbRoomCreateData roomCreateData in canCreateRoomList)
		{
			CreateRoomCreate(roomCreateData);
		}
	}

	public zbRoom GetRoom(int roomId)
	{
		foreach (zbRoom room in roomList)
		{
			if (room.roomData.RoomId == roomId)
			{
				return room;
			}
		}
		return null;
	}

	public void Update()
	{
		m_refreshStateTimer += Time.unscaledDeltaTime;
		if (m_refreshStateTimer >= 10f)
		{
			m_refreshStateTimer = 0f;
			RefreshAllRoomState();
		}
	}

	public void RefreshAllRoomState()
	{
		foreach (zbRoom room in roomList)
		{
			if (!room.IsBroken)
			{
				room.RefreshRoomState();
				room.RefreshRoomHud();
			}
		}
	}

	public void CreateRoom(HomelandRoomDetail roomData, bool buildingAnimation = false)
	{
		GameObject gameObject = ((Component)m_frontTransform.Find("floor" + roomData.Y)).gameObject;
		zbRoom component = Object.Instantiate<GameObject>(m_prefab).GetComponent<zbRoom>();
		((Component)component).transform.SetParent(gameObject.transform);
		((Object)component).name = "room_" + roomData.Y + "_" + roomData.X;
		component.Refresh(roomData);
		if (buildingAnimation)
		{
			component.fsm.SetState(ERoomState.Building);
		}
		roomList.Add(component);
	}

	public void CreateRoomEmpty(int x, int y)
	{
		if (!IsMustNull(x, y))
		{
			GameObject gameObject = ((Component)m_frontTransform.Find("floor" + y)).gameObject;
			zbRoomEmpty component = Object.Instantiate<GameObject>(m_prefabEmpty).GetComponent<zbRoomEmpty>();
			((Component)component).transform.SetParent(gameObject.transform);
			component.Refresh(x, y);
		}
	}

	public bool IsMustNull(int x, int y)
	{
		zbHomeFloorCsvRecord recordByKey = zbDataConfig.Instance.homeFloorCsvTable.GetRecordByKey(y);
		if (recordByKey != null)
		{
			if ((x != 1 || recordByKey.block1 >= 0) && (x != 2 || recordByKey.block2 >= 0) && (x != 3 || recordByKey.block3 >= 0) && (x != 4 || recordByKey.block4 >= 0) && (x != 5 || recordByKey.block5 >= 0))
			{
				if (x == 6)
				{
					return recordByKey.block6 < 0;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public void CreateRoomCreate(zbRoomCreateData roomCreateData)
	{
		GameObject gameObject = ((Component)m_frontTransform.Find("floor" + roomCreateData.y)).gameObject;
		zbRoomCreate component = Object.Instantiate<GameObject>(m_prefabCreate).GetComponent<zbRoomCreate>();
		((Component)component).transform.SetParent(gameObject.transform);
		((Object)component).name = "room_" + roomCreateData.y + "_" + roomCreateData.x;
		component.Refresh(roomCreateData);
	}

	public bool IsExistRoom(HomelandRoomDetail testRoomData)
	{
		return IsExistRoom(testRoomData.X, testRoomData.Y);
	}

	public bool IsExistRoom(int X, int Y)
	{
		foreach (HomelandRoomDetail room in zbServerData.sGameData.HomelandInfo.RoomList)
		{
			if (room.Y == Y)
			{
				zbHomeRoomTypeCsvRecord recordByKey = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(room.TypeId);
				int x = room.X;
				int num = room.X + recordByKey.length - 1;
				if (X >= x && X <= num)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void RefreshAllRooms(HomelandRoomDetail testRoomData)
	{
		foreach (zbRoom room in roomList)
		{
			Object.Destroy((Object)(object)((Component)room).gameObject);
		}
		Build();
	}

	public void RemoveRoom(int roomId)
	{
		roomList.RemoveAll(delegate(zbRoom room)
		{
			if ((Object)(object)room != (Object)null && (Object)(object)((Component)room).gameObject != (Object)null && room.roomData.RoomId == roomId)
			{
				zbHomeMgr.Instance.leaverMgr.OnRoomDestroy();
				zbHomeMgr.Instance.workerMgr.OnRoomDestroy();
				Object.Destroy((Object)(object)((Component)room).gameObject);
				return true;
			}
			return false;
		});
	}

	public void UpdateHudWhenCameraMove()
	{
		foreach (zbRoom room in roomList)
		{
			room.roomHud.Update();
		}
	}

	public zbRoom GetRoomDating()
	{
		return GetRoom(zbServerData.sGameData.HomelandInfo.BaseInfo.RoomMark.HallId);
	}

	public zbRoom GetRoomByXY(int roomX, int roomY)
	{
		foreach (zbRoom room in roomList)
		{
			if (room.roomData.X == roomX && room.roomData.Y == roomY)
			{
				return room;
			}
		}
		return null;
	}

	public zbFloor GetFloorByPosition(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		float num = 1.8f;
		float y = position.y;
		foreach (zbFloor floor in zbHomeMgr.Instance.floorMgr.floors)
		{
			float y2 = ((Component)floor).transform.position.y;
			float num2 = num / 2f;
			if (y >= y2 - num2 && y <= y2 + num2)
			{
				return floor;
			}
		}
		return null;
	}

	public zbRoom GetRoomByPosition(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		zbFloor floorByPosition = GetFloorByPosition(position);
		if ((Object)(object)floorByPosition == (Object)null)
		{
			return null;
		}
		foreach (zbRoom room in roomList)
		{
			if (room.floor.floorIndex == floorByPosition.floorIndex && IsInsideRoom(room, position))
			{
				return room;
			}
		}
		return null;
	}

	private bool IsInsideRoom(zbRoom room, Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position2 = ((Component)room).transform.position;
		float num = room.GetRoomWidth() / 2f;
		float num2 = position2.x - num;
		float num3 = position2.x + num;
		float num4 = 0.9f;
		float num5 = position2.y - num4;
		float num6 = position2.y + num4;
		bool num7 = position.x >= num2 && position.x <= num3;
		bool flag = position.y >= num5 && position.y <= num6;
		return num7 & flag;
	}
}
