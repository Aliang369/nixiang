using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFacilityPanel
{
	private zbFacilityWnd m_wndParent;

	private VisualElement m_RootElement;

	private VisualElement m_bgPick;

	private zbFacilityRoom[] m_roomList = new zbFacilityRoom[28];

	public zbFacilityPlayer[] m_playerList = new zbFacilityPlayer[4];

	private int m_lastStartRoomId;

	private int m_lastFacilityId;

	public void OnEnable(zbFacilityWnd wndParent, VisualElement root, GameObject gameObjectSceneRoot)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_bgPick = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_pick", (string)null);
		m_bgPick.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_wndParent.m_uiEvent.RegisterCallback<PointerDownEvent>(m_wndParent.hudRoot, (Action<PointerDownEvent>)OnPickRoom, "", (TrickleDown)0);
		Transform val = gameObjectSceneRoot.transform.Find("facilityRooms");
		for (int i = 0; i < 28; i++)
		{
			m_roomList[i] = ((Component)((Component)val).transform.Find("facilityRoom" + i)).GetComponent<zbFacilityRoom>();
		}
		for (int j = 0; j < 4; j++)
		{
			m_playerList[j] = ((Component)((Component)val).transform.Find("facilityPlayer" + j)).GetComponent<zbFacilityPlayer>();
		}
	}

	public void Refresh()
	{
		(int[], int) currentGirdSpec = zbSelf.Instance.facilityHandler.GetCurrentGirdSpec(m_wndParent.m_record, m_wndParent.m_facilityMapInfo.CurrentRoom);
		int[] item = currentGirdSpec.Item1;
		int item2 = currentGirdSpec.Item2;
		int num = item[0];
		int num2 = item[1];
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 7; j++)
			{
				int num3 = i + j * 4;
				if (i < num && j < num2)
				{
					int num4 = item2 + i + j * num;
					bool canExplore = zbSelf.Instance.facilityHandler.CanAroundRoomExplore(item, item2, num4);
					m_roomList[num3].Refresh(m_wndParent, this, num4, canExplore, m_lastStartRoomId != item2 || m_lastFacilityId != m_wndParent.m_facilityMapInfo.BaseInfo.FacilityId);
					((VisualElement)m_roomList[num3].roomHud).SendToBack();
					if (m_wndParent.m_facilityMapInfo.CurrentRoom == num4)
					{
						m_playerList[0].Refresh(m_roomList[num3], isCurrentRoom: true);
					}
				}
				else
				{
					m_roomList[num3].Refresh(m_wndParent, this, 0, canExplore: false, firstIn: false);
					((VisualElement)m_roomList[num3].roomHud).SendToBack();
				}
			}
		}
		m_lastStartRoomId = item2;
		m_lastFacilityId = m_wndParent.m_facilityMapInfo.BaseInfo.FacilityId;
		for (int k = 1; k < 4; k++)
		{
			((Component)m_playerList[k]).gameObject.SetActive(false);
		}
	}

	private void OnPickRoom(PointerDownEvent evt)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector2.op_Implicit(zbUtil.GetWorldPosByToolKitPosition(((EventBase)evt).target, Vector2.op_Implicit(((PointerEventBase<PointerDownEvent>)(object)evt).localPosition), zbCameraHelper.Instance.mainCamera));
		Collider2D val2 = Physics2D.OverlapPoint(new Vector2(val.x, val.y));
		if ((Object)(object)val2 != (Object)null)
		{
			zbFacilityRoom componentInParent = ((Component)val2).gameObject.GetComponentInParent<zbFacilityRoom>();
			if ((Object)(object)componentInParent != (Object)null)
			{
				componentInParent.OnClick();
			}
		}
	}
}
