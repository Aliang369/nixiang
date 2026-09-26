using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMineWarAwardHud : VisualElement
{
	public VisualElement m_Root;

	private VisualElement m_nodeProgress;

	private VisualElement m_spriteProgress;

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

	private MineWarPointDetail mineWarPointDetail;

	private float parentScale;

	public void Init()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		m_initOrthographicSize = zbWorldScaler.cameraOrthographicSize;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/mineWar/mineWarAwardHud.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		((VisualElement)this).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		m_Root = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "root", (string)null);
		m_btnMoney = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btnMoney", (string)null);
		m_nodeProgress = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progress_bg", (string)null);
		m_spriteProgress = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progress", (string)null);
		((VisualElement)this).SendToBack();
	}

	public void Free()
	{
		((VisualElement)this).RemoveFromHierarchy();
	}

	public void Refresh(MineWarPointDetail _mineWarPointDetail, zbMineWarPoint room)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		m_MineWarPoint = room;
		mineWarPointDetail = _mineWarPointDetail;
		if (_mineWarPointDetail == null)
		{
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		parentScale = ((Component)m_MineWarPoint.m_spriteRenderer).transform.localScale.x;
		if (mineWarPointDetail.PlayerId == 0)
		{
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else if (mineWarPointDetail.PlayerId != zbSelf.Instance.GetRoleId())
		{
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_nodeProgress.transform.scale = new Vector3(1f / parentScale, -1f / parentScale, 1f);
	}

	public Vector2 WorldToToolKit(Vector3 worldPos)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return RuntimePanelUtils.CameraTransformWorldToPanel(m_Root.panel, worldPos, zbCameraHelper.Instance.mainCamera);
	}

	public void Update()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_homeWnd == (Object)null)
		{
			m_homeWnd = zbUIMgr.Instance.GetWndById(EWndId.MineWar) as zbMineWarWnd;
			if ((Object)(object)m_homeWnd != (Object)null && m_homeWnd.hudRoot != null)
			{
				m_homeWnd.m_homeMap.m_eleMineWarBg.Add((VisualElement)(object)this);
				m_homeWnd.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMoney, (Action<ClickEvent>)OnGetAward, "", (TrickleDown)0);
			}
		}
		if (m_Root.panel == null || (Object)(object)m_MineWarPoint == (Object)null || ((VisualElement)this).parent == null || (Object)(object)((Component)m_MineWarPoint).gameObject == (Object)null || !((Component)m_MineWarPoint).gameObject.activeSelf)
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
		float num = Math.Min(zbSelf.Instance.timeHandler.GetServerTime() - mineWarPointDetail.StartTm, mineWarPointDetail.FullTm - mineWarPointDetail.StartTm);
		if (num > 300f)
		{
			if (((VisualElement)this).style.display != StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
			{
				((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			float num2 = mineWarPointDetail.FullTm - mineWarPointDetail.StartTm;
			m_spriteProgress.style.height = StyleLength.op_Implicit(new Length(num / num2 * 72f, (LengthUnit)0));
		}
		else if (((VisualElement)this).style.display != StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1))
		{
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
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

	private void OnGetAward(ClickEvent evt)
	{
		if (mineWarPointDetail != null)
		{
			zbWsMsgHandler.Instance.MineWar_IdleAward_Req(mineWarPointDetail.PointId);
		}
	}

	public zbMineWarAwardHud()
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
