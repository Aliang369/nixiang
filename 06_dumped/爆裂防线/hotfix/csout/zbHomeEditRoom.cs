using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHomeEditRoom
{
	public HomelandRoomDetail roomData;

	public zbHomeEditWnd wndParent;

	public zbHomeEditFloor floor;

	public int y;

	public int x;

	public VisualElement roomRootElement;

	public VisualElement roomBgElement;

	public VisualElement roomFgElement;

	public VisualElement roomFrameElement;

	private VisualElement m_spriteColor;

	private VisualElement m_spriteIcon;

	private Label m_lbLevel;

	private Label m_lbName;

	private Button m_btnNo;

	private Button m_btnBag;

	public bool canEdit = true;

	public bool isEditing;

	public zbHomeEditRoom(zbHomeEditWnd _wndParent, zbHomeEditFloor _floor, int _x)
	{
		wndParent = _wndParent;
		floor = _floor;
		x = _x;
		y = floor.y;
	}

	public void OnEnable()
	{
		roomRootElement = UQueryExtensions.Q(floor.floorElement, "floor_room" + x, (string)null);
		roomBgElement = UQueryExtensions.Q(roomRootElement, "room_bg", (string)null);
		roomFgElement = UQueryExtensions.Q(roomRootElement, "room_fg", (string)null);
		roomFrameElement = UQueryExtensions.Q(roomRootElement, "room_frame", (string)null);
		m_spriteColor = UQueryExtensions.Q<VisualElement>(roomRootElement, "color", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>(roomRootElement, "icon", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(roomRootElement, "lb_level", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(roomRootElement, "lb_name", (string)null);
		m_btnNo = UQueryExtensions.Q<Button>(roomRootElement, "btn_no", (string)null);
		m_btnBag = UQueryExtensions.Q<Button>(roomRootElement, "btn_bag", (string)null);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnNo, (Action<ClickEvent>)OnNo, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBag, (Action<ClickEvent>)OnBag, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (zbHomeMgr.Instance.roomMgr.IsMustNull(x, y))
		{
			roomRootElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		RefreshRoomData();
		RefreshBackground();
		RefreshSize();
		RefreshRootVisible();
		RefreshSelect();
	}

	private void OnNo(ClickEvent evt)
	{
		if (roomData != null)
		{
			_ = zbSelf.Instance.homeEditHandler;
			if (!zbSelf.Instance.homeEditHandler.UndoLastEdit(roomData))
			{
				zbSelf.Instance.homeEditHandler.curEditRoomId = 0;
				zbSelf.Instance.homeEditHandler.ClearHistory(roomData.RoomId);
			}
		}
		wndParent.RefreshDetail();
	}

	private void OnBag(ClickEvent evt)
	{
		wndParent.m_roomAnim.MoveToBagAnim(this);
		zbSelf.Instance.homeEditHandler.AddToBag(roomData);
		wndParent.RefreshDetail();
	}

	public void RefreshRoomData()
	{
		roomData = null;
		foreach (HomelandRoomDetail room in zbSelf.Instance.homeEditHandler.roomList)
		{
			if (room.X == x && room.Y == y)
			{
				roomData = room;
				break;
			}
		}
	}

	public void RefreshBackground()
	{
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		if (roomData != null)
		{
			((TextElement)m_lbLevel).text = "Lv." + roomData.Level;
			zbHomeRoomTypeCsvRecord recordByKey = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(roomData.TypeId);
			if (recordByKey != null)
			{
				m_spriteColor.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((recordByKey.type != 3 || roomData.Level <= 0) ? 1 : 0));
				((TextElement)m_lbName).text = recordByKey.name;
				string roomBgIcon = GetRoomBgIcon(recordByKey.length);
				Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(roomBgIcon);
				if ((Object)(object)val != (Object)null)
				{
					roomBgElement.style.backgroundImage = StyleBackground.op_Implicit(val);
				}
				Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.iconEdit);
				if ((Object)(object)val2 != (Object)null)
				{
					roomFgElement.style.backgroundImage = StyleBackground.op_Implicit(val2);
				}
				Texture2D val3 = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.iconType);
				if ((Object)(object)val3 != (Object)null)
				{
					m_spriteIcon.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val3));
					m_spriteIcon.MarkDirtyRepaint();
				}
				zbUtil.SetElementGray(roomBgElement, recordByKey.move == 0);
				canEdit = ((recordByKey.move == 1 && roomData.Level > 0) ? true : false);
			}
			roomFgElement.style.visibility = StyleEnum<Visibility>.op_Implicit((Visibility)0);
		}
		else
		{
			string roomBgIcon2 = GetRoomBgIcon(1);
			Texture2D val4 = zbAssetLoader.Instance.LoadResouce<Texture2D>(roomBgIcon2);
			if ((Object)(object)val4 != (Object)null)
			{
				roomBgElement.style.backgroundImage = StyleBackground.op_Implicit(val4);
			}
			roomFgElement.style.visibility = StyleEnum<Visibility>.op_Implicit((Visibility)1);
			canEdit = false;
		}
	}

	public void RefreshRootVisible()
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		bool flag = true;
		if ((x == 2 || x == 5) && wndParent.GetRoomLength(x - 1, y) >= 2)
		{
			flag = false;
		}
		if ((x == 3 || x == 6) && (wndParent.GetRoomLength(x - 1, y) >= 2 || wndParent.GetRoomLength(x - 2, y) >= 3))
		{
			flag = false;
		}
		if (flag)
		{
			roomRootElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			roomRootElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	public void RefreshSize()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		Background backgroundImage = roomBgElement.resolvedStyle.backgroundImage;
		if ((Object)(object)((Background)(ref backgroundImage)).texture != (Object)null)
		{
			IStyle style = roomBgElement.style;
			backgroundImage = roomBgElement.resolvedStyle.backgroundImage;
			style.width = StyleLength.op_Implicit((float)((Texture)((Background)(ref backgroundImage)).texture).width);
			IStyle style2 = roomBgElement.style;
			backgroundImage = roomBgElement.resolvedStyle.backgroundImage;
			style2.height = StyleLength.op_Implicit((float)((Texture)((Background)(ref backgroundImage)).texture).height);
			IStyle style3 = roomFrameElement.style;
			backgroundImage = roomBgElement.resolvedStyle.backgroundImage;
			style3.width = StyleLength.op_Implicit((float)((Texture)((Background)(ref backgroundImage)).texture).width);
			IStyle style4 = roomFrameElement.style;
			backgroundImage = roomBgElement.resolvedStyle.backgroundImage;
			style4.height = StyleLength.op_Implicit((float)((Texture)((Background)(ref backgroundImage)).texture).height);
			IStyle style5 = ((VisualElement)m_lbName).style;
			backgroundImage = roomBgElement.resolvedStyle.backgroundImage;
			style5.width = StyleLength.op_Implicit((float)(((Texture)((Background)(ref backgroundImage)).texture).width - 60));
			ITransform transform = ((VisualElement)m_btnNo).transform;
			backgroundImage = roomBgElement.resolvedStyle.backgroundImage;
			transform.position = new Vector3((float)(((Texture)((Background)(ref backgroundImage)).texture).width / 2 - 85), -85f, 0f);
			ITransform transform2 = ((VisualElement)m_btnBag).transform;
			backgroundImage = roomBgElement.resolvedStyle.backgroundImage;
			transform2.position = new Vector3((float)(((Texture)((Background)(ref backgroundImage)).texture).width / 2), -85f, 0f);
			IStyle style6 = roomRootElement.style;
			backgroundImage = roomBgElement.resolvedStyle.backgroundImage;
			style6.width = StyleLength.op_Implicit((float)((Texture)((Background)(ref backgroundImage)).texture).width);
			IStyle style7 = roomRootElement.style;
			backgroundImage = roomBgElement.resolvedStyle.backgroundImage;
			style7.height = StyleLength.op_Implicit((float)((Texture)((Background)(ref backgroundImage)).texture).height);
		}
		backgroundImage = roomFgElement.resolvedStyle.backgroundImage;
		if ((Object)(object)((Background)(ref backgroundImage)).texture != (Object)null)
		{
			IStyle style8 = roomFgElement.style;
			backgroundImage = roomFgElement.resolvedStyle.backgroundImage;
			style8.width = StyleLength.op_Implicit((float)((Texture)((Background)(ref backgroundImage)).texture).width);
			IStyle style9 = roomFgElement.style;
			backgroundImage = roomFgElement.resolvedStyle.backgroundImage;
			style9.height = StyleLength.op_Implicit((float)((Texture)((Background)(ref backgroundImage)).texture).height);
			ITransform transform3 = roomFgElement.transform;
			backgroundImage = roomBgElement.resolvedStyle.backgroundImage;
			int width = ((Texture)((Background)(ref backgroundImage)).texture).width;
			backgroundImage = roomFgElement.resolvedStyle.backgroundImage;
			transform3.position = new Vector3((float)((width - ((Texture)((Background)(ref backgroundImage)).texture).width) / 2), roomFgElement.transform.position.y, roomFgElement.transform.position.z);
		}
	}

	public string GetRoomBgIcon(int length)
	{
		return $"ui/image/homeedit/frame_room{Mathf.Clamp(length, 1, 3)}.png";
	}

	public void RefreshSelect()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (roomData != null)
		{
			isEditing = zbSelf.Instance.homeEditHandler.curEditRoomId == roomData.RoomId;
		}
		else
		{
			isEditing = false;
		}
		roomFrameElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!isEditing));
	}

	public bool IsEditing()
	{
		return isEditing;
	}

	public void SetVisible(bool visible)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		roomFgElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!visible));
		roomFrameElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!visible || !isEditing) ? 1 : 0));
	}

	public void RefreshNextRoomFrame(zbHomeEditRoom nextRoom)
	{
		if (nextRoom != null && this == nextRoom)
		{
			wndParent.dummyRoom.SetNextRoomSize(roomFrameElement);
		}
	}
}
