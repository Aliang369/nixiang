using System;
using System.Collections.Generic;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbDungeonMapWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private zbDungeonHandler m_dungeonbaseHandler;

	private zbDungeonMapCanvas m_mapCanvas;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		AddDark();
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_mapCanvas = UQueryExtensions.Q<zbDungeonMapCanvas>(m_RootElement, (string)null, (string)null);
	}

	public void RefreshCustom(zbDungeonHandler dungeonBaseHandler)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		m_dungeonbaseHandler = dungeonBaseHandler;
		Dictionary<Vector2Int, DungeonMapCellDetail> dictionary = new Dictionary<Vector2Int, DungeonMapCellDetail>();
		Vector2Int key = default(Vector2Int);
		foreach (DungeonMapCellDetail cell in dungeonBaseHandler.dungeonMapInfo.CellList)
		{
			((Vector2Int)(ref key))._002Ector(cell.X, cell.Y);
			dictionary[key] = cell;
		}
		Vector2Int? playerPos = null;
		if (!string.IsNullOrEmpty(m_dungeonbaseHandler.dungeonBaseInfo.RoleSiteInfo?.CellId))
		{
			string[] array = m_dungeonbaseHandler.dungeonBaseInfo.RoleSiteInfo.CellId.Split(':');
			if (array.Length == 2 && int.TryParse(array[0], out var result) && int.TryParse(array[1], out var result2))
			{
				playerPos = new Vector2Int(result, result2);
			}
		}
		m_mapCanvas.SetMap(dictionary, playerPos);
	}

	private void OnClose()
	{
		Hide();
	}
}
