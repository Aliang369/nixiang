using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMineWarHud : VisualElement
{
	public VisualElement m_Root;

	private VisualElement m_Temp1;

	private VisualElement m_Temp2;

	private Label m_lbTime;

	private Label m_lbName;

	private zbMineWarPoint m_MineWarPoint;

	private float m_initOrthographicSize;

	private zbMineWarWnd m_homeWnd;

	public bool isVisible;

	private Vector2 m_lastPos;

	private float m_lastOrthoSize;

	private const float m_positionThreshold = 0.001f;

	private Vector3 _lastScale;

	private float _lastLeft;

	private float _lastTop;

	public Button m_btnMoney;

	private float parentScale;

	private MineWarPointDetail mineWarPointDetail;

	private bool lastVisible;

	public void Init(zbMineWarPoint room)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		m_initOrthographicSize = zbWorldScaler.cameraOrthographicSize;
		m_MineWarPoint = room;
		parentScale = ((Component)m_MineWarPoint.m_spriteRenderer).transform.localScale.x;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/mineWar/mineWarHud.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		((VisualElement)this).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		m_Root = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "root", (string)null);
		m_Temp1 = (VisualElement)(object)UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_temp1", (string)null);
		m_Temp2 = (VisualElement)(object)UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_temp2", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_Time", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_btnMoney = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btnMoney", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		((VisualElement)this).SendToBack();
	}

	public void Free()
	{
		((VisualElement)this).RemoveFromHierarchy();
	}

	public void Refresh(MineWarPointDetail _mineWarPointDetail)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		mineWarPointDetail = _mineWarPointDetail;
		if (mineWarPointDetail.PlayerId == 0)
		{
			m_Temp2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			m_Temp2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbName).text = mineWarPointDetail.PlayerName;
		}
		if (mineWarPointDetail.ProtectTm > zbSelf.Instance.timeHandler.GetServerTime() || mineWarPointDetail.LockedTm > zbSelf.Instance.timeHandler.GetServerTime() || mineWarPointDetail.DestroyTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			lastVisible = true;
		}
		else
		{
			lastVisible = false;
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
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_homeWnd == (Object)null)
		{
			m_homeWnd = zbUIMgr.Instance.GetWndById(EWndId.MineWar) as zbMineWarWnd;
			if ((Object)(object)m_homeWnd != (Object)null && m_homeWnd.hudRoot != null)
			{
				m_homeWnd.hudRoot.Add((VisualElement)(object)this);
			}
		}
		if (m_Root.panel == null || (Object)(object)m_MineWarPoint == (Object)null || ((VisualElement)this).parent == null || !((Component)m_MineWarPoint).gameObject.activeSelf)
		{
			zbUtil.SetDisplayIfChanged((VisualElement)(object)this, (DisplayStyle)1);
			return;
		}
		zbUtil.SetDisplayIfChanged((VisualElement)(object)this, (DisplayStyle)0);
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
		if (mineWarPointDetail == null)
		{
			return;
		}
		if (mineWarPointDetail.ProtectTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			if (m_Temp1.style.display != StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
			{
				m_Temp1.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			((TextElement)m_lbTime).text = zbUtil.FormatTimeSmart(mineWarPointDetail.ProtectTm - zbSelf.Instance.timeHandler.GetServerTime());
			return;
		}
		if (mineWarPointDetail.LockedTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			if (m_Temp1.style.display != StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
			{
				m_Temp1.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			((TextElement)m_lbTime).text = zbUtil.FormatTimeSmart(mineWarPointDetail.LockedTm - zbSelf.Instance.timeHandler.GetServerTime());
			return;
		}
		if (mineWarPointDetail.DestroyTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			if (m_Temp1.style.display != StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
			{
				m_Temp1.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			((TextElement)m_lbTime).text = zbUtil.FormatTimeSmart(mineWarPointDetail.DestroyTm - zbSelf.Instance.timeHandler.GetServerTime());
			return;
		}
		if (lastVisible)
		{
			if (zbSelf.Instance.mineWarHandler.currServerId == zbSelf.Instance.GetServerId())
			{
				zbWsMsgHandler.Instance.MineWar_Data_Req();
			}
			else
			{
				zbWsMsgHandler.Instance.CrossMine_Data_Req(zbSelf.Instance.mineWarHandler.currServerId);
			}
			lastVisible = false;
		}
		if (m_Temp1.style.display != StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1))
		{
			m_Temp1.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void UpdateHudTransform()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		float orthographicSize = zbCameraHelper.Instance.mainCamera.orthographicSize;
		float num = m_initOrthographicSize / orthographicSize * parentScale;
		Vector3 newScale = Vector3.one * num;
		zbUtil.SetScaleIfChanged(ref _lastScale, newScale, m_Root);
		float num2 = 266f * num;
		float num3 = 190f * num;
		Vector2 val = WorldToToolKit(((Component)m_MineWarPoint).transform.position);
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

	public zbMineWarHud()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		m_initOrthographicSize = 8.4f;
		_lastScale = Vector3.zero;
		_lastLeft = -1f;
		_lastTop = -1f;
		((VisualElement)this)._002Ector();
	}
}
