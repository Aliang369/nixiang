using UnityEngine;
using UnityEngine.UIElements;

public class zbRoomHud : VisualElement
{
	public VisualElement m_Root;

	private VisualElement m_spriteColor;

	private VisualElement m_spriteIcon;

	private Label m_lbLevel;

	private VisualElement m_spriteUp;

	private zbRoom m_Room;

	private float m_initOrthographicSize;

	private zbHomeWnd m_homeWnd;

	public bool isVisible;

	private bool m_isRequired;

	private Vector2 m_lastPos;

	private float m_lastOrthoSize;

	private const float m_positionThreshold = 0.001f;

	private Vector3 _lastScale;

	private float _lastWidth;

	private float _lastHeight;

	private float _lastLeft;

	private float _lastTop;

	public void SetVisible(bool visible)
	{
		isVisible = visible;
	}

	public void Init(zbRoom room)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		m_Room = room;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/home/roomHud.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		((VisualElement)this).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		m_Root = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "root", (string)null);
		m_spriteColor = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "color", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_spriteUp = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "up", (string)null);
		m_homeWnd = zbUIMgr.Instance.GetWndById(EWndId.Home) as zbHomeWnd;
		m_homeWnd.hudRoot.Add((VisualElement)(object)this);
		((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		((VisualElement)this).SendToBack();
		UpdateHudTransform();
	}

	public void Free()
	{
		((VisualElement)this).RemoveFromHierarchy();
	}

	public void Refresh()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		SetVisible(m_Room.roomRecord.type == 3);
		m_initOrthographicSize = zbWorldScaler.cameraOrthographicSize;
		m_spriteUp.style.left = StyleLength.op_Implicit(m_Room.GetRoomWidth() * 100f - 32f);
		if (m_Room.roomData.Level > 0)
		{
			m_isRequired = true;
			m_spriteColor.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbLevel).text = "Lv." + m_Room.roomData.Level;
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(m_Room.roomRecord.iconType);
			if ((Object)(object)val != (Object)null)
			{
				m_spriteIcon.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
				m_spriteIcon.MarkDirtyRepaint();
			}
		}
		else
		{
			m_isRequired = false;
			m_spriteColor.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		RefreshRoomState();
	}

	private void RefreshRoomState()
	{
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		if (m_Room.roomData.Level == 0 && zbSelf.Instance.homelandHandler.CanRoomUpgrade(m_Room.roomData) && !m_Room.IsFightRoom())
		{
			m_spriteUp.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_spriteUp.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/home/room_repair.png"));
		}
		else if (!m_Room.IsFightRoom() && m_Room.roomData.Level > 0 && zbSelf.Instance.homelandHandler.CanRoomUpgrade(m_Room.roomData))
		{
			m_spriteUp.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_spriteUp.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/common/arrow_up.png"));
		}
		else
		{
			m_spriteUp.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
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
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		if (m_Root.panel == null || (Object)(object)m_Room == (Object)null || ((VisualElement)this).parent == null || !((Component)m_Room).gameObject.activeSelf || zbSelf.Instance.homeEditHandler.createRoomRecord != null)
		{
			zbUtil.SetDisplayIfChanged((VisualElement)(object)this, (DisplayStyle)1);
			return;
		}
		zbUtil.SetDisplayIfChanged((VisualElement)(object)this, (DisplayStyle)0);
		zbUtil.SetDisplayIfChanged(m_spriteColor, (DisplayStyle)((!isVisible || !m_isRequired) ? 1 : 0));
		Vector2 val = Vector2.op_Implicit(((Component)zbCameraHelper.Instance.mainCamera).transform.position);
		float orthographicSize = zbCameraHelper.Instance.mainCamera.orthographicSize;
		bool flag = false;
		if (Vector2.Distance(val, m_lastPos) > 0.001f || !Mathf.Approximately(orthographicSize, m_lastOrthoSize))
		{
			m_lastPos = val;
			m_lastOrthoSize = orthographicSize;
			flag = true;
		}
		if (flag)
		{
			UpdateHudTransform();
		}
	}

	private void UpdateHudTransform()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		float orthographicSize = zbCameraHelper.Instance.mainCamera.orthographicSize;
		float num = m_initOrthographicSize / orthographicSize;
		Vector3 newScale = Vector3.one * num;
		zbUtil.SetScaleIfChanged(ref _lastScale, newScale, m_Root);
		float num2 = m_Room.GetRoomWidth() * 100f * num;
		float num3 = m_Room.GetRoomHeight() * 100f * num;
		zbUtil.SetStyleIfChanged(ref _lastWidth, num2, delegate(float num4)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			m_Root.style.width = StyleLength.op_Implicit(num4);
		});
		zbUtil.SetStyleIfChanged(ref _lastHeight, num3, delegate(float num4)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			m_Root.style.height = StyleLength.op_Implicit(num4);
		});
		Vector2 val = WorldToToolKit(((Component)m_Room).transform.position);
		Rect worldBound = m_homeWnd.hudRoot.worldBound;
		Vector2 position = ((Rect)(ref worldBound)).position;
		Vector2 val2 = val - position;
		float newValue = val2.x - num2 / 2f;
		float newValue2 = val2.y - num3 / 2f;
		zbUtil.SetStyleIfChanged(ref _lastLeft, newValue, delegate(float num4)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			m_Root.style.left = StyleLength.op_Implicit(num4);
		});
		zbUtil.SetStyleIfChanged(ref _lastTop, newValue2, delegate(float num4)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			m_Root.style.top = StyleLength.op_Implicit(num4);
		});
	}

	public zbRoomHud()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		m_initOrthographicSize = 5f;
		_lastScale = Vector3.zero;
		_lastWidth = -1f;
		_lastHeight = -1f;
		_lastLeft = -1f;
		_lastTop = -1f;
		((VisualElement)this)._002Ector();
	}
}
