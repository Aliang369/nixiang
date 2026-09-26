using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarHud : VisualElement
{
	public VisualElement m_Root;

	private Label m_lblName;

	private float m_initOrthographicSize;

	public bool isVisible;

	private Vector2 m_lastPos;

	private float m_lastOrthoSize;

	private const float m_positionThreshold = 0.001f;

	private Vector3 _lastScale;

	private float _lastLeft;

	private float _lastTop;

	public Button m_btnMoney;

	private float parentScale;

	private VisualElement m_rootThis;

	private VisualElement m_rootHud;

	private zbLegionWarPoint m_legionWarPoint;

	private VisualElement m_biaoji;

	public zbLegionWarHud(VisualElement root, zbLegionWarPoint legionWarPoint)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		m_initOrthographicSize = 8.4f;
		m_lastPos = Vector2.left * 10f;
		_lastScale = Vector3.zero;
		_lastLeft = -1f;
		_lastTop = -1f;
		parentScale = 1f;
		((VisualElement)this)._002Ector();
		m_rootHud = root;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/legionWar/legionWarHud.uxml");
		m_rootThis = (VisualElement)(object)val.Instantiate();
		m_rootHud.Add(m_rootThis);
		m_legionWarPoint = legionWarPoint;
	}

	public void Init()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_initOrthographicSize = zbWorldScaler.cameraOrthographicSize;
		((VisualElement)this).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		m_Root = UQueryExtensions.Q<VisualElement>(m_rootThis, "nodeCenter", (string)null);
		m_lblName = UQueryExtensions.Q<Label>(m_rootThis, "lb_name", (string)null);
		m_biaoji = UQueryExtensions.Q<VisualElement>(m_rootThis, "biaoJi", (string)null);
		zbUIEffect.CreateScaleAction(m_biaoji, 1.5f, 1.7249999f, 1.5f);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		((VisualElement)this).SendToBack();
	}

	public void Free()
	{
		((VisualElement)this).RemoveFromHierarchy();
	}

	public void Refresh(LegionWarPointForceDetail[] pointForceList)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		zbUtil.SetDisplayIfChanged(m_biaoji, (DisplayStyle)1);
		if (zbSelf.Instance.legionWarHandler.legionWarLegionInfo.MarkPointId > 0 && zbSelf.Instance.legionWarHandler.legionWarLegionInfo.MarkPointId == m_legionWarPoint.legionWarPointStateDetail.PointId)
		{
			zbUtil.SetDisplayIfChanged(m_biaoji, (DisplayStyle)0);
		}
		if (m_legionWarPoint.legionWarPointStateDetail.PointId == 1001 || m_legionWarPoint.legionWarPointStateDetail.PointId == 2001)
		{
			m_Root.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			m_Root.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		((TextElement)m_lblName).text = m_legionWarPoint.legionWarPointStateDetail.PointId.ToString();
		float redOccupyPct = 0f;
		float blueOccupyPct = 0f;
		foreach (LegionWarPointForceDetail legionWarPointForceDetail in pointForceList)
		{
			if (legionWarPointForceDetail.ForceId == ELegionWarForce.Red)
			{
				redOccupyPct = legionWarPointForceDetail.OccupyPct;
			}
			else
			{
				blueOccupyPct = legionWarPointForceDetail.OccupyPct;
			}
		}
		zbSelf.Instance.legionWarHandler.SetBluntProgress(m_Root, redOccupyPct, blueOccupyPct, zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId, "process_84x12_", 84, ".jpg");
		((VisualElement)m_lblName).schedule.Execute((Action)delegate
		{
			UpdateHudTransform();
		});
	}

	public Vector2 WorldToToolKit(Vector3 worldPos)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return RuntimePanelUtils.CameraTransformWorldToPanel(m_Root.panel, worldPos, zbCameraHelper.Instance.mainCamera);
	}

	public void Update()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		if (m_Root.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1) || m_Root.panel == null || (Object)(object)m_legionWarPoint == (Object)null || !((Component)m_legionWarPoint).gameObject.activeSelf)
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
	}

	private void UpdateHudTransform()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		float orthographicSize = zbCameraHelper.Instance.mainCamera.orthographicSize;
		float num = m_initOrthographicSize / orthographicSize * parentScale;
		Vector3 newScale = Vector3.one * num;
		zbUtil.SetScaleIfChanged(ref _lastScale, newScale, m_Root);
		float num2 = 250f * num;
		Vector2 val = WorldToToolKit(((Component)m_legionWarPoint).transform.position);
		Rect worldBound = m_rootHud.worldBound;
		Vector2 position = ((Rect)(ref worldBound)).position;
		Vector2 val2 = val - position;
		float x = val2.x;
		float num3 = val2.y - num2 / 2f;
		if (m_legionWarPoint.legionWarMapRecord.spineName == "animation/ui/judian/jianzhu_1/jianzhu_1")
		{
			num3 += 70f * num;
		}
		zbUtil.SetStyleIfChanged(ref _lastLeft, x, delegate(float num4)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			m_Root.style.left = StyleLength.op_Implicit(num4);
		});
		zbUtil.SetStyleIfChanged(ref _lastTop, num3, delegate(float num4)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			m_Root.style.top = StyleLength.op_Implicit(num4);
		});
	}
}
