using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFloorHud : VisualElement
{
	private VisualElement m_Root;

	private Label m_lbLimit;

	private VisualElement m_progress;

	private Label m_lbProgress;

	public zbFloor m_floor;

	private float m_initOrthographicSize;

	private zbHomeWnd m_homeWnd;

	public bool isVisible;

	private Vector2 m_lastPos;

	private float m_lastOrthoSize;

	private const float m_positionThreshold = 0.001f;

	private Vector3 _lastScale;

	private float _lastWidth;

	private float _lastHeight;

	private float _lastLeft;

	private float _lastTop;

	public void Init(zbFloor floor)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		m_floor = floor;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/home/floorHud.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		((VisualElement)this).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		m_Root = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "root", (string)null);
		m_lbLimit = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_limit", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progress", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_progress", (string)null);
		((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		((VisualElement)this).SendToBack();
	}

	public void Free()
	{
		((VisualElement)this).RemoveFromHierarchy();
	}

	public void SetVisible(bool visible)
	{
		isVisible = visible;
	}

	public void Refresh()
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		m_initOrthographicSize = zbWorldScaler.cameraOrthographicSize;
		if (zbSelf.Instance.homelandHandler.GetFloorState(m_floor.floorIndex) != EActiveState.Pending || m_floor.floorIndex < zbSelf.Instance.homelandHandler.GetNextActiveFloor())
		{
			SetVisible(visible: false);
			return;
		}
		SetVisible(visible: true);
		zbHomeFloorCsvRecord recordByKey = zbDataConfig.Instance.homeFloorCsvTable.GetRecordByKey(m_floor.floorIndex);
		if (recordByKey != null)
		{
			((TextElement)m_lbProgress).text = zbServerData.sGameData.PeerageInfo.PeerageId + "/" + recordByKey.peerageId;
			float num = Mathf.Clamp((float)zbServerData.sGameData.PeerageInfo.PeerageId / (float)recordByKey.peerageId, 0f, 1f);
			m_progress.style.width = StyleLength.op_Implicit(new Length(num * 388f, (LengthUnit)0));
			zbPeerageCsvRecord recordByKey2 = zbDataConfig.Instance.peerageCsvTable.GetRecordByKey(recordByKey.peerageId);
			if (recordByKey2 != null)
			{
				((TextElement)m_lbLimit).text = zbUtil.Language("lang.10290") + recordByKey2.name;
			}
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
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_homeWnd == (Object)null)
		{
			m_homeWnd = zbUIMgr.Instance.GetWndById(EWndId.Home) as zbHomeWnd;
			if ((Object)(object)m_homeWnd != (Object)null && m_homeWnd.hudRoot != null)
			{
				m_homeWnd.hudRoot.Add((VisualElement)(object)this);
			}
		}
		if (m_Root.panel == null || (Object)(object)m_floor == (Object)null || !isVisible)
		{
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
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
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		float orthographicSize = zbCameraHelper.Instance.mainCamera.orthographicSize;
		float num = m_initOrthographicSize / orthographicSize;
		Vector3 newScale = Vector3.one * num;
		zbUtil.SetScaleIfChanged(ref _lastScale, newScale, m_Root);
		float num2 = 2280f * num;
		float num3 = 220f * num;
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
		Vector2 val = WorldToToolKit(((Component)m_floor).transform.position);
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

	public zbFloorHud()
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
