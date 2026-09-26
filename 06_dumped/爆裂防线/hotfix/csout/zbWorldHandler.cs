using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;

public class zbWorldHandler
{
	private long lastTeamServerTm;

	private bool hasTeamRequested;

	public WorldEventRadarAck worldEventRadarAckData;

	public int curReconId;

	public int worldResIdleSettleSecond;

	private static readonly int[] kNeighborOffsets = new int[16]
	{
		1, 0, -1, 0, 0, 1, 0, -1, 1, 1,
		1, -1, -1, 1, -1, -1
	};

	private Dictionary<long, WorldMapGridDetail> m_gridByXYCache;

	private Dictionary<int, WorldMapGridDetail> m_gridByIdCache;

	private IList<WorldMapGridDetail> m_gridCacheSource;

	private int m_gridCacheCount = -1;

	public void Init()
	{
	}

	public void UpdateWorldInfo(WorldInfo worldInfo)
	{
		ResetRequstTime();
		zbServerData.sGameData.WorldInfo = worldInfo;
		worldResIdleSettleSecond = worldInfo.ResIdleInfo.SettleSecond;
		InvalidateWorldGridCache();
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_WorldInfo);
	}

	public bool CanWorldResIdle()
	{
		if (zbServerData.sGameData.WorldInfo != null && zbServerData.sGameData.WorldInfo.ResIdleInfo != null && zbServerData.sGameData.WorldInfo.BuildingResList.Count > 0 && ((IEnumerable<WorldBuildingResDetail>)zbServerData.sGameData.WorldInfo.BuildingResList).Any((WorldBuildingResDetail b) => b.ResLevel > 0))
		{
			return true;
		}
		return false;
	}

	public string GetStateString(EWorldReconState state)
	{
		string result = "";
		switch (state)
		{
		case EWorldReconState.EmptyValue:
			result = zbUtil.Language("lang.10229");
			break;
		case EWorldReconState.Idle:
			result = zbUtil.Language("lang.10208");
			break;
		case EWorldReconState.Explore:
			result = zbUtil.Language("lang.10209");
			break;
		case EWorldReconState.Explored:
			result = zbUtil.Language("lang.10210");
			break;
		}
		return result;
	}

	public int GetExploredCount()
	{
		int num = 0;
		foreach (WorldMapGridDetail grid in zbServerData.sGameData.WorldInfo.WorldMapInfo.GridList)
		{
			if (grid.State == EWorldMapGridState.Explored)
			{
				num++;
			}
		}
		return num;
	}

	public bool IsShowBorder(int x, int y)
	{
		for (int i = 0; i < kNeighborOffsets.Length; i += 2)
		{
			WorldMapGridDetail worldMapGridDetail = GetWorldMapGridDetail(x + kNeighborOffsets[i], y + kNeighborOffsets[i + 1]);
			if (worldMapGridDetail != null && worldMapGridDetail.State >= EWorldMapGridState.Explorable)
			{
				return true;
			}
		}
		return false;
	}

	public WorldMapGridDetail GetHomeGridDetail()
	{
		WorldMapGridDetail[] mapGridBuildingList = GetMapGridBuildingList();
		foreach (WorldMapGridDetail worldMapGridDetail in mapGridBuildingList)
		{
			zbWorldBuildingCsvRecord recordByKey = zbDataConfig.Instance.worldBuildingCsvTable.GetRecordByKey(worldMapGridDetail.BuildingId);
			if (recordByKey != null && recordByKey.type == 1)
			{
				return worldMapGridDetail;
			}
		}
		return null;
	}

	public WorldMapGridDetail[] GetMapGridBuildingList()
	{
		List<WorldMapGridDetail> list = new List<WorldMapGridDetail>();
		foreach (WorldMapGridDetail grid in zbServerData.sGameData.WorldInfo.WorldMapInfo.GridList)
		{
			if (grid.BuildingId > 0)
			{
				zbWorldBuildingCsvRecord recordByKey = zbDataConfig.Instance.worldBuildingCsvTable.GetRecordByKey(grid.BuildingId);
				if (recordByKey != null && recordByKey.type <= 2)
				{
					list.Add(grid);
				}
			}
		}
		return list.ToArray();
	}

	public WorldMapGridDetail[] GetMapGridResList()
	{
		List<WorldMapGridDetail> list = new List<WorldMapGridDetail>();
		foreach (WorldMapGridDetail grid in zbServerData.sGameData.WorldInfo.WorldMapInfo.GridList)
		{
			if (grid.BuildingId > 0)
			{
				zbWorldBuildingCsvRecord recordByKey = zbDataConfig.Instance.worldBuildingCsvTable.GetRecordByKey(grid.BuildingId);
				if (recordByKey != null && recordByKey.type == 3)
				{
					list.Add(grid);
				}
			}
		}
		return list.ToArray();
	}

	public WorldMapGridDetail[] GetMapGridEventList()
	{
		List<WorldMapGridDetail> list = new List<WorldMapGridDetail>();
		foreach (WorldMapGridDetail grid in zbServerData.sGameData.WorldInfo.WorldMapInfo.GridList)
		{
			WorldEventDetail worldEventDetail = GetWorldEventDetail(grid.GridId);
			if (worldEventDetail != null && worldEventDetail.EventType != EWorldEventType.WorldLair && worldEventDetail.EventType != EWorldEventType.Dungeon)
			{
				list.Add(grid);
			}
		}
		return list.ToArray();
	}

	public WorldEventDetail GetDungeonEventDetail()
	{
		foreach (WorldEventDetail @event in zbServerData.sGameData.WorldInfo.EventInfo.EventList)
		{
			if (@event.EventType == EWorldEventType.Dungeon)
			{
				return @event;
			}
		}
		return null;
	}

	public WorldBuildingResDetail GetWorldBuildingResDetail(int gridId)
	{
		foreach (WorldBuildingResDetail buildingRes in zbServerData.sGameData.WorldInfo.BuildingResList)
		{
			if (buildingRes.GridId == gridId)
			{
				return buildingRes;
			}
		}
		return null;
	}

	public WorldEventDetail GetWorldEventDetail(int gridId)
	{
		if (zbServerData.sGameData.WorldInfo.EventInfo == null)
		{
			return null;
		}
		foreach (WorldEventDetail @event in zbServerData.sGameData.WorldInfo.EventInfo.EventList)
		{
			if (@event.GridId == gridId)
			{
				return @event;
			}
		}
		return null;
	}

	public (int col, int row) GetColAndRow(WorldMapGridDetail gridDetail)
	{
		int item = gridDetail.X + (zbServerData.sGameData.WorldInfo.WorldMapInfo.MaxCol + 1) / 2;
		int item2 = (zbServerData.sGameData.WorldInfo.WorldMapInfo.MaxRow + 1) / 2 - gridDetail.Y;
		return (col: item, row: item2);
	}

	private static long MakeGridXYKey(int x, int y)
	{
		return (long)(((ulong)(uint)x << 32) | (uint)y);
	}

	public void InvalidateWorldGridCache()
	{
		m_gridCacheSource = null;
		m_gridCacheCount = -1;
	}

	private bool EnsureWorldGridCache()
	{
		RepeatedField<WorldMapGridDetail> val = zbServerData.sGameData.WorldInfo?.WorldMapInfo?.GridList;
		if (val == null)
		{
			return false;
		}
		if (m_gridCacheSource == val && m_gridCacheCount == val.Count && m_gridByXYCache != null && m_gridByIdCache != null)
		{
			return true;
		}
		m_gridCacheSource = (IList<WorldMapGridDetail>)val;
		m_gridCacheCount = val.Count;
		if (m_gridByXYCache == null)
		{
			m_gridByXYCache = new Dictionary<long, WorldMapGridDetail>(val.Count * 2);
		}
		else
		{
			m_gridByXYCache.Clear();
		}
		if (m_gridByIdCache == null)
		{
			m_gridByIdCache = new Dictionary<int, WorldMapGridDetail>(val.Count * 2);
		}
		else
		{
			m_gridByIdCache.Clear();
		}
		foreach (WorldMapGridDetail item in val)
		{
			m_gridByXYCache[MakeGridXYKey(item.X, item.Y)] = item;
			m_gridByIdCache[item.GridId] = item;
		}
		return true;
	}

	public WorldMapGridDetail GetWorldMapGridDetailByGridId(int gridId)
	{
		if (EnsureWorldGridCache())
		{
			m_gridByIdCache.TryGetValue(gridId, out var value);
			return value;
		}
		foreach (WorldMapGridDetail grid in zbServerData.sGameData.WorldInfo.WorldMapInfo.GridList)
		{
			if (gridId == grid.GridId)
			{
				return grid;
			}
		}
		return null;
	}

	public WorldMapGridDetail GetWorldMapGridDetail(int x, int y)
	{
		if (EnsureWorldGridCache())
		{
			m_gridByXYCache.TryGetValue(MakeGridXYKey(x, y), out var value);
			return value;
		}
		foreach (WorldMapGridDetail grid in zbServerData.sGameData.WorldInfo.WorldMapInfo.GridList)
		{
			if (x == grid.X && y == grid.Y)
			{
				return grid;
			}
		}
		return null;
	}

	public WorldReconDetail GetWorldReconDetail(int reconId)
	{
		foreach (WorldReconDetail recon in zbServerData.sGameData.WorldInfo.ReconList)
		{
			if (recon.ReconId == reconId)
			{
				return recon;
			}
		}
		return null;
	}

	public WorldReconDetail GetWorldReconDetailByGridId(int gridId)
	{
		foreach (WorldReconDetail recon in zbServerData.sGameData.WorldInfo.ReconList)
		{
			if (recon.GridId == gridId)
			{
				return recon;
			}
		}
		return null;
	}

	public WorldMapGridDetail GetCanExploreGrid()
	{
		foreach (WorldMapGridDetail grid in zbServerData.sGameData.WorldInfo.WorldMapInfo.GridList)
		{
			if (grid.State == EWorldMapGridState.Explorable && zbSelf.Instance.worldHandler.GetWorldReconDetailByGridId(grid.GridId) == null)
			{
				return grid;
			}
		}
		return null;
	}

	public bool HasWorldReconIdleNotify(int reconId)
	{
		WorldReconDetail worldReconDetail = GetWorldReconDetail(reconId);
		if (worldReconDetail != null && worldReconDetail.State == EWorldReconState.Idle && GetCanExploreGrid() != null)
		{
			return true;
		}
		return false;
	}

	public bool HasWorldReconExploredNotify(int reconId)
	{
		WorldReconDetail worldReconDetail = GetWorldReconDetail(reconId);
		if (worldReconDetail != null && worldReconDetail.State == EWorldReconState.Explored)
		{
			return true;
		}
		return false;
	}

	public bool HasWorldReconNotify(int reconId)
	{
		if (!HasWorldReconIdleNotify(reconId))
		{
			return HasWorldReconExploredNotify(reconId);
		}
		return true;
	}

	public bool HasWorldRadarNotify()
	{
		if (zbServerData.sGameData.WorldInfo.EventInfo.NextTm >= 0 && zbSelf.Instance.timeHandler.GetServerTime() >= zbServerData.sGameData.WorldInfo.EventInfo.NextTm)
		{
			return true;
		}
		return false;
	}

	public bool HasCommRadarNotify()
	{
		HomelandRoomDetail homelandRoomDetail = zbSelf.Instance.homelandHandler.GetHomelandRoomDetail(zbServerData.sGameData.HomelandInfo.BaseInfo.RoomMark.CommRadarId);
		if (homelandRoomDetail != null && zbSelf.Instance.homelandHandler.CanRoomUpgrade(homelandRoomDetail))
		{
			return true;
		}
		return false;
	}

	public bool HasResCollectNotify()
	{
		if (CanWorldResIdle() && worldResIdleSettleSecond > zbServerData.sGameData.WorldInfo.ResIdleInfo.SettleMax / 2)
		{
			return true;
		}
		return false;
	}

	public bool HasRedPoint()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Explore))
		{
			return false;
		}
		if (!HasWorldReconNotify(1) && !HasWorldReconNotify(2) && !HasWorldRadarNotify() && !HasCommRadarNotify() && !HasCommRadarNotify())
		{
			return HasResCollectNotify();
		}
		return true;
	}

	public void Update()
	{
		if (zbIntervalTimer.Instance.IsOneSecondPassed())
		{
			CheckReconTeamReq();
			if (CanWorldResIdle())
			{
				worldResIdleSettleSecond++;
			}
		}
	}

	public void CheckReconTeamReq()
	{
		if (zbGameStateMachine.Instance.IsFighting() || zbServerData.sGameData.WorldInfo == null || zbServerData.sGameData.WorldInfo.ReconList == null)
		{
			return;
		}
		foreach (WorldReconDetail recon in zbServerData.sGameData.WorldInfo.ReconList)
		{
			long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
			long stateTm = recon.StateTm;
			if (!hasTeamRequested && lastTeamServerTm < stateTm && serverTime >= stateTm && recon.State == EWorldReconState.Explore)
			{
				zbWsMsgHandler.Instance.World_Data_Req();
				hasTeamRequested = true;
			}
			lastTeamServerTm = serverTime;
			if (hasTeamRequested)
			{
				break;
			}
		}
	}

	public void ResetRequstTime()
	{
		lastTeamServerTm = 0L;
		hasTeamRequested = false;
	}
}
