using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;

public class zbDungeonFloor : MonoBehaviour
{
	[NonSerialized]
	private GameObject m_prefabRoom;

	public int floor;

	public List<zbDungeonRoom> rooms = new List<zbDungeonRoom>();

	public List<DungeonMapCellDetail> dungeonMapCellDetails;

	private void Awake()
	{
		m_prefabRoom = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/dungeon/dungeonroom.prefab");
	}

	public void Create(int floor)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		this.floor = floor;
		((Component)this).transform.localPosition = new Vector3(0f, (float)(floor - 1) * 2.2f, 0f);
		((Component)this).transform.localScale = Vector3.one;
	}

	public void Build(List<DungeonMapCellDetail> cells)
	{
		dungeonMapCellDetails = cells;
		ClearRoom();
		foreach (DungeonMapCellDetail dungeonMapCellDetail in dungeonMapCellDetails)
		{
			if (dungeonMapCellDetail.State != EDungeonMapCellState.Invisible)
			{
				CreateRoom(dungeonMapCellDetail);
			}
		}
	}

	public void CreateRoom(DungeonMapCellDetail cellDetail)
	{
		zbDungeonRoom zbDungeonRoom2 = GetDungeonRoom(cellDetail.CellId);
		if ((Object)(object)zbDungeonRoom2 == (Object)null)
		{
			zbDungeonRoom2 = Object.Instantiate<GameObject>(m_prefabRoom).GetComponent<zbDungeonRoom>();
			((Component)zbDungeonRoom2).transform.SetParent(((Component)this).transform);
			rooms.Add(zbDungeonRoom2);
		}
		zbDungeonRoom2.Create(this, cellDetail);
	}

	public zbDungeonRoom GetDungeonRoom(string cellId)
	{
		foreach (zbDungeonRoom room in rooms)
		{
			if (room.dungeonMapCellDetail.CellId.Equals(cellId))
			{
				return room;
			}
		}
		return null;
	}

	private void ClearRoom()
	{
		if (dungeonMapCellDetails == null)
		{
			return;
		}
		HashSet<string> hashSet = new HashSet<string>(from cell in dungeonMapCellDetails
			where cell.State != EDungeonMapCellState.Invisible
			select cell.CellId);
		for (int num = rooms.Count - 1; num >= 0; num--)
		{
			zbDungeonRoom zbDungeonRoom2 = rooms[num];
			if (!hashSet.Contains(zbDungeonRoom2.dungeonMapCellDetail.CellId))
			{
				Object.Destroy((Object)(object)((Component)zbDungeonRoom2).gameObject);
				rooms.RemoveAt(num);
			}
		}
	}

	public void ClearAllRooms()
	{
		foreach (zbDungeonRoom room in rooms)
		{
			Object.Destroy((Object)(object)((Component)room).gameObject);
		}
		rooms.Clear();
	}
}
