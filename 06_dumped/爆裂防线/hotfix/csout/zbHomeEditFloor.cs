using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHomeEditFloor
{
	public int y;

	public List<zbHomeEditRoom> roomList = new List<zbHomeEditRoom>();

	public zbHomeEditWnd wndParent;

	public VisualElement floorElement;

	public VisualElement floorElevator;

	public zbHomeEditFloor(zbHomeEditWnd _wndParent, int _y)
	{
		wndParent = _wndParent;
		y = _y;
		for (int i = 1; i <= 6; i++)
		{
			zbHomeEditRoom item = new zbHomeEditRoom(_wndParent, this, i);
			roomList.Add(item);
		}
	}

	public void OnEnable()
	{
		floorElement = UQueryExtensions.Q(wndParent.GetRootElement(), "floor" + y, (string)null);
		floorElevator = UQueryExtensions.Q<VisualElement>(floorElement, "elevator", (string)null);
		foreach (zbHomeEditRoom room in roomList)
		{
			room.OnEnable();
		}
	}

	public void Refresh()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (zbSelf.Instance.homelandHandler.GetFloorState(y) == EActiveState.Done)
		{
			floorElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			floorElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		foreach (zbHomeEditRoom room in roomList)
		{
			room.Refresh();
		}
	}

	public zbHomeEditRoom PickRoomAt(Vector2 localPos)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		foreach (zbHomeEditRoom room in roomList)
		{
			if (!zbHomeMgr.Instance.roomMgr.IsMustNull(room.x, room.y))
			{
				Rect worldBound = room.roomRootElement.worldBound;
				if (((Rect)(ref worldBound)).Contains(localPos))
				{
					return room;
				}
			}
		}
		return null;
	}
}
