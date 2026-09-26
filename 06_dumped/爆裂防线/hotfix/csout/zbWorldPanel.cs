using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbWorldPanel
{
	private zbWorldWnd m_wndParent;

	private VisualElement m_RootElement;

	private ScrollView m_listView;

	private Button m_btnLeft;

	public void OnEnable(zbWorldWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_listView = UQueryExtensions.Q<ScrollView>(m_RootElement, "listView", (string)null);
		m_btnLeft = UQueryExtensions.Q<Button>(m_RootElement, "btn_left", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLeft, (Action)OnLeft, "", (TrickleDown)0);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_WorldEventAwardBox_Ack, OnEventUpdate);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_Update_WorldInfo, OnEventUpdate);
	}

	public void Refresh()
	{
		((VisualElement)m_listView).Clear();
		zbWorldPanelItemTitle zbWorldPanelItemTitle2 = new zbWorldPanelItemTitle("ui/image/world/world_panel1.png", zbUtil.Language("lang.10450"));
		((VisualElement)m_listView).Add((VisualElement)(object)zbWorldPanelItemTitle2);
		for (int i = 0; i < 2; i++)
		{
			zbWorldPanelItemRecon zbWorldPanelItemRecon2 = new zbWorldPanelItemRecon(m_wndParent);
			zbWorldPanelItemRecon2.Refresh(i + 1);
			((VisualElement)m_listView).Add((VisualElement)(object)zbWorldPanelItemRecon2);
		}
		zbWorldPanelItemTitle zbWorldPanelItemTitle3 = new zbWorldPanelItemTitle("ui/image/world/world_panel2.png", zbUtil.Language("lang.10482"));
		((VisualElement)m_listView).Add((VisualElement)(object)zbWorldPanelItemTitle3);
		WorldMapGridDetail[] panelResList = GetPanelResList();
		foreach (WorldMapGridDetail gridDetail in panelResList)
		{
			zbWorldPanelItemRes zbWorldPanelItemRes2 = new zbWorldPanelItemRes(m_wndParent);
			zbWorldPanelItemRes2.Refresh(gridDetail);
			((VisualElement)m_listView).Add((VisualElement)(object)zbWorldPanelItemRes2);
		}
	}

	public WorldMapGridDetail[] GetPanelResList()
	{
		List<WorldMapGridDetail> list = new List<WorldMapGridDetail>();
		foreach (WorldMapGridDetail grid in zbServerData.sGameData.WorldInfo.WorldMapInfo.GridList)
		{
			if (grid.BuildingId > 0 && grid.State == EWorldMapGridState.Explored)
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

	public void OnLeft()
	{
		m_wndParent.SetPanelVisible(vis: false);
		zbSetSave.Instance.SetWorldPanelOpen(val: false);
	}

	private void OnEventUpdate(object data)
	{
		Refresh();
	}
}
