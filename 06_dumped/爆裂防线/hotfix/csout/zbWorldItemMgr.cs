using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldItemMgr
{
	private zbWorldWnd m_wndParent;

	private VisualElement m_zoomContent;

	private VisualElement m_grids;

	public zbWorldGridElement worldGridElement;

	public zbWorldGridLineElement worldGridLineElement;

	public List<zbWorldItemBuilding> buildingList = new List<zbWorldItemBuilding>();

	public List<zbWorldItemRes> resList = new List<zbWorldItemRes>();

	public List<zbWorldItemEvent> eventList = new List<zbWorldItemEvent>();

	public List<zbWorldItemTeam> teamList = new List<zbWorldItemTeam>();

	public zbWorldItemDungeon itemDungeon;

	public zbWorldItemGuide itemGuide;

	private float m_cellWidth = 140f;

	private float m_cellHeight = 140f;

	public void Init(zbWorldWnd wndParent, VisualElement zoom)
	{
		m_wndParent = wndParent;
		m_zoomContent = zoom;
		m_grids = UQueryExtensions.Q<VisualElement>(m_zoomContent, "grids", (string)null);
		InitGridAndLineElement();
		itemGuide = new zbWorldItemGuide(m_wndParent);
		m_zoomContent.Add((VisualElement)(object)itemGuide);
		Refresh();
	}

	private void InitGridAndLineElement()
	{
		int maxCol = zbServerData.sGameData.WorldInfo.WorldMapInfo.MaxCol;
		int maxRow = zbServerData.sGameData.WorldInfo.WorldMapInfo.MaxRow;
		if (worldGridElement == null)
		{
			worldGridElement = new zbWorldGridElement(maxCol, maxRow, m_cellWidth, m_cellHeight);
			m_grids.Add((VisualElement)(object)worldGridElement);
		}
		((VisualElement)worldGridElement).Clear();
		worldGridElement.StartDraw();
		if (worldGridLineElement == null)
		{
			worldGridLineElement = new zbWorldGridLineElement(maxCol, maxRow, m_cellWidth, m_cellHeight, this);
			m_grids.Add((VisualElement)(object)worldGridLineElement);
		}
		worldGridLineElement.ClearFinishedAnimations();
	}

	public void RefreshLines()
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		worldGridLineElement.ClearFinishedAnimations();
		foreach (WorldReconDetail recon in zbServerData.sGameData.WorldInfo.ReconList)
		{
			WorldMapGridDetail worldMapGridDetailByGridId = zbSelf.Instance.worldHandler.GetWorldMapGridDetailByGridId(recon.GridId);
			if (worldMapGridDetailByGridId != null && recon.State == EWorldReconState.Explore && !worldGridLineElement.IsReconDrawing(recon.ReconId) && zbSelf.Instance.newTagHandler.IsNewGridId(worldMapGridDetailByGridId.GridId.ToString()))
			{
				zbSelf.Instance.newTagHandler.SetNewGridIdViewed(worldMapGridDetailByGridId.GridId.ToString());
				worldGridLineElement.AnimateAddLine(0, 0, worldMapGridDetailByGridId.X, worldMapGridDetailByGridId.Y, new Color(7f / 51f, 25f / 51f, 0.22745098f), 4f, recon.ReconId);
			}
		}
	}

	public bool IsReconLineDrawing(int reconId)
	{
		if (worldGridLineElement != null && worldGridLineElement.IsReconDrawing(reconId))
		{
			return true;
		}
		return false;
	}

	public void OnGridTouch(WorldMapGridDetail gridDetail)
	{
		WorldEventDetail dungeonEventDetail = zbSelf.Instance.worldHandler.GetDungeonEventDetail();
		if (itemDungeon != null && dungeonEventDetail != null && dungeonEventDetail.GridId == gridDetail.GridId)
		{
			itemDungeon.OnTouch();
			return;
		}
		foreach (zbWorldItemTeam team in teamList)
		{
			if (team.gridDetail != null && team.gridDetail.GridId == gridDetail.GridId)
			{
				team.OnTouch();
				return;
			}
		}
		if (gridDetail.State == EWorldMapGridState.Explorable)
		{
			zbUIMgr.Instance.ShowWorldReconExplore(gridDetail);
			return;
		}
		foreach (zbWorldItemBuilding building in buildingList)
		{
			if (building.gridDetail.GridId == gridDetail.GridId)
			{
				building.OnTouch();
				return;
			}
		}
		foreach (zbWorldItemRes res in resList)
		{
			if (res.gridDetail.GridId == gridDetail.GridId)
			{
				res.OnTouch();
				return;
			}
		}
		foreach (zbWorldItemEvent @event in eventList)
		{
			if (@event.gridDetail.GridId == gridDetail.GridId)
			{
				@event.OnTouch();
				return;
			}
		}
		if (gridDetail.State != EWorldMapGridState.Explored)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10494"));
		}
	}

	public void Refresh()
	{
		RefreshGrids();
		RefreshBuildingList();
		RefreshResList();
		RefreshEventList();
		RefreshDungeonItem();
		RefreshLines();
		RefreshTeamList();
	}

	public void Update()
	{
		foreach (zbWorldItemTeam team in teamList)
		{
			team.Update();
		}
	}

	private void RefreshGrids()
	{
		WorldReconDetail worldReconDetail = zbSelf.Instance.worldHandler.GetWorldReconDetail(1);
		WorldReconDetail worldReconDetail2 = zbSelf.Instance.worldHandler.GetWorldReconDetail(2);
		foreach (WorldMapGridDetail grid in zbServerData.sGameData.WorldInfo.WorldMapInfo.GridList)
		{
			zbWorldGridElement.MapCellState state = zbWorldGridElement.MapCellState.Locked;
			if (worldReconDetail != null && worldReconDetail.GridId == grid.GridId)
			{
				if (worldReconDetail.State == EWorldReconState.Explore || worldReconDetail.State == EWorldReconState.Explored)
				{
					state = zbWorldGridElement.MapCellState.ToExplore;
				}
			}
			else if (worldReconDetail2 != null && worldReconDetail2.GridId == grid.GridId)
			{
				if (worldReconDetail2.State == EWorldReconState.Explore || worldReconDetail2.State == EWorldReconState.Explored)
				{
					state = zbWorldGridElement.MapCellState.ToExplore;
				}
			}
			else if (grid.State == EWorldMapGridState.Explored)
			{
				state = zbWorldGridElement.MapCellState.Unlocked;
			}
			else if (grid.State == EWorldMapGridState.Explorable)
			{
				state = zbWorldGridElement.MapCellState.ToExplore;
			}
			bool showBorder = zbSelf.Instance.worldHandler.IsShowBorder(grid.X, grid.Y);
			worldGridElement.SetCellState(grid.X, grid.Y, state, showBorder);
		}
		worldGridElement.StartDraw();
	}

	private void RefreshBuildingList()
	{
		WorldMapGridDetail[] newDataList = zbSelf.Instance.worldHandler.GetMapGridBuildingList();
		foreach (zbWorldItemBuilding item in buildingList.Where((zbWorldItemBuilding item) => !newDataList.Any((WorldMapGridDetail worldMapGridDetail) => worldMapGridDetail.GridId == item.gridDetail.GridId)).ToList())
		{
			m_zoomContent.Remove((VisualElement)(object)item);
			buildingList.Remove(item);
		}
		WorldMapGridDetail[] array = newDataList;
		foreach (WorldMapGridDetail data in array)
		{
			zbWorldItemBuilding zbWorldItemBuilding2 = buildingList.FirstOrDefault((zbWorldItemBuilding item) => item.gridDetail.GridId == data.GridId);
			if (zbWorldItemBuilding2 != null)
			{
				zbWorldItemBuilding2.Refresh();
			}
			else
			{
				CreateBuildingItem(data);
			}
		}
	}

	private void RefreshResList()
	{
		WorldMapGridDetail[] newDataList = zbSelf.Instance.worldHandler.GetMapGridResList();
		foreach (zbWorldItemRes item in resList.Where((zbWorldItemRes item) => !newDataList.Any((WorldMapGridDetail worldMapGridDetail) => worldMapGridDetail.GridId == item.gridDetail.GridId)).ToList())
		{
			m_zoomContent.Remove((VisualElement)(object)item);
			resList.Remove(item);
		}
		WorldMapGridDetail[] array = newDataList;
		foreach (WorldMapGridDetail data in array)
		{
			zbWorldItemRes zbWorldItemRes2 = resList.FirstOrDefault((zbWorldItemRes item) => item.gridDetail.GridId == data.GridId);
			if (zbWorldItemRes2 != null)
			{
				zbWorldItemRes2.Refresh();
			}
			else
			{
				CreateResItem(data);
			}
		}
	}

	private void RefreshEventList()
	{
		WorldMapGridDetail[] newDataList = zbSelf.Instance.worldHandler.GetMapGridEventList();
		foreach (zbWorldItemEvent item in eventList.Where((zbWorldItemEvent item) => !newDataList.Any((WorldMapGridDetail worldMapGridDetail) => worldMapGridDetail.GridId == item.gridDetail.GridId)).ToList())
		{
			m_zoomContent.Remove((VisualElement)(object)item);
			eventList.Remove(item);
		}
		WorldMapGridDetail[] array = newDataList;
		foreach (WorldMapGridDetail data in array)
		{
			zbWorldItemEvent zbWorldItemEvent2 = eventList.FirstOrDefault((zbWorldItemEvent item) => item.gridDetail.GridId == data.GridId);
			if (zbWorldItemEvent2 != null)
			{
				zbWorldItemEvent2.Refresh();
			}
			else
			{
				CreateEventItem(data);
			}
		}
	}

	public void RefreshTeamList()
	{
		RepeatedField<WorldReconDetail> newDataList = zbServerData.sGameData.WorldInfo.ReconList;
		foreach (zbWorldItemTeam item in teamList.Where((zbWorldItemTeam item) => !((IEnumerable<WorldReconDetail>)newDataList).Any((WorldReconDetail worldReconDetail) => worldReconDetail.ReconId == item.reconDetail.ReconId)).ToList())
		{
			m_zoomContent.Remove((VisualElement)(object)item);
			teamList.Remove(item);
		}
		foreach (WorldReconDetail data in newDataList)
		{
			zbWorldItemTeam zbWorldItemTeam2 = teamList.FirstOrDefault((zbWorldItemTeam item) => item.reconDetail.ReconId == data.ReconId);
			if (zbWorldItemTeam2 != null)
			{
				zbWorldItemTeam2.Refresh();
			}
			else
			{
				CreateTeamItem(data);
			}
		}
	}

	private void RefreshDungeonItem()
	{
		if (itemDungeon == null)
		{
			itemDungeon = new zbWorldItemDungeon(m_wndParent);
			m_zoomContent.Add((VisualElement)(object)itemDungeon);
		}
		itemDungeon.Refresh();
	}

	private void CreateBuildingItem(WorldMapGridDetail data)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		zbWorldItemBuilding zbWorldItemBuilding2 = new zbWorldItemBuilding(m_wndParent, data);
		buildingList.Add(zbWorldItemBuilding2);
		m_zoomContent.Add((VisualElement)(object)zbWorldItemBuilding2);
		((VisualElement)zbWorldItemBuilding2).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		var (num, num2) = zbSelf.Instance.worldHandler.GetColAndRow(data);
		((VisualElement)zbWorldItemBuilding2).style.left = StyleLength.op_Implicit((float)num * m_cellWidth - m_cellWidth);
		((VisualElement)zbWorldItemBuilding2).style.top = StyleLength.op_Implicit((float)num2 * m_cellHeight - m_cellHeight);
		zbWorldItemBuilding2.Refresh();
	}

	private void CreateResItem(WorldMapGridDetail data)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		zbWorldItemRes zbWorldItemRes2 = new zbWorldItemRes(m_wndParent, data);
		resList.Add(zbWorldItemRes2);
		m_zoomContent.Add((VisualElement)(object)zbWorldItemRes2);
		((VisualElement)zbWorldItemRes2).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		var (num, num2) = zbSelf.Instance.worldHandler.GetColAndRow(data);
		((VisualElement)zbWorldItemRes2).style.left = StyleLength.op_Implicit((float)num * m_cellWidth - m_cellWidth);
		((VisualElement)zbWorldItemRes2).style.top = StyleLength.op_Implicit((float)num2 * m_cellHeight - m_cellHeight);
		zbWorldItemRes2.Refresh();
	}

	private void CreateEventItem(WorldMapGridDetail data)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		zbWorldItemEvent zbWorldItemEvent2 = new zbWorldItemEvent(m_wndParent, data);
		eventList.Add(zbWorldItemEvent2);
		m_zoomContent.Add((VisualElement)(object)zbWorldItemEvent2);
		((VisualElement)zbWorldItemEvent2).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		var (num, num2) = zbSelf.Instance.worldHandler.GetColAndRow(data);
		((VisualElement)zbWorldItemEvent2).style.left = StyleLength.op_Implicit((float)num * m_cellWidth - m_cellWidth);
		((VisualElement)zbWorldItemEvent2).style.top = StyleLength.op_Implicit((float)num2 * m_cellHeight - m_cellHeight);
		zbWorldItemEvent2.Refresh();
	}

	private void CreateTeamItem(WorldReconDetail data)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		zbWorldItemTeam zbWorldItemTeam2 = new zbWorldItemTeam(m_wndParent, this, data);
		teamList.Add(zbWorldItemTeam2);
		m_zoomContent.Add((VisualElement)(object)zbWorldItemTeam2);
		((VisualElement)zbWorldItemTeam2).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		WorldMapGridDetail worldMapGridDetailByGridId = zbSelf.Instance.worldHandler.GetWorldMapGridDetailByGridId(1);
		if (worldMapGridDetailByGridId != null)
		{
			var (num, num2) = zbSelf.Instance.worldHandler.GetColAndRow(worldMapGridDetailByGridId);
			((VisualElement)zbWorldItemTeam2).style.left = StyleLength.op_Implicit((float)num * m_cellWidth - m_cellWidth);
			((VisualElement)zbWorldItemTeam2).style.top = StyleLength.op_Implicit((float)num2 * m_cellHeight - m_cellHeight);
		}
		zbWorldItemTeam2.Refresh();
	}

	public zbWorldItemRes GetResItem(int buildId)
	{
		foreach (zbWorldItemRes res in resList)
		{
			if (res.gridDetail.BuildingId == buildId)
			{
				return res;
			}
		}
		return null;
	}
}
