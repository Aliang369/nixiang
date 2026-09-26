using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using Spine.Unity;
using UnityEngine;

public class zbDungeonMgr
{
	private static zbDungeonMgr sInstance;

	public zbDungeonHero hero;

	public List<zbDungeonFloor> floors;

	public zbDungeonWnd dungeonWnd;

	private zbDungeonMapCinema m_dungeonMapCinema;

	private GameObject m_objScene;

	private GameObject m_rootFloor;

	private GameObject m_prefabFloor;

	private GameObject m_bossComing;

	public Dictionary<int, List<DungeonMapCellDetail>> cellDictionary;

	public zbDungeonHandler dungeonHandler;

	public static zbDungeonMgr Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbDungeonMgr();
			}
			return sInstance;
		}
	}

	public void Init(zbDungeonWnd _dungeonWnd)
	{
		floors = new List<zbDungeonFloor>();
		cellDictionary = new Dictionary<int, List<DungeonMapCellDetail>>();
		dungeonWnd = _dungeonWnd;
		m_objScene = _dungeonWnd.GetMyGameObjectSceneRoot();
		m_rootFloor = ((Component)m_objScene.transform.Find("root_floor")).gameObject;
		m_prefabFloor = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/dungeon/dungeonfloor.prefab");
		m_dungeonMapCinema = m_objScene.GetComponent<zbDungeonMapCinema>();
		m_bossComing = zbUtil.FindGameObjectNoRecursive(((Component)_dungeonWnd).transform, "SpineBossComing");
		m_bossComing.SetActive(false);
		GameObject gameObject = ((Component)m_objScene.transform.Find("root_hero")).gameObject;
		GameObject val = Object.Instantiate<GameObject>(zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/dungeon/dungeonhero.prefab"));
		hero = val.GetComponent<zbDungeonHero>();
		((Component)hero).transform.SetParent(gameObject.transform);
		hero.Init();
	}

	public void ResetDungeonInfo()
	{
		if ((Object)(object)hero != (Object)null)
		{
			hero.ResetRoleInfo();
		}
	}

	public void Free()
	{
	}

	public void Refresh(zbDungeonHandler _dungeonHandler)
	{
		dungeonHandler = _dungeonHandler;
		Build();
	}

	public void Build()
	{
		cellDictionary.Clear();
		ClearFloor();
		foreach (DungeonMapCellDetail cell in dungeonHandler.dungeonMapInfo.CellList)
		{
			if (!cellDictionary.TryGetValue(cell.Y, out var value))
			{
				value = new List<DungeonMapCellDetail>();
				cellDictionary[cell.Y] = value;
			}
			value.Add(cell);
		}
		foreach (KeyValuePair<int, List<DungeonMapCellDetail>> item in cellDictionary)
		{
			CreateFloor(item.Key, item.Value);
		}
		CreateHero();
	}

	public void CreateFloor(int floor, List<DungeonMapCellDetail> cells)
	{
		zbDungeonFloor zbDungeonFloor2 = GetDungeonFloorByFloor(floor);
		if ((Object)(object)zbDungeonFloor2 == (Object)null)
		{
			zbDungeonFloor2 = Object.Instantiate<GameObject>(m_prefabFloor).GetComponent<zbDungeonFloor>();
			((Component)zbDungeonFloor2).transform.SetParent(m_rootFloor.transform);
			((Object)zbDungeonFloor2).name = "floor_" + floor;
			floors.Add(zbDungeonFloor2);
			zbDungeonFloor2.Create(floor);
		}
		zbDungeonFloor2.Build(cells);
	}

	public void CreateHero()
	{
		hero.Refresh();
		m_dungeonMapCinema.SetFollowTarget(((Component)hero).transform);
	}

	public Vector2 GetHeroUiPosition()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return zbUtil.ScreenToUIToolkit(Vector2.op_Implicit(zbCameraHelper.Instance.mainCamera.WorldToScreenPoint(hero.targetPosition)));
	}

	public zbDungeonFloor GetDungeonFloorByFloor(int floor)
	{
		return floors.FirstOrDefault((zbDungeonFloor f) => f.floor == floor);
	}

	public void ClearFloor()
	{
		HashSet<int> hashSet = new HashSet<int>(((IEnumerable<DungeonMapCellDetail>)dungeonHandler.dungeonMapInfo.CellList).Select((DungeonMapCellDetail cell) => cell.Y));
		for (int num = floors.Count - 1; num >= 0; num--)
		{
			zbDungeonFloor zbDungeonFloor2 = floors[num];
			if (!hashSet.Contains(zbDungeonFloor2.floor))
			{
				Object.Destroy((Object)(object)((Component)zbDungeonFloor2).gameObject);
				zbDungeonFloor2.ClearAllRooms();
				floors.RemoveAt(num);
			}
		}
	}

	public zbDungeonRoom GetNextElevatorRoom()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)hero == (Object)null || (Object)(object)hero.currentRoom == (Object)null)
		{
			return null;
		}
		if (hero.currentRoom.IsElevator())
		{
			DungeonMapCellDetail dungeonMapCellDetail = hero.currentRoom.dungeonMapCellDetail;
			int gridId = ((!(hero.heroMoveDir == Vector3.up)) ? (dungeonHandler.GetMinGridId(dungeonMapCellDetail) - 25) : (dungeonHandler.GetMinGridId(dungeonMapCellDetail) + 25));
			return GetRoomByGridId(gridId);
		}
		return null;
	}

	public zbDungeonRoom GetNextRoom(zbDungeonRoom room, bool isLeft)
	{
		int gridId = ((!isLeft) ? (dungeonHandler.GetMaxGridId(room.dungeonMapCellDetail) + 1) : (dungeonHandler.GetMinGridId(room.dungeonMapCellDetail) - 1));
		return GetRoomByGridId(gridId);
	}

	public zbDungeonRoom GetRoomByGridId(int gridId)
	{
		return floors.SelectMany((zbDungeonFloor f) => f.rooms).FirstOrDefault((zbDungeonRoom r) => dungeonHandler.IsGridInRoom(r.dungeonMapCellDetail, gridId));
	}

	public zbDungeonRoom GetRoomByCellId(string cellId)
	{
		return floors.SelectMany((zbDungeonFloor f) => f.rooms).FirstOrDefault((zbDungeonRoom r) => r.dungeonMapCellDetail?.CellId == cellId);
	}

	public zbDungeonMapCinema GetMapCamera()
	{
		return m_dungeonMapCinema;
	}

	public void OnTriggerEvent()
	{
		hero.OnTriggerEvent();
	}

	public void ActionBossComing()
	{
		if (Object.op_Implicit((Object)(object)m_bossComing))
		{
			SkeletonAnimation component = m_bossComing.GetComponent<SkeletonAnimation>();
			m_bossComing.SetActive(true);
			component.AnimationState.SetAnimation(0, "animation", false);
		}
		zbAudioMgr.Instance.PlayAudio("audio/combat/bosscoming.ogg");
	}

	public void ActionBossEnd()
	{
		if (Object.op_Implicit((Object)(object)m_bossComing))
		{
			m_bossComing.SetActive(false);
		}
	}

	public void Update()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_bossComing != (Object)null && m_bossComing.activeSelf)
		{
			Vector3 position = ((Component)zbCameraHelper.Instance.mainCamera).transform.position;
			m_bossComing.transform.position = new Vector3(position.x, position.y, position.z + 1f);
		}
	}
}
