using UnityEngine;
using UnityEngine.UIElements;

public class zbHomeEditDummyRoom
{
	public zbHomeEditWnd wndParent;

	public VisualElement dummyRootElement;

	public VisualElement dummyFgElement;

	public zbHomeEditRoom proxyRoom;

	public zbHomeEditDummyRoom(zbHomeEditWnd _wndParent)
	{
		wndParent = _wndParent;
	}

	public void OnEnable()
	{
		dummyRootElement = UQueryExtensions.Q(wndParent.GetRootElement(), "room_dummy", (string)null);
		dummyFgElement = UQueryExtensions.Q(dummyRootElement, "room_fg", (string)null);
		SetVisible(visible: false);
	}

	public void BeginMove(zbHomeEditRoom _proxyRoom, Vector3 mousePos)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		proxyRoom = _proxyRoom;
		IStyle style = dummyFgElement.style;
		Background backgroundImage = proxyRoom.roomFgElement.resolvedStyle.backgroundImage;
		style.backgroundImage = StyleBackground.op_Implicit(((Background)(ref backgroundImage)).texture);
		IStyle style2 = dummyFgElement.style;
		backgroundImage = proxyRoom.roomFgElement.resolvedStyle.backgroundImage;
		style2.width = StyleLength.op_Implicit((float)((Texture)((Background)(ref backgroundImage)).texture).width);
		IStyle style3 = dummyFgElement.style;
		backgroundImage = proxyRoom.roomFgElement.resolvedStyle.backgroundImage;
		style3.height = StyleLength.op_Implicit((float)((Texture)((Background)(ref backgroundImage)).texture).height);
		SetDragPostion(mousePos);
		SetVisible(visible: true);
	}

	public void DragMove(Vector3 mousePos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		SetDragPostion(mousePos);
		wndParent.ResfeshNextLocateRoom(mousePos);
	}

	public void EndMove()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		SetVisible(visible: false);
		wndParent.ResfeshNextLocateRoom(new Vector3(0f, 0f, 0f));
	}

	public void SetDragPostion(Vector3 mousePos)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		dummyRootElement.transform.scale = wndParent.zoomContent.transform.scale;
		dummyRootElement.transform.position = mousePos;
	}

	public void SetNextRoomSize(VisualElement nextRoom)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		nextRoom.style.height = StyleLength.op_Implicit(146f);
		zbHomeEditRoom obj = proxyRoom;
		object obj2;
		Background backgroundImage;
		if (obj == null)
		{
			obj2 = null;
		}
		else
		{
			VisualElement roomFgElement = obj.roomFgElement;
			if (roomFgElement == null)
			{
				obj2 = null;
			}
			else
			{
				IResolvedStyle resolvedStyle = roomFgElement.resolvedStyle;
				if (resolvedStyle == null)
				{
					obj2 = null;
				}
				else
				{
					backgroundImage = resolvedStyle.backgroundImage;
					obj2 = ((Background)(ref backgroundImage)).texture;
				}
			}
		}
		if ((Object)obj2 != (Object)null)
		{
			backgroundImage = proxyRoom.roomFgElement.resolvedStyle.backgroundImage;
			float num = ((Texture)((Background)(ref backgroundImage)).texture).width;
			if (num >= 690f)
			{
				nextRoom.style.width = StyleLength.op_Implicit(710f);
			}
			else if (num >= 450f)
			{
				nextRoom.style.width = StyleLength.op_Implicit(470f);
			}
			else if (num >= 210f)
			{
				nextRoom.style.width = StyleLength.op_Implicit(230f);
			}
		}
	}

	public void SetVisible(bool visible)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (visible)
		{
			dummyRootElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			dummyRootElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}
}
