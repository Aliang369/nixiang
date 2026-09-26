using System;
using System.Collections;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFacilityRoomHud : VisualElement
{
	public VisualElement m_Root;

	private SpineVisualElement m_spineExplore;

	private Label m_lbTili;

	public VisualElement m_nodePlayer;

	private VisualElement m_spriteHead;

	private VisualElement m_spriteFrame;

	private Label m_lbLevel;

	private Button m_btnPlayer;

	private zbFacilityRoom m_Room;

	private zbFacilityWnd m_facilityWnd;

	private Vector2 m_lastPos;

	private const float m_positionThreshold = 0.001f;

	private const float HUD_WIDTH = 174f;

	private const float HUD_HEIGHT = 109f;

	private float _lastLeft = -1f;

	private float _lastTop = -1f;

	private bool m_isAdded;

	private bool m_isProgressing;

	public bool isVisible;

	public void Init(zbFacilityRoom room)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		m_Room = room;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/facility/facilityRoomHud.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		((VisualElement)this).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		m_Root = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "root", (string)null);
		m_spineExplore = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)this, "spine_explore", (string)null);
		m_lbTili = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_tili", (string)null);
		m_nodePlayer = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "node_player", (string)null);
		m_nodePlayer.userData = this;
		m_spriteHead = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "head", (string)null);
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "frame", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_btnPlayer = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_player", (string)null);
		((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Free()
	{
		((VisualElement)this).RemoveFromHierarchy();
	}

	public void SetVisible(bool visible)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		isVisible = visible;
		((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!visible));
	}

	public void Refresh(zbFacilityWnd parentWnd)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		if (m_Room.roomData.RoomState == EFacilityRoomState.Locked)
		{
			((VisualElement)m_spineExplore).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (parentWnd.m_facilityMapInfo.BaseInfo.FacilityCreator.RoleId != zbSelf.Instance.GetRoleId() && zbSelf.Instance.facilityHandler.GetLeftShareRoomCount(parentWnd.m_record, parentWnd.m_facilityMapInfo.BaseInfo) <= 0)
			{
				m_spineExplore.AnimationState.SetAnimation(0, "idle_gray", true);
			}
			else
			{
				m_spineExplore.AnimationState.SetAnimation(0, "idle", true);
			}
		}
		else
		{
			((VisualElement)m_spineExplore).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		((TextElement)m_lbTili).text = zbSelf.Instance.facilityHandler.tiliCost.ToString();
		((VisualElement)m_lbTili).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_Room.roomData.RoomState != EFacilityRoomState.Locked || m_Room.roomData.RoomType == EFacilityRoomType.EntranceStair) ? 1 : 0));
		RefreshPlayer();
	}

	private void RefreshPlayer()
	{
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)m_Room == (Object)null) && m_Room.roomData != null && zbSelf.Instance.facilityHandler.facilityMapInfo != null)
		{
			if (m_Room.roomData.RoomType == EFacilityRoomType.Monster && m_Room.roomData.RoomRobot != null)
			{
				m_nodePlayer.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_btnPlayer).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_lbLevel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				zbUtil.InitPlayerInfo(m_spriteHead, m_Room.roomData.RoomRobot.HeadId, m_spriteFrame, m_Room.roomData.RoomRobot.HeadFrameId, m_lbLevel, m_Room.roomData.RoomRobot.Level);
				m_nodePlayer.style.top = StyleLength.op_Implicit(-10f);
				m_nodePlayer.style.left = StyleLength.op_Implicit(22f);
			}
			else if (m_Room.roomData.RoomId != zbSelf.Instance.facilityHandler.facilityMapInfo.CurrentRoom && m_Room.roomData.RoomOnline != null && m_Room.roomData.RoomOnline.RoleId != zbSelf.Instance.GetRoleId())
			{
				m_nodePlayer.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_btnPlayer).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_lbLevel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				zbUtil.InitPlayerInfo(m_spriteHead, m_Room.roomData.RoomOnline.HeadId, m_spriteFrame, m_Room.roomData.RoomOnline.HeadFrameId, m_lbLevel, m_Room.roomData.RoomOnline.Level);
				m_nodePlayer.style.top = StyleLength.op_Implicit(-28f);
				m_nodePlayer.style.left = StyleLength.op_Implicit(-27f);
			}
			else
			{
				m_nodePlayer.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
	}

	public void PlayProgressAnim(Action<bool> callback)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (!m_isProgressing)
		{
			m_isProgressing = true;
			((VisualElement)m_lbTili).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_spineExplore).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_spineExplore.AnimationState.SetAnimation(0, "action", false);
			((MonoBehaviour)m_facilityWnd).StartCoroutine(SpineAnimation(callback));
		}
	}

	private IEnumerator SpineAnimation(Action<bool> callback)
	{
		yield return (object)new WaitForSeconds(1f);
		m_isProgressing = false;
		callback?.Invoke(obj: true);
	}

	public void OnPlayer()
	{
		if (m_Room.roomData.RoomOnline != null)
		{
			zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_Room.roomData.RoomOnline.RoleId);
		}
	}

	public Vector2 WorldToToolKit(Vector3 worldPos)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return RuntimePanelUtils.CameraTransformWorldToPanel(m_Root.panel, worldPos, zbCameraHelper.Instance.mainCamera);
	}

	public void Update()
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		if (!m_isAdded)
		{
			TryAddToHudRoot();
		}
		if (!m_isAdded)
		{
			return;
		}
		if (m_Root.panel == null || (Object)(object)m_Room == (Object)null || ((VisualElement)this).parent == null || !((Component)m_Room).gameObject.activeSelf || m_Room.roomData == null || m_Room.roomData.RoomType <= EFacilityRoomType.EmptyValue || !isVisible)
		{
			zbUtil.SetDisplayIfChanged((VisualElement)(object)this, (DisplayStyle)1);
			return;
		}
		zbUtil.SetDisplayIfChanged((VisualElement)(object)this, (DisplayStyle)0);
		Vector2 val = Vector2.op_Implicit(((Component)zbCameraHelper.Instance.mainCamera).transform.position);
		_ = zbCameraHelper.Instance.mainCamera.orthographicSize;
		if (Vector2.Distance(val, m_lastPos) > 0.001f)
		{
			m_lastPos = val;
			UpdateHudTransform();
		}
	}

	private void TryAddToHudRoot()
	{
		if (m_facilityWnd == null)
		{
			m_facilityWnd = zbUIMgr.Instance.GetWndById(EWndId.Facility) as zbFacilityWnd;
		}
		if (!((Object)(object)m_facilityWnd == (Object)null) && m_facilityWnd.hudRoot != null)
		{
			m_facilityWnd.hudRoot.Add((VisualElement)(object)this);
			m_facilityWnd.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPlayer, (Action)OnPlayer, "", (TrickleDown)0);
			m_isAdded = true;
			UpdateHudTransform();
		}
	}

	private void UpdateHudTransform()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = WorldToToolKit(((Component)m_Room).transform.position);
		Rect worldBound = m_facilityWnd.hudRoot.worldBound;
		Vector2 position = ((Rect)(ref worldBound)).position;
		Vector2 val2 = val - position;
		float newValue = val2.x - 87f;
		float newValue2 = val2.y - 54.5f;
		zbUtil.SetStyleIfChanged(ref _lastLeft, newValue, delegate(float num)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			m_Root.style.left = StyleLength.op_Implicit(num);
		});
		zbUtil.SetStyleIfChanged(ref _lastTop, newValue2, delegate(float num)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			m_Root.style.top = StyleLength.op_Implicit(num);
		});
	}
}
